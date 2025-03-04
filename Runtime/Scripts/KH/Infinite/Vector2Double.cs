using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vector2Double {
    public double x;
    public double y;

    public Vector2Double(double x, double y) {
        this.x = x;
        this.y = y;
    }

    public static Vector2Double zero {
        get => new Vector2Double(0, 0);
    }

    public static Vector2Double one {
        get => new Vector2Double(1, 1);
    }

    public override bool Equals(object obj) {
        return obj is Vector2Double @double &&
               x == @double.x &&
               y == @double.y;
    }

    public override int GetHashCode() {
        return HashCode.Combine(x, y);
    }

    public override string ToString() {
        return $"({x}, {y})";
    }

    public static double Distance(Vector2Double a, Vector2Double b) {
        return Math.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
    }

    public static bool operator ==(Vector2Double a, Vector2Double b) {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Vector2Double a, Vector2Double b) {
        return a.x != b.x || a.y != b.y;
    }

    public static Vector2Double operator *(Vector2Double a, Vector2Double b) {
        return new Vector2Double(a.x * b.x, a.y * b.y);
    }

    public static Vector2Double operator *(Vector2Int a, Vector2Double b) {
        return new Vector2Double(a.x * b.x, a.y * b.y);
    }

    public static Vector2Double operator *(Vector2Double a, Vector2Int b) {
        return new Vector2Double(a.x * b.x, a.y * b.y);
    }

    public static Vector2Double operator +(Vector2Double a, Vector2Double b) {
        return new Vector2Double(a.x + b.x, a.y + b.y);
    }

    public static Vector2Double operator +(Vector2Double a, Vector2Int b) {
        return new Vector2Double(a.x + b.x, a.y + b.y);
    }

    public static Vector2Double operator +(Vector2Int a, Vector2Double b) {
        return new Vector2Double(a.x + b.x, a.y + b.y);
    }

    public static Vector2Double operator -(Vector2Double a, Vector2Double b) {
        return new Vector2Double(a.x - b.x, a.y - b.y);
    }

    public static Vector2Double Get2DPosition(Vector3 pos, Vector2Double offset) {
        return new Vector2Double(((double)pos.x) + offset.x, ((double)pos.z) + offset.y);
    }
}
