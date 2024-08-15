using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace KH.KVBDSL {
    public class KVBDSLSerializerTests {
        CultureInfo _cachedCulture;

        [SetUp]
        public void Setup() {
            _cachedCulture = Thread.CurrentThread.CurrentCulture;
        }

        [TearDown]
        public void TearDown() {
            Thread.CurrentThread.CurrentCulture = _cachedCulture;
        }

        [Test]
        public void TestCommaSeparatorCulture() {
            // France uses ',' as a decimal separator.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

            // Verify that it's the culture is used by default and formats as ','.
            Assert.AreEqual("1,5", 1.5f.ToString());

            // Verify that the serializer uses '.' regardless.
            AssertExpected("f 1.5", 1.5f);
        }

        [Test]
        public void TestGeneral() {
            Dictionary<string, object> values = new Dictionary<string, object>();
            values["key1"] = "foo";
            values["key2"] = 5;
            values["key3"] = false;
            values["key4"] = 5.5f;
            values["key5"] = GenerateList("foo", 5);
            values["key6"] = GenerateDict(("key6a", 5), ("key6b", 2.5f));

            Assert.AreEqual(BootlegMLS(
                "key1: s foo",
                "key2: i 5",
                "key3: b false",
                "key4: f 5.5",
                "key5: [",
                "s foo",
                "i 5",
                "]",
                "key6: {",
                "key6a: i 5",
                "key6b: f 2.5",
                "}",
                ""
            ), new Serializer().Serialize(values));
        }

        [Test]
        public void TestListWithUnsupportedValue() {
            // The seconds list entry will be omitted, as it's not a supported type.
            AssertExpected("[\ns foo\n]", GenerateList("foo", new Stack()));
        }

        [Test]
        public void TestDictWithUnsupportedValue() {
            // The seconds list entry will be omitted, as it's not a supported type.
            AssertExpected("{\nk1: s foo\n}", GenerateDict(("k1", "foo"), ("k2", new Stack())));
        }

        [Test]
        public void TestGenericOuterDictionary() {
            Dictionary<string, string> values = new Dictionary<string, string>();
            values["key1"] = "foo";
            string result = new Serializer().Serialize(values);
            Assert.AreEqual("key1: s foo\n", result);
        }

        [Test]
        public void TestGenericInnerDictionary() {
            var values = new Dictionary<string, object>();
            var inner = new Dictionary<string, int>();
            inner["key1a"] = 5;
            values["key1"] = inner;
            string result = new Serializer().Serialize(values);
            Assert.AreEqual("key1: {\nkey1a: i 5\n}\n", result);
        }

        [Test]
        public void TestNonGenericList() {
            var values = new Dictionary<string, object>();
            var inner = new List<int> {
                1,
                2
            };
            values["key1"] = inner;
            string result = new Serializer().Serialize(values);
            Assert.AreEqual("key1: [\ni 1\ni 2\n]\n", result);
        }

        [Test]
        public void TestIndentation() {
            var values = GenerateDict(("key1", GenerateList(GenerateDict(("key2", GenerateList(5))))));
            string result = new Serializer(Serializer.Options.Indent).Serialize(values);
            Assert.AreEqual(BootlegMLS(
                "key1: [",
                "  {",
                "    key2: [",
                "      i 5",
                "    ]",
                "  }",
                "]",
                ""
                ), result);
        }

        [Test]
        public void TestKeyEscaping() {
            AssertKey("foo", "foo");
            AssertKey("\"fo:o\"", "fo:o");
            AssertKey("\"\\\"foo\\\"\"", "\"foo\"");
            AssertKey("\"\\\"\\\\\\\b\\\f\\\n\\\r\\\t\\\v\"", "\"\\\b\f\n\r\t\v");
        }

        [Test]
        public void TestStringEscaping() {
            // Cases where unquoted strings are preferred.
            AssertExpected("s foo", "foo");
            AssertExpected("s foo\"bar", "foo\"bar");
            // Cases where quoted strings are preferred.
            AssertExpected("\"\"", "");
            AssertExpected("\" foo\"", " foo");
            AssertExpected("\"foo \"", "foo ");
            AssertExpected("\" foo \"", " foo ");
            AssertExpected("\" foo \"", " foo ");
            AssertExpected("\"\\\"foo\\\"\"", "\"foo\"");
            AssertExpected("\"\\\"\\\\\\\b\\\f\\\n\\\r\\\t\\\v\"", "\"\\\b\f\n\r\t\v", Serializer.Options.DisableMLS);
            // Cases where MLS is preferred.
            AssertExpected("\"\"\"\n1\n2\n\"\"\"", "1\n2");
            // Should preserve the whitespace after the 1 by placing in a \p.
            AssertExpected("\"\"\"\n1  \\p\n2\n\"\"\"", "1  \n2");
            // Should escape any three quotes in a row. Could be a bit smarter about it
            // by tactically encoding quotes, but that feels messier. Shouldn't come up
            // much anyway.
            AssertExpected("\"\"\"\n\n\\\"\\\"\\\"\"\"\n\"\"\"", "\n\"\"\"\"\"");
        }

        void AssertKey(string expectedEncoding, string key) {
            Dictionary<string, object> value = new Dictionary<string, object>();
            value[key] = true;
            Assert.AreEqual($"{expectedEncoding}: b true\n", new Serializer().Serialize(value));
        }

        void AssertExpectedWhole(string expected, Dictionary<string, object> value, Serializer.Options options = Serializer.Options.None) {
            Assert.AreEqual(expected, new Serializer(options).Serialize(value));
        }

        void AssertExpected(string expectedEncoding, object valueToEncode, Serializer.Options options = Serializer.Options.None) {
            Dictionary<string, object> value = new Dictionary<string, object>();
            value["key"] = valueToEncode;
            Assert.AreEqual($"key: {expectedEncoding}\n", new Serializer(options).Serialize(value));
        }

        private static List<object> GenerateList(params object[] arr) {
            return arr.ToList();
        }

        private static Dictionary<string, object> GenerateDict(params (string key, object value)[] values) {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (var value in values) {
                dict[value.key] = value.value;
            }
            return dict;
        }

        private static string BootlegMLS(params string[] lines) {
            return string.Join("\n", lines);
        }
    }
}