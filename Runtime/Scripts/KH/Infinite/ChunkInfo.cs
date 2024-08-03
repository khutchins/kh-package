using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pomerandomian;

namespace KH.Infinite {

    public interface IChunkGenerator {
        public abstract void Generate(ChunkInfo info, IRandom random);
    }

    public class ChunkInfo {
        public object GeneratedObject;
        public bool IsGenerated;
        public ChunkLocation Location;

        public ChunkInfo(ChunkLocation location) {
            Location = location;
        }
    }

    public struct ChunkLocation {
        /// <summary>
        /// Unique key for the dictionary. Generally the position divided
        /// by the smallest unit to keep key size in bounds for the longest
        /// time.
        /// </summary>
        public Vector2Long Key;
        /// <summary>
        /// Upper left bound of the chunk (inf space, not world space).
        /// </summary>
        public Vector2Long Bound1;
        /// <summary>
        /// Bottom right bound of the chunk (inf space, not world space).
        /// </summary>
        public Vector2Long Bound2;
        /// <summary>
        /// Bottom left bound of the chunk (inf space, not world space).
        /// 
        /// Computed on demand.
        /// </summary>
        public Vector2Long Bound3 { get => new Vector2Long(Bound1.x, Bound2.y); }
        /// <summary>
        /// Top right bound of the chunk (inf space, not world space).
        /// 
        /// Computed on demand.
        /// </summary>
        public Vector2Long Bound4 { get => new Vector2Long(Bound2.x, Bound1.y); }

        public bool InRange(Vector2Long pos, float distance) {
            return Vector2Long.Distance(pos, Bound1) <= distance
                || Vector2Long.Distance(pos, Bound2) <= distance
                || Vector2Long.Distance(pos, Bound3) <= distance
                || Vector2Long.Distance(pos, Bound4) <= distance;
        }
    }
}