using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KH.Extensions {
    public static class VectorExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVectorX0Y(this Vector2 vec) {
            return new Vector3(vec.x, 0, vec.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector0XY(this Vector2 vec) {
            return new Vector3(0, vec.x, vec.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVectorXY0(this Vector2 vec) {
            return new Vector3(vec.x, vec.y, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVectorXY(this Vector3 vec) {
            return new Vector2(vec.x, vec.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVectorXZ(this Vector3 vec) {
            return new Vector2(vec.x, vec.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVectorYZ(this Vector3 vec) {
            return new Vector2(vec.y, vec.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int ToVectorX0Y(this Vector2Int vec) {
            return new Vector3Int(vec.x, 0, vec.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int ToVector0XY(this Vector2Int vec) {
            return new Vector3Int(0, vec.x, vec.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int ToVectorXY0(this Vector2Int vec) {
            return new Vector3Int(vec.x, vec.y, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int ToVectorXY(this Vector3Int vec) {
            return new Vector2Int(vec.x, vec.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int ToVectorXZ(this Vector3Int vec) {
            return new Vector2Int(vec.x, vec.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int ToVectorYZ(this Vector3Int vec) {
            return new Vector2Int(vec.y, vec.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsXY(this Vector2 vec, Vector3 comp) {
            return vec.x == comp.x && vec.y == comp.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsXZ(this Vector2 vec, Vector3 comp) {
            return vec.x == comp.x && vec.y == comp.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsYZ(this Vector2 vec, Vector3 comp) {
            return vec.x == comp.y && vec.y == comp.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsXY(this Vector2Int vec, Vector3Int comp) {
            return vec.x == comp.x && vec.y == comp.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsXZ(this Vector2Int vec, Vector3Int comp) {
            return vec.x == comp.x && vec.y == comp.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsYZ(this Vector2Int vec, Vector3Int comp) {
            return vec.x == comp.y && vec.y == comp.z;
        }
    }
}