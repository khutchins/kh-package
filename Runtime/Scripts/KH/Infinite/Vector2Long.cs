using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vector2Long {
    public long x;
    public long y;

    public Vector2Long(long x, long y) {
        this.x = x;
        this.y = y;
    }

    public static Vector2Long zero {
        get => new(0, 0);
    }

    public static Vector2Long one {
        get => new(1, 1);
    }

    public override bool Equals(object obj) {
        return obj is Vector2Long @long &&
               x == @long.x &&
               y == @long.y;
    }

    public override int GetHashCode() {
        return HashCode.Combine(x, y);
    }

    public override string ToString() {
        return $"({x}, {y})";
    }

    public static float Distance(Vector2Long a, Vector2Long b) {
        return Mathf.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
    }

    public static bool operator ==(Vector2Long a, Vector2Long b) {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Vector2Long a, Vector2Long b) {
        return a.x != b.x || a.y != b.y;
    }

    public static Vector2Long operator *(Vector2Long a, Vector2Long b) {
        return new Vector2Long(a.x * b.x, a.y * b.y);
    }

    public static Vector2Long operator *(Vector2Int a, Vector2Long b) {
        return new Vector2Long(a.x * b.x, a.y * b.y);
    }

    public static Vector2Long operator *(Vector2Long a, Vector2Int b) {
        return new Vector2Long(a.x * b.x, a.y * b.y);
    }

    public static Vector2Long operator +(Vector2Long a, Vector2Long b) {
        return new Vector2Long(a.x + b.x, a.y + b.y);
    }

    public static Vector2Long operator +(Vector2Long a, Vector2Int b) {
        return new Vector2Long(a.x + b.x, a.y + b.y);
    }

    public static Vector2Long operator +(Vector2Int a, Vector2Long b) {
        return new Vector2Long(a.x + b.x, a.y + b.y);
    }

    public static Vector2Long operator -(Vector2Long a, Vector2Long b) {
        return new Vector2Long(a.x - b.x, a.y - b.y);
    }

    public static Vector2Long Get2DPosition(Vector3 pos, Vector2Long offset) {
        return new Vector2Long(((long)pos.x) + offset.x, ((long)pos.z) + offset.y);
    }

    public Vector3 ToWorldPosition(Vector2Long offset) {
        return new Vector3(x - offset.x, 0, y - offset.y);
    }

    /// <summary>
    /// Divides while mimicking floor (i.e. negative numbers round down)
    /// </summary>
    public Vector2Long DivideAndFloor(Vector2Long divisor) {
        long nx = x % divisor.x == 0 || x > 0 ? x / divisor.x : x / divisor.x - 1;
        long ny = y % divisor.y == 0 || y > 0 ? y / divisor.y : y / divisor.y - 1;
        return new Vector2Long(nx, ny);
    }

    /// <summary>
    /// Divides while mimicking floor (i.e. negative numbers round down)
    /// </summary>
    public Vector2Long DivideAndFloor(Vector2Int divisor) {
        long nx = x % divisor.x == 0 || x > 0 ? x / divisor.x : x / divisor.x - 1;
        long ny = y % divisor.y == 0 || y > 0 ? y / divisor.y : y / divisor.y - 1;
        return new Vector2Long(nx, ny);
    }
}
