using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace KH.KVBDSL {
    public class KVBDSLDeserializerTests {
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
        public void TestSimpleStringTypes() {
            AssertValue("foo", "s foo");
            AssertValue("foo", "s \"foo\"");
            AssertValue("foo", "s \"\"\"\nfoo\"\"\"");
        }

        [Test]
        public void TestUnquotedString() {
            AssertString("foo", "foo");
            AssertString("foo", "foo ");
            AssertString("foo", " foo ");
            AssertString("foo\"", " foo\" ");
            AssertString("測試字串", "測試字串");
            // This it a '\' followed by a 't', _not_ an escape character.
            AssertString("\\t", "\\t");
        }

        [Test]
        public void TestQuotedString() {
            AssertQuotedString("", "");
            AssertQuotedString(" ", " ");
            AssertQuotedString("foo ", "foo ");
            AssertQuotedString(" foo ", " foo ");
            AssertQuotedString(" foo", " foo");
            AssertQuotedString("測試字串", "測試字串");

            // Test string key with no type but opening ".
            SimpleDictAssert("key:\"foo\"", "key", "foo");
        }

        [Test]
        public void TestMLString() {
            // Test method automatically adds """ on both ends.

            // Simple string, no modifications.
            AssertMLS("foo\nbar\nbaz", "foo\nbar\nbaz");
            // Simple string, leading and trailing newline cleared.
            AssertMLS("foo\nbar\nbaz", "foo\nbar\nbaz\n");
            // Simple string, leading spaces cleared.
            AssertMLS("foo\nbar\nbaz", BootlegMLS(
                "  foo",
                "  bar",
                "  baz",
                ""
            ));
            // Simple string, leading tabs cleared.
            AssertMLS("foo\nbar\nbaz", BootlegMLS(
                "\t\tfoo",
                "\t\tbar",
                "\t\tbaz",
                ""
            ));
            // Simple string, variable tabs.
            AssertMLS("foo\n\tbar\n\tbaz", BootlegMLS(
                "\tfoo",
                "\t\tbar",
                "\t\tbaz",
                ""
            ));
            // Simple string, variable spaces.
            AssertMLS("foo\n bar\n baz", BootlegMLS(
                " foo",
                "  bar",
                "  baz",
                ""
            ));
            // Simple string, variable spaces, limiting is at end, no final newline.
            AssertMLS(" foo\n bar\nbaz", BootlegMLS(
                "  foo",
                "  bar",
                " baz"
            ));

            // Simple string, mix of tabs and spaces.
            AssertMLS("foo\nbar\nbaz", BootlegMLS(
                "\t\t   \t foo",
                "\t\t   \t bar",
                "\t\t   \t baz"
            ));

            // Simple string, mix of tabs and spaces, middle shortest.
            AssertMLS("\t foo\n bar\n\t baz", BootlegMLS(
                "\t\t   \t foo",
                "\t\t    bar",
                "\t\t   \t baz"
            ));

            // Simple string, mix of tabs and spaces, mismatch at start.
            AssertMLS("\t\t foo\n \t bar\n\t\t baz", BootlegMLS(
                "\t\t foo",
                " \t bar",
                "\t\t baz"
            ));

            // Whitespace preserved at end with escape.
            AssertMLS("foo  ", BootlegMLS(
                "foo  \\p",
                ""
            ));

            // Whitespace preserved at end no extra end newline with escape.
            AssertMLS("foo  ", BootlegMLS(
                "foo  \\p"
            ));

            // Preservation escape is still erased even if not at EOL.
            AssertMLS("foo  test", BootlegMLS(
                "foo  \\ptest"
            ));

            // Escape codes work.
            AssertMLS("foo\t\nfive", BootlegMLS(
                "foo\\t\\nfive",
                ""
            ));

            // Escape codes signalling whitespace are not removed at EOL.
            AssertMLS("foo \t\n", BootlegMLS(
                "foo \\t\\n",
                ""
            ));

            AssertMLS("測試字串", BootlegMLS("測試字串"));

            // Pathological case. Shouldn't end early. Probably more of an issue on the serialization side.
            AssertMLS("\"", "\\\"");

            // Handles \r\n properly (by stripping the \r).
            SimpleDictAssert($"key: \"\"\"\r\n foo\r\n bar\"\"\"", "key", "foo\nbar");

            AssertBadParse("\"\"\" same line mls doesn't work! \"\"\"");
            AssertBadParse("\"\"\"\nThis has no closer.");
            AssertBadParse("\"\"\"\nThis has no closer but kind of looks like it does due to an escape. \\\"\"\"");
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
        public void TestLong() {
            AssertLong(5000000000L, "5000000000");
            AssertLong(-9223372036854775808, "-9223372036854775808");
            AssertLong(9223372036854775807, "9223372036854775807");
            AssertLong(2147483648L, "2147483648");
            AssertLong(0, "0");

            AssertBadParse("l 5.5");
            AssertBadParse("l foo");
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
        public void TestFloatWithCommaSeparatorCulture() {
            // France uses ',' as a decimal separator.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

            // Verify that it's the culture is used by default and formats as ','.
            Assert.AreEqual(1.5f, float.Parse("1,5"));

            // Verify that the deserializer uses '.' regardless.
            AssertFloat(1.5f, "1.5");
        }


        [Test]
        public void TestDouble() {
            AssertDouble(5.5d, "5.5");
            AssertDouble(-12345.6789d, "-12345.6789");
            AssertDouble(1E+20d, "1E+20");
            AssertDouble(0d, "0");

            AssertBadParse("d 5,5");
            AssertBadParse("d foo");
        }

        [Test]
        public void TestDoubleWithCommaSeparatorCulture() {
            // France uses ',' as a decimal separator.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

            // Verify that it's the culture is used by default and formats as ','.
            Assert.AreEqual(1.5, double.Parse("1,5"));

            // Verify that the deserializer uses '.' regardless.
            AssertDouble(1.5, "1.5");
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
            // Basic example
            // key:[
            // s foo
            // i 5
            // f 3.5
            // ]
            AssertArray(GenerateList("foo", 5, 3.5), GenerateTestArray("s foo", "i 5", "f 3.5"));
            // Nested array
            // key:[
            // [
            // "foo"
            // "bar"
            // ]
            // "baz"
            // ]
            AssertArray(GenerateList(GenerateList("foo", "bar"), "baz"), "[ \n \"foo\" \n \"bar\" \n ] \n \"baz\" \n");

            // Test bad key parse in array. Should just drop the offending key.
            // key:[
            // s foo
            // _ test
            // ]
            AssertArray(GenerateList("foo"), GenerateTestArray("s foo", "_ test"));
            // Test no closing ']'
            // key:[
            // s foo
            // i 5
            AssertBadArrayParse("[ \n s foo \n i 5 \n");
            // Test text after opening '['
            AssertBadArrayParse("[ hello there \n s foo \n i 5 \n ]");
            // Test text after closing ']'
            AssertBadArrayParse("[ \n s foo \n i 5 \n ] goodbye!");
        }

        [Test]
        public void TestDictionary() {
            // Basic example.
            // key:{
            // key1: s foo
            // key2: i 5
            AssertDict(
                GenerateDict(("key1", "foo"), ("key2", 5)),
                BootlegMLS("key1:s foo", "key2: i 5"));
            // Nested dictionary.
            // key:{
            // key1: s foo
            // key2: {
            //   ik: f 2.5
            // }
            // key3: i 5
            AssertDict(
                GenerateDict(("key1", "foo"), ("key2", GenerateDict(("ik", 2.5))), ("key3", 5)),
                BootlegMLS("key1:s foo", $"key2:{BootlegMLS("{", "ik: f 2.5", "}")}", "key3: i 5"));

            // Test no closing '}'
            AssertBadArrayParse(BootlegMLS("{", "key1: s foo", "key2: i  5"));
            // Test text after opening '{'
            AssertBadArrayParse(BootlegMLS("{ hello there", "key1: s foo", "key2: i  5", "}"));
            // Test text after closing ']'
            AssertBadArrayParse(BootlegMLS("{", "key1: s foo", "key2: i  5", "} goodbye!"));
        }

        [Test] public void TestComments() {
            string file = BootlegMLS(
                "key1: s foo",
                " # Hello, I am a comment",
                "key2: i 2"
            );
            var actual = new Deserializer().Parse(file.ToString());
            var expected = GenerateDict(("key1", "foo"), ("key2", 2));
            Assert.AreEqual(expected, actual);

            file = BootlegMLS(
                "key1: [",
                " s foo",
                " # Hello, I am a comment",
                " s bar",
                "]"
            );
            actual = new Deserializer().Parse(file.ToString());
            expected = GenerateDict(("key1", GenerateList("foo", "bar")));
            Assert.AreEqual(expected, actual);

            file = BootlegMLS(
                "key1: {",
                " k1: s foo",
                " # Hello, I am a comment",
                " k2: s bar",
                "}"
            );
            actual = new Deserializer().Parse(file.ToString());
            expected = GenerateDict(("key1", GenerateDict(("k1", "foo"), ("k2", "bar"))));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestKeyParse() {
            AssertKey("", "");
            AssertKey("foo", "foo");
            AssertKey("foo", "foo ");
            AssertKey("foo", " foo");
            AssertKey("foo", " foo ");
            AssertKey("foo", "\"foo\"");
            AssertKey("foo ", "\"foo \"");
            AssertKey(" foo ", "\" foo \"");
            AssertKey(" foo", "\" foo\"");
            AssertKey("foo:bar", "\"foo:bar\"");
            AssertKey("foo bar", "foo bar");
            // "\"foo" -> "foo
            AssertKey("\"foo", "\"\\\"foo\"");
            AssertKey("5\\n3", "5\\n3");
            AssertKey("測試字串", "測試字串");

            // No : allowed in unquoted strings.
            AssertBadKeyParse("foo:bar");
            // No \n allowed in keys. Can't just test for bad key parse here, as
            // foo will be discarded for being an incomplete entry, and the value
            // will be attached to bar.
            SimpleDictAssert("foo\nbar: i 5", "bar", 5);
            SimpleDictAssert("\"foo\nbar\": i 5", "bar\"", 5);
            // No closing quote.
            AssertBadKeyParse("\"foo");
            
            
        }

        [Test]
        public void TestInvalidType() {
            AssertBadParse("_ foo");
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
            file.AppendLine("key6:[");
            file.AppendLine("  s foo");
            file.AppendLine("  i 5");
            file.AppendLine("]");
            file.AppendLine("key7:{");
            file.AppendLine("  key7a:s foo");
            file.AppendLine("  key7b:i 5");
            file.AppendLine("}");
            file.AppendLine("key8: \"\"\"");
            file.AppendLine("  this is");
            file.AppendLine("  some ok text. \"\"\"");
            file.AppendLine("key9: _rgb #806040");
            file.AppendLine("key10: _rgba #80604080");
            file.AppendLine("key_long: l 5");
            file.AppendLine("key_double: d -1.5");
            file.AppendLine("key999:i 1");
            Dictionary<string, object> expected = new Dictionary<string, object>();
            expected["key0"] = 1;
            expected["key1"] = "foo";
            expected["key1b"] = "fooo";
            expected["key2"] = "bar";
            expected["key3"] = 43289;
            expected["key4"] = 43289.5;
            expected["key5"] = true;
            expected["key6"] = GenerateList("foo", 5);
            expected["key7"] = GenerateDict(("key7a", "foo"), ("key7b", 5));
            expected["key8"] = "this is\nsome ok text.";
            expected["key9"] = new Color(128f / 255, 96f / 255, 64f / 255);
            expected["key10"] = new Color(128f / 255, 96f / 255, 64f / 255, 128f / 255);
            expected["key_long"] = 5L;
            expected["key_double"] = -1.5;
            expected["key999"] = 1;

            var actual = new Deserializer().Parse(file.ToString());
            Assert.AreEqual(expected, actual);
        }

        private static void AssertBadKeyParse(string testKey) {
            Deserializer deserializer = new Deserializer();
            Assert.AreEqual(new Dictionary<string, object>(), deserializer.Parse($"{testKey}:s foo"));
        }

        private static void AssertBadParse(string typeAndValue) {
            Deserializer deserializer = new Deserializer();
            Assert.AreEqual(new Dictionary<string, object>(), deserializer.Parse($"foo:{typeAndValue}"));
        }

        private static void AssertBadArrayParse(string arrayValue) {
            Deserializer deserializer = new Deserializer();
            Assert.AreEqual(new Dictionary<string, object>(), deserializer.Parse($"foo:{arrayValue}"));
        }

        private static void AssertBadDictionaryParse(string dictValue) {
            Deserializer deserializer = new Deserializer();
            Assert.AreEqual(new Dictionary<string, object>(), deserializer.Parse($"foo:{dictValue}"));
        }

        private static void AssertKey(string expectedKey, string testKey) {
            SimpleDictAssert($"{testKey}: s 123", expectedKey, "123");
        }

        private static void AssertValue(object expectedValue, string typeAndValue) {
            SimpleDictAssert($"key:{typeAndValue}", "key", expectedValue);
        }

        private static void AssertString(string expectedStr, string testStr) {
            SimpleDictAssert($"key:s {testStr}", "key", expectedStr);
        }

        private static void AssertMLS(string expectedStr, string testStr) {
            SimpleDictAssert($"key: \"\"\"\n{testStr}\"\"\"", "key", expectedStr);
        }

        private static void AssertInt(int expected, string testStr) {
            SimpleDictAssert($"key:i {testStr}", "key", expected);
        }

        private static void AssertLong(long expected, string testStr) {
            SimpleDictAssert($"key:l {testStr}", "key", expected);
        }


        private static void AssertFloat(float expected, string testStr) {
            SimpleDictAssert($"key:f {testStr}", "key", expected);
        }

        private static void AssertDouble(double expected, string testStr) {
            SimpleDictAssert($"key:d {testStr}", "key", expected);
        }

        private static void AssertBool(bool expected, string testStr) {
            SimpleDictAssert($"key:b {testStr}", "key", expected);
        }

        private static void AssertQuotedString(string expectedStr, string testStr) {
            SimpleDictAssert($"key:s \"{testStr}\"", "key", expectedStr);
        }

        private static void AssertDict(Dictionary<string, object> expected, string testStr) {
            SimpleDictAssert($"key:{{\n{testStr}\n}}", "key", expected);
        }

        private static void AssertArray(List<object> expected, string testStr) {
            SimpleDictAssert($"key:[\n{testStr}\n]", "key", expected);
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

        private static string GenerateTestArray(params string[] values) {
            return string.Join('\n', values);
        }

        private static void SimpleDictAssert(string file, string key, object value) {
            Deserializer deserializer = new Deserializer();
            Dictionary<string, object> actual = deserializer.Parse(file);
            Assert.True(actual.ContainsKey(key));
            Assert.AreEqual(value, actual[key]);
        }
    }
}