using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pomerandomian;
using UnityEngine;

namespace KH.Music {
    [CreateAssetMenu(menuName = "KH/SongPlaylist")]
    public class SongPlaylist : ScriptableObject {
        public Song[] Songs;
        [SerializeField] bool Shuffle;

        private List<Song> _shuffledSongs;
        private int _index;

        public Song GetNextSong() {
            if (Songs.Length == 0) {
                Debug.LogWarning($"Playlist {this} has no songs!");
            }
            if (Shuffle) {
                if (_shuffledSongs == null || _index < 0 || _index >= _shuffledSongs.Count) {
                    EnsureShuffledSongs(true);
                    _index = 0;
                }
                return _shuffledSongs[_index++];
            } else {
                Song song = Songs[_index];
                _index = (_index + 1) % Songs.Length;
                return song;
            }
        }

        void EnsureShuffledSongs(bool forceReshuffle) {
            if (_shuffledSongs == null || forceReshuffle) {
                IRandom rand = new SystemRandom();
                _shuffledSongs = Songs.Shuffle(rand).ToList();
            }
        }

        public List<Song> GetSongList() {
            if (Shuffle) {
                EnsureShuffledSongs(false);
                return _shuffledSongs;
            } else {
                return Songs.ToList();
            }
        }

        public float PlaylistLength() {
            return Songs.Select(x => x.Audio.length).Sum();
        }
    }
}