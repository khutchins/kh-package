using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pomerandomian;

namespace KH.Music {
    public class RadioStation : MonoBehaviour {
        [SerializeField] SongPlaylist Playlist;

        List<Song> _shuffledList;
        float _totalLength;
        float _startTime;

        void Start() {
            _shuffledList = Playlist.GetSongList();
            _totalLength = Playlist.PlaylistLength();
            // This will push us back into the past, so that it won't start at a clean song boundary.
            _startTime = Time.unscaledTime - new SystemRandom().Next(0, _totalLength);
        }

        /// <summary>
        /// Use when turning on the radio for the first time. Avoid using this when a song
        /// finishes, as there could potentially be a floating point issue that would return
        /// the last finished song, but near the end.
        /// </summary>
        /// <returns>Snapshot indicating what song to play and where to start it.</returns>
        public SongSnapshot GetCurrentPosition() {
            float delta = (Time.unscaledTime - _startTime) % _totalLength;

            float remaining = delta;

            for (int i = 0; i < _shuffledList.Count; i++) {
                Song song = _shuffledList[i];
                if (remaining < song.Audio.length) {
                    return new SongSnapshot() {
                        song = song,
                        songIndex = i,
                        startTime = remaining
                    };
                }
                remaining -= song.Audio.length;
            }

            Debug.LogWarning("Past end of playlist. This shouldn't occur.");
            return new SongSnapshot();
        }
        
        /// <summary>
        /// Returns the next song on the radio. Use if you already have a playing song.
        /// Otherwise, use GetCurrentPosition().
        /// </summary>
        /// <param name="lastSong">The previous song read.</param>
        /// <returns></returns>
        public SongSnapshot GetNextSong(SongSnapshot lastSong) {
            int nextIndex = (lastSong.songIndex + 1) % _shuffledList.Count;
            return new SongSnapshot() {
                song = _shuffledList[nextIndex],
                songIndex = nextIndex,
                startTime = 0
            };
        }

        public struct SongSnapshot {
            public Song song;
            public int songIndex;
            public float startTime;
        }
    }
}