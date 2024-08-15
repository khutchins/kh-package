using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            // key:[
            // s foo
            // i 5
            // f 3.5
            // ]
            AssertArray(GenerateList("foo", 5, 3.5), GenerateTestArray("s foo", "i 5", "f 3.5"));
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

        private static void AssertBadArrayParse(string arrayValue) {
            Deserializer deserializer = new Deserializer();
            Assert.AreEqual(new Dictionary<string, object>(), deserializer.ParseString($"foo:{arrayValue}"));
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

        private static void AssertArray(List<object> expected, string testStr) {
            SimpleDictAssert($"key:[\n{testStr}\n]", "key", expected);
        }

        private static List<object> GenerateList(params object[] arr) {
            return arr.ToList();
        }

        private static string GenerateTestArray(params string[] values) {
            return string.Join('\n', values);
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