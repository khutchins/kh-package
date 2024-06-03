using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Infinite {
    public class ChunkManager<T> where T : ChunkInfo {

        private Func<ChunkLocation, T> _infoGenerator;
        private Func<Vector2Int, T, object> _chunkGenerator;
        private Action<T> _clearer;
        private int _cleanup = 200;
        private int _create = 100;
        private Vector2Long _lastPos = Vector2Long.zero;
        private readonly Vector2Int _smallestUnit;

        private readonly Dictionary<Vector2Long, T> _chunkCache = new();
        private readonly List<T> _chunkInfoList = new();

        public ChunkManager(Vector2Int smallestUnit) {
            _smallestUnit = smallestUnit;
        }

        public void SetInfoGenerator(Func<ChunkLocation, T> generator) {
            _infoGenerator = generator;
        }

        public void SetChunkGenerator(Func<Vector2Int, T, object> generator) {
            _chunkGenerator = generator;
        }

        public void SetChunkClearer(Action<T> clearer) {
            _clearer = clearer;
        }

        public void SetRadius(int create, int cleanup) {
            if (cleanup < create) cleanup = create;
            _create = create;
            _cleanup = cleanup;
        }

        public void Update(Vector2Long position) {
            _lastPos = position;
            for (long x = position.x - _create; x <= position.x + _create; x += _smallestUnit.x / 2) {
                for (long y = position.y - _create; y <= position.y + _create; y += _smallestUnit.y / 2) {
                    EnsureChunkForPointInternal(new Vector2Long(x, y));
                }
            }
        }

        private T EnsureChunkForPointInternal(Vector2Long point) {
            Vector2Long key = point.DivideAndFloor(_smallestUnit);

            if (!_chunkCache.ContainsKey(key)) {

                T info = ChunkInfoForPointSkipCache(key);

                if (Vector2Long.Distance(_lastPos, info.Location.Bound1) > _create
                        && Vector2Long.Distance(_lastPos, info.Location.Bound2) > _create) {
                    // Not actually in bounds.
                    return null;
                }

                info.GeneratedObject = _chunkGenerator.Invoke(_smallestUnit, info);

                _chunkInfoList.Add(info);
                _chunkCache[key] = info;
            }

            return _chunkCache[key];
        }

        public T ChunkInfoWithOffset(ChunkLocation loc, Vector2Long offset) {
            return ChunkInfoForPointSkipCache(loc.Key + offset);
        }

        private T ChunkInfoForPointSkipCache(Vector2Long key) {
            Vector2Long modifiedPos = key * _smallestUnit;
            return _infoGenerator(new ChunkLocation() { 
                Key = key, 
                Bound1 = modifiedPos, 
                Bound2 = modifiedPos + _smallestUnit 
            });
        }

        public IEnumerator CleanupCoroutine() {
            while (true) {
                int clears = 0;
                for (int i = 0; i < _chunkInfoList.Count; i++) {
                    T info = _chunkInfoList[i];
                    if (Vector2Long.Distance(_lastPos, info.Location.Bound1) > _cleanup
                        && Vector2Long.Distance(_lastPos, info.Location.Bound2) > _cleanup) {
                        _chunkCache.Remove(info.Location.Key);
                        _chunkInfoList.RemoveAt(i);
                        _clearer?.Invoke(info);
                        i--;
                        // Don't want it to hang on clearing. Not sure if that'll happen, but hey, whatever.
                        clears++;
                        if (clears % 10 == 0) yield return null;
                    }
                }
                yield return null;
            }
        }
    }
}