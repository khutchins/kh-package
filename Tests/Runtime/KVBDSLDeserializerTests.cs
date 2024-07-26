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
        }

        [Test]
        public void TestMLString() {
            // TODO:
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

            AssertBadKeyParse("\"foo");
            AssertBadKeyParse("foo:bar");
        }

        [Test]
        public void TestGeneral() {
            StringBuilder file = new StringBuilder();
            file.AppendLine("key1:s \"foo\"");
            file.AppendLine("key1b: s fooo");
            file.AppendLine("key2: \"bar\"");
            file.AppendLine("key3:i 43289");
            file.AppendLine("key4:f 43289.5");
            file.AppendLine("key5:b true");
            Dictionary<string, object> expected = new Dictionary<string, object>();
            expected["key1"] = "foo";
            expected["key1b"] = "foo";
            expected["key2"] = "bar";
            expected["key3"] = 43289;
            expected["key4"] = 43289.5;
        }

        private static void AssertBadKeyParse(string testKey) {
            Assert.AreEqual(new Dictionary<string, object>(), Deserializer.ParseString($"{testKey}:s foo"));
        }

        private static void AssertKey(string expectedKey, string testKey) {
            SimpleDictAssert($"{testKey}: s 123", expectedKey, "123");
        }

        private static void AssertString(string expectedStr, string testStr) {
            SimpleDictAssert($"key:s {testStr}", "key", expectedStr);
        }

        private static void AssertQuotedString(string expectedStr, string testStr) {
            SimpleDictAssert($"key:s \"{testStr}\"", "key", expectedStr);
        }

        private static void SimpleDictAssert(string file, string key, object value) {
            Dictionary<string, object> expected = new Dictionary<string, object>();
            expected[key] = value;
            Assert.AreEqual(expected, Deserializer.ParseString(file));
        }
    }
}