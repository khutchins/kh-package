using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using KH.Texts;

namespace KH.Texts {
    public class TextPlayerTests {

        [Test]
        public void TestShortString() {
            TextPlayer player = new TextPlayer("t");

            TextUpdateExpectation[] expected = {
                new TextUpdateExpectation.Builder().SetString("", "t").Build(),
                new TextUpdateExpectation.Builder().SetString("t", "").Build(),
            };

            AssertMatches(expected, player);
        }

        [Test]
        public void TestBasicString() {
            TextPlayer player = new TextPlayer("test");

            TextUpdateExpectation[] expected = {
                new TextUpdateExpectation.Builder().SetString("", "test").Build(),
                new TextUpdateExpectation.Builder().SetString("t", "est").Build(),
                new TextUpdateExpectation.Builder().SetString("te", "st").Build(),
                new TextUpdateExpectation.Builder().SetString("tes", "t").Build(),
                new TextUpdateExpectation.Builder().SetString("test", "").Build(),
            };

            AssertMatches(expected, player);
        }

        [Test]
        public void TestBasicStringWithSpace() {
            TextPlayer player = new TextPlayer("a b");

            TextUpdateExpectation[] expected = {
                new TextUpdateExpectation.Builder().SetString("", "a b").Build(),
                new TextUpdateExpectation.Builder().SetString("a", " b").Build(),
                new TextUpdateExpectation.Builder().SetString("a b", "").Build(),
            };

            AssertMatches(expected, player);
        }

        [Test]
        public void TestBlipsInBasicString() {
            TextPlayer player = new TextPlayer("a b");

            TextUpdateExpectation[] expected = {
                new TextUpdateExpectation.Builder().SetString("", "a b").SetPlayBlip(false).Build(),
                new TextUpdateExpectation.Builder().SetString("a", " b").SetPlayBlip(true).Build(),
                new TextUpdateExpectation.Builder().SetString("a b", "").SetPlayBlip(true).Build(),
            };

            AssertMatches(expected, player);
        }

        [Test]
        public void TestDelay() {
            TextPlayer player = new TextPlayer("a! (b)");

            TextUpdateExpectation[] expected = {
                new TextUpdateExpectation.Builder().SetString("", "a! (b)").SetDelay(0).Build(),
                new TextUpdateExpectation.Builder().SetString("a", "! (b)").SetDelay(0.015f).Build(),
                new TextUpdateExpectation.Builder().SetString("a!", " (b)").SetDelay(0.2f).Build(),
                new TextUpdateExpectation.Builder().SetString("a! (b)", "").SetDelay(0.015f).Build(),
            };

            AssertMatches(expected, player);
        }

        void AssertMatches(TextUpdateExpectation[] expectations, TextPlayer player, int from = 0, bool strictEnd = true) {
            if (from < 0) Assert.Fail("From index cannot be less than zero: {0}", from);
            int idx = 0;
            int expectIdx = -from;
            foreach (TextUpdate update in player) {
                if (expectIdx >= expectations.Length && strictEnd) {
                    Assert.Fail("Update length {0} greater than provided {1}. Extra TextUpdate: {2}", idx + 1, expectations.Length + from, update);
				} else if (expectIdx >= expectations.Length) {
                    return;
				}
                if (idx >= from) AssertMatch(expectations[expectIdx], update);
                idx++;
                expectIdx++;
            }
        }

        void AssertMatch(TextUpdateExpectation expectation, TextUpdate actual) {
            if (expectation.NewString != null) Assert.AreEqual(expectation.NewString, actual.NewString);
            if (expectation.Delay != null) Assert.AreEqual((float)expectation.Delay, actual.Delay, "Expected delay of {0} but was {1}", expectation.Delay, actual.Delay);
            if (expectation.PlayBlip != null) Assert.AreEqual((bool)expectation.PlayBlip, actual.PlayBlip);
            if (expectation.BypassKeypress != null) Assert.AreEqual((bool)expectation.BypassKeypress, actual.BypassKeypress);
            if (expectation.UnrecognizedTags != null) CollectionAssert.AreEqual(expectation.UnrecognizedTags, actual.UnrecognizedTags);
        }

        class TextUpdateExpectation {

            public readonly string NewString;
            public readonly float? Delay;
            public readonly bool? PlayBlip;
            public readonly List<TextToken> UnrecognizedTags;
            public readonly bool? BypassKeypress;

            public TextUpdateExpectation(string newStr, float? delay, bool? playBlip, bool? bypassKepress, List<TextToken> unrecognizedTags = null) {
                NewString = newStr;
                Delay = delay;
                PlayBlip = playBlip;
                BypassKeypress = bypassKepress;
                UnrecognizedTags = unrecognizedTags;
			}

            public class Builder {
                private string _newString;
                private float? _delay;
                private bool? _playBlip;
                private List<TextToken> _unrecognizedTags;
                private bool? _bypassKeypress;

                public Builder() {

				}

                public TextUpdateExpectation Build() {
                    return new TextUpdateExpectation(_newString, _delay, _playBlip, _bypassKeypress, _unrecognizedTags);
				}

                public Builder SetString(string str) {
                    _newString = str;
                    return this;
				}

                public Builder SetString(string visible, string remainder) {
                    if (remainder == null || remainder.Length <= 0) {
                        _newString = visible;
					} else {
                        _newString = string.Format("{0}<color=#00000000>{1}</color>", visible, remainder);
                    }
                    return this;
                }

                public Builder SetDelay(float delay) {
                    _delay = delay;
                    return this;
                }

                public Builder SetBypassKeypress(bool bypassKeypress) {
                    _bypassKeypress = bypassKeypress;
                    return this;
                }

                public Builder SetPlayBlip(bool playBlip) {
                    _playBlip = playBlip;
                    return this;
                }

                public Builder AddTag(TextToken token) {
                    if (_unrecognizedTags == null) {
                        _unrecognizedTags = new List<TextToken>();
					}
                    _unrecognizedTags.Add(token);
                    return this;
				}

                public Builder SetTags(List<TextToken> tokens) {
                    _unrecognizedTags = tokens;
                    return this;
				}
            }
        }
    }
}