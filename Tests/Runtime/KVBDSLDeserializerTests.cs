using KH.Console;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KH.KVBDSL {
    public class KVBDSLDeserializerTests  {
        [Test]
        public void TestUnquotedString() {
            AssertString("foo", "foo");
            AssertString("foo", "foo ");
            AssertString("foo", " foo ");
        }

        [Test]
        public void TestQuotedString() {
            AssertQuotedString("", "");
            AssertQuotedString(" ", " ");
            AssertQuotedString("foo ", "foo ");
            AssertQuotedString(" foo ", " foo ");
            AssertQuotedString(" foo", " foo");

            // Test string key with no type but opening ".
            SimpleDictAssert("key:\"foo\"", "key", "foo");
        }

        [Test]
        public void TestMLString() {
            // TODO:
            Assert.Fail();
        }

        [Test]
        public void TestInt() {
            AssertInt(-1, "-1");
            AssertInt(1, "1");
            AssertInt(1, "+1");
            AssertInt(1000, "1000");
            AssertInt(1000, " 1000");
            AssertInt(1000, "1000 ");

            AssertBadParse("i 100.5");
            AssertBadParse("i foo");
        }

        [Test]
        public void TestFloat() {
            AssertFloat(-1, "-1");
            AssertFloat(1, "1");
            AssertFloat(1, "+1");
            AssertFloat(1000, "1000");
            AssertFloat(1000, " 1000");
            AssertFloat(1000, "1000 ");
            AssertFloat(1000.5f, "1000.5 ");

            AssertBadParse("f 100,5");
            AssertBadParse("f foo");
        }

        [Test]
        public void TestBool() {
            AssertBool(true, "true");
            AssertBool(true, "TRUE");
            AssertBool(false, "false");
            AssertBool(false, "FALSE");

            AssertBadParse("b 1");
            AssertBadParse("b 0");
            AssertBadParse("b yes");
            AssertBadParse("b no");
        }

        [Test]
        public void TestArray() {
            Assert.Fail();
        }

        [Test]
        public void TestDictionary() {
            Assert.Fail();
        }

        [Test]
        public void TestKeyParse() {
            AssertKey("foo", "foo");
            AssertKey("foo", "foo ");
            AssertKey("foo", " foo");
            AssertKey("foo", " foo ");
            AssertKey("foo", "\"foo\"");
            AssertKey("foo ", "\"foo \"");
            AssertKey(" foo ", "\" foo \"");
            AssertKey(" foo", "\" foo\"");
            AssertKey("foo:bar", "\"foo:bar\"");
            AssertKey("\"foo\"", "\\\"foo\"");
            AssertKey("5\n3", "5\\n3");

            AssertBadKeyParse("\"foo");
            AssertBadKeyParse("foo:bar");
        }

        [Test]
        public void TestGeneral() {
            StringBuilder file = new StringBuilder();
            // This has a known fine type at the beginning and end, just in case
            // the otherwise first or last item has peculiarities if they're not
            // in the front or back of the parse string.
            file.AppendLine("key0:i 1");
            file.AppendLine("key1:s \"foo\"");
            file.AppendLine("key1b: s fooo");
            file.AppendLine("key2: \"bar\"");
            file.AppendLine("key3:i 43289");
            file.AppendLine("key4:f 43289.5");
            file.AppendLine("key5:b true");
            file.AppendLine("key999:i 1");
            Dictionary<string, object> expected = new Dictionary<string, object>();
            expected["key0"] = 1;
            expected["key1"] = "foo";
            expected["key1b"] = "fooo";
            expected["key2"] = "bar";
            expected["key3"] = 43289;
            expected["key4"] = 43289.5;
            expected["key5"] = true;
            expected["key999"] = 1;

            var actual = new Deserializer().ParseString(file.ToString());
            Assert.AreEqual(expected, actual);
        }

        private static void AssertBadKeyParse(string testKey) {
            Deserializer deserializer = new Deserializer();
            Assert.AreEqual(new Dictionary<string, object>(), deserializer.ParseString($"{testKey}:s foo"));
        }

        private static void AssertBadParse(string typeAndValue) {
            Deserializer deserializer = new Deserializer();
            Assert.AreEqual(new Dictionary<string, object>(), deserializer.ParseString($"foo:{typeAndValue}"));
        }

        private static void AssertKey(string expectedKey, string testKey) {
            SimpleDictAssert($"{testKey}: s 123", expectedKey, "123");
        }

        private static void AssertString(string expectedStr, string testStr) {
            SimpleDictAssert($"key:s {testStr}", "key", expectedStr);
        }

        private static void AssertInt(int expected, string testStr) {
            SimpleDictAssert($"key:i {testStr}", "key", expected);
        }

        private static void AssertFloat(float expected, string testStr) {
            SimpleDictAssert($"key:f {testStr}", "key", expected);
        }

        private static void AssertBool(bool expected, string testStr) {
            SimpleDictAssert($"key:b {testStr}", "key", expected);
        }

        private static void AssertQuotedString(string expectedStr, string testStr) {
            SimpleDictAssert($"key:s \"{testStr}\"", "key", expectedStr);
        }

        private static void SimpleDictAssert(string file, string key, object value) {
            Deserializer deserializer = new Deserializer();
            Dictionary<string, object> expected = new Dictionary<string, object>();
            expected[key] = value;
            Dictionary<string, object> actual = deserializer.ParseString(file);
            Assert.AreEqual(expected, actual);
        }
    }
}