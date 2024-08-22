using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Infinite {
    public interface IChunkManager {
        void SetRadius(int create, int cleanup);
        void Update(Vector2Long position);
        IEnumerator CleanupCoroutine();
    }

    public class ChunkManager<T> : IChunkManager where T : ChunkInfo {

        private Func<ChunkLocation, T> _infoGenerator;
        private Func<Vector2Int, T, object> _chunkGenerator;
        private Action<T> _clearer;
        private Action<T> _chunkClearer;
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

        public void SetInfoClearer(Action<T> clearer) {
            _chunkClearer = clearer;
        }

        public void SetRadius(int create, int cleanup) {
            if (cleanup < create) cleanup = create;
            _create = create;
            _cleanup = cleanup;
        }

        /// <summary>
        /// Create chunks based on the updated player position. The position is in world space, but
        /// with no y-component and the x and z components floored and put into a Vector2Long. I should
        /// probably change this to take in a Vector2Double type.
        /// </summary>
        /// <param name="position"></param>
        public void Update(Vector2Long position) {
            _lastPos = position;
            double px = _lastPos.x * 1.0 / _smallestUnit.x;
            double py = _lastPos.y * 1.0 / _smallestUnit.y;
            double cx = _create * 1.0 / _smallestUnit.x;
            double cy = _create * 1.0 / _smallestUnit.y;

            long sx = (long)Math.Floor(px - cx);
            long ex = (long)Math.Floor(px + cx);
            long sy = (long)Math.Floor(py - cy);
            long ey = (long)Math.Floor(py + cy);
            Vector2Long curr = new Vector2Long(sx, sy);
            for (curr.x = sx; curr.x <= ex; curr.x++) {
                for (curr.y = sy; curr.y <= ey; curr.y++) {
                    EnsureChunkForPointInternalInKeySpace(curr);
                }
            }
        }

        private T EnsureChunkForPointInternal(Vector2Long point, bool forceGenerate = true) {
            Vector2Long key = point.DivideAndFloor(_smallestUnit);
            return EnsureChunkForPointInternalInKeySpace(key, forceGenerate);
        }

        private T EnsureChunkForPointInternalInKeySpace(Vector2Long pointInKeySpace, bool forceGenerate = true) {
            if (!forceGenerate) {
                _chunkCache.TryGetValue(pointInKeySpace, out T cinfo);
                return cinfo;
            }
            T info = EnsureChunkInfo(pointInKeySpace);

            // Only actually generate the chunk if it's in bounds.
            if (!info.IsGenerated && info.Location.InRange(_lastPos, _create)) {
                MaybeGenerate(info);
            }

            return info;
        }

        private T EnsureChunkInfo(Vector2Long key) {
            if (!_chunkCache.ContainsKey(key)) {

                T info = ChunkInfoForPointSkipCache(key);

                _chunkInfoList.Add(info);
                _chunkCache[key] = info;
            }
            return _chunkCache[key];
        }

        /// <summary>
        /// Returns the first chunk info found for the location in otherManager. Will generate even if it's
        /// currently out of range.
        /// </summary>
        /// <param name="otherManager">The manager to do a lookup on.</param>
        /// <param name="location">The location to look up, in the coordinate space of THIS chunk manager.</param>
        /// <returns>The first chunk info for the given location. Could match multiple if going from a bigger chunk size to a smaller one.</returns>
        public U LookupChunkInfo<U>(ChunkManager<U> otherManager, ChunkLocation location) where U : ChunkInfo {
            double xLoc = location.Key.x * _smallestUnit.x;
            double yLoc = location.Key.y * _smallestUnit.y;
            return otherManager.ChunkInfoForWorldSpace(xLoc, yLoc);
        }

        /// <summary>
        /// Returns the first chunk info found for the location in otherManager. Will generate even if it's
        /// currently out of range.
        /// </summary>
        /// <param name="otherManager">The manager to do a lookup on.</param>
        /// <param name="location">The location to look up, in the coordinate space of THIS chunk manager.</param>
        /// <returns>The first chunk info for the given location. Could match multiple if going from a bigger chunk size to a smaller one.</returns>
        public bool TryLookupChunkInfo<U>(ChunkManager<U> otherManager, ChunkLocation location, out U info) where U : ChunkInfo {
            double xLoc = location.Key.x * _smallestUnit.x;
            double yLoc = location.Key.y * _smallestUnit.y;
            if (otherManager == null) {
                info = null;
                return false;
            }
            return otherManager.TryGetChunkInfoForWorldSpace(xLoc, yLoc, out info);
        }

        public bool TryGetChunkInfoForWorldSpace(double xLoc, double yLoc, out T info) {
            info = EnsureChunkForPointInternal(new Vector2Long((long)xLoc, (long)yLoc), false);
            return info != null;
        }

        public T ChunkInfoForWorldSpace(double xLoc, double yLoc) {
            return EnsureChunkForPointInternal(new Vector2Long((long)xLoc, (long)yLoc));
        }

        public T ChunkInfoWithOffset(ChunkLocation loc, Vector2Long offset) {
            return EnsureChunkInfo(loc.Key + offset);
        }

        private T ChunkInfoForPointSkipCache(Vector2Long key) {
            Vector2Long modifiedPos = key * _smallestUnit;
            return _infoGenerator(new ChunkLocation() {
                Key = key,
                Bound1 = modifiedPos,
                Bound2 = modifiedPos + _smallestUnit
            });
        }

        private bool MaybeGenerate(T info) {
            if (info.IsGenerated) return false;
            info.GeneratedObject = _chunkGenerator?.Invoke(_smallestUnit, info);
            info.IsGenerated = true;
            return true;
        }

        /// <summary>
        /// Cleans up all chunks that are out of range. You shouldn't use this
        /// directly, as it could potentially take a long time. Use CleanupCoroutine
        /// instead.
        /// </summary>
        public void ForceCleanupAll() {
            for (int i = 0; i < _chunkInfoList.Count; i++) {
                T info = _chunkInfoList[i];
                if (!info.Location.InRange(_lastPos, _cleanup)) {
                    Cleanup(i, info);
                    i--;
                }
            }
        }

        private void Cleanup(int idx, T info) {
            _chunkCache.Remove(info.Location.Key);
            _chunkInfoList.RemoveAt(idx);
            if (info.IsGenerated) {
                _clearer?.Invoke(info);
                info.IsGenerated = false;
            }
            _chunkClearer?.Invoke(info);
        }

        public IEnumerator CleanupCoroutine() {
            while (true) {
                int clears = 0;
                for (int i = 0; i < _chunkInfoList.Count; i++) {
                    T info = _chunkInfoList[i];
                    if (!info.Location.InRange(_lastPos, _cleanup)) {
                        Cleanup(i, info);
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