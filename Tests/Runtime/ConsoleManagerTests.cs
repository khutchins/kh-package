using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using KH.Texts;

namespace KH.Console {
    public class ConsoleManagerTests {

        [Test]
        public void TestStringEscaping() {
            // No escaping.
            Assert.AreEqual("foo", ConsoleManager.EscapeStringIfNecessary("foo"));
            Assert.AreEqual("tes\"t", ConsoleManager.EscapeStringIfNecessary("tes\"t"));
            Assert.AreEqual("tes't", ConsoleManager.EscapeStringIfNecessary("tes't"));
            Assert.AreEqual("tes\\t", ConsoleManager.EscapeStringIfNecessary("tes\\t"));
            Assert.AreEqual("'\"", ConsoleManager.EscapeStringIfNecessary("'\""));

            // Basic escaping.
            Assert.AreEqual("\"tes t\"", ConsoleManager.EscapeStringIfNecessary("tes t"));
            Assert.AreEqual("\"tes t", ConsoleManager.EscapeStringIfNecessary("tes t", false));
            Assert.AreEqual("'tes\" t'", ConsoleManager.EscapeStringIfNecessary("tes\" t"));
            Assert.AreEqual("'tes\" t", ConsoleManager.EscapeStringIfNecessary("tes\" t", false));

            // All escape characters present.
            Assert.AreEqual("\"te\\\\s\\\"' t\"", ConsoleManager.EscapeStringIfNecessary("te\\s\"' t"));
            Assert.AreEqual("\"te\\\\s\\\"' t", ConsoleManager.EscapeStringIfNecessary("te\\s\"' t", false));
        }

        [Test]
        public void TestStringParsing() {
            Assert.That(CommandParser.ParseText("foo"), Is.EquivalentTo(new string[] { "foo" }));
            Assert.That(CommandParser.ParseText("foo", true), Is.EquivalentTo(new string[] { "foo" }));

            Assert.That(CommandParser.ParseText("foo "), Is.EquivalentTo(new string[] { "foo" }));
            Assert.That(CommandParser.ParseText("foo ", true), Is.EquivalentTo(new string[] { "foo", "" }));

            Assert.That(CommandParser.ParseText("'foo '"), Is.EquivalentTo(new string[] { "foo " }));
            Assert.That(CommandParser.ParseText("'foo '", true), Is.EquivalentTo(new string[] { "foo " }));

            Assert.That(CommandParser.ParseText("\"foo \""), Is.EquivalentTo(new string[] { "foo " }));
            Assert.That(CommandParser.ParseText("\"foo \"", true), Is.EquivalentTo(new string[] { "foo " }));

            Assert.That(CommandParser.ParseText("foo \"bar\" 'baz' "), Is.EquivalentTo(new string[] { "foo", "bar", "baz" }));
            Assert.That(CommandParser.ParseText("foo \"bar\" 'baz' ", true), Is.EquivalentTo(new string[] { "foo", "bar", "baz", "" }));
        }
    }
}