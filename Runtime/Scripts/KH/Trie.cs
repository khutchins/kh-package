using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Antlr3.Runtime;
using UnityEngine;

namespace KH {
    public class Trie : TrieNode {
        public Trie() : base("") {
        }

        public void Insert(string word) {
            Insert(word, 0);
        }

        public void Remove(string word) {
            Remove(word, 0);
        }

        public bool Contains(string word) {
            return Contains(word, 0);
        }

        public string GetShortestSharedPrefix(string basePrefix) {
            var node = GetPrefix(basePrefix);
            if (node == null) return basePrefix;
            return node.ShortestSharedPrefix(basePrefix);
        }

        public IEnumerable<string> WordsWithPrefix(string prefix) {
            var node = GetPrefix(prefix);
            if (node == null) {
                yield break;
            } else {
                foreach (string word in node.AllChildren(prefix)) {
                    yield return word;
                }
            }
        }

        private TrieNode GetPrefix(string prefix) {
            return GetPrefix(prefix, 0);
        }
    }

    public class TrieNode {
        public readonly string Letter;
        private Dictionary<string, TrieNode> _children = new Dictionary<string, TrieNode>();
        private bool _terminal;

        public bool Terminal {
            get => _terminal;
        }

        public TrieNode(string letter) {
            Letter = letter;
        }

        protected void Insert(string word, int index) {
            if (index == word.Length) {
                _terminal = true;
            } else {
                var node = EnsureChild(word[index].ToString());
                node.Insert(word, index + 1);
            }
        }

        protected bool Remove(string word, int index) {
            if (index == word.Length) {
                _terminal = false;
            } else {
                string nextLetter = word[index].ToString();
                if (_children.TryGetValue(nextLetter, out TrieNode value)) {
                    if (value.Remove(word, index + 1)) {
                        _children.Remove(nextLetter);
                    }
                }
            }

            return !_terminal && _children.Count == 0;
        }

        public string ShortestSharedPrefix(string basePrefix) {
            if (_terminal) return basePrefix;
            if (_children.Count > 1) return basePrefix;
            if (_children.Count == 0) return basePrefix;
            var entry = _children.First();
            return entry.Value.ShortestSharedPrefix(basePrefix + entry.Key);
        }

        protected bool Contains(string word, int index) {
            if (index == word.Length) return _terminal;
            if (_children.TryGetValue(word[index].ToString(), out TrieNode value)) {
                return value.Contains(word, index + 1);
            }
            return false;
        }

        protected TrieNode GetPrefix(string word, int index) {
            if (index == word.Length) return this;
            if (_children.TryGetValue(word[index].ToString(), out TrieNode value)) {
                return value.GetPrefix(word, index + 1);
            }
            return null;
        }

        public IEnumerable<string> AllChildren(string prefix) {
            if (_terminal) yield return prefix;
            foreach (var entry in _children) {
                foreach (string word in entry.Value.AllChildren(prefix + entry.Key)) {
                    yield return word;
                }
            }
        }

        private TrieNode EnsureChild(string letter) {
            if (!_children.TryGetValue(letter, out TrieNode value)) {
                value = new TrieNode(letter);
                _children[letter] = value;
            }
            return value;
        }
    }
}