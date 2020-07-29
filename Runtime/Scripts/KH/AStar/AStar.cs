using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.AStar {

	public class Point : IEquatable<Point> {
		public int x, y;
		public Point(int x, int y) { this.x = x; this.y = y; }

		public override bool Equals(object obj) {
			return Equals(obj as Point);
		}

		public bool Equals(Point other) {
			return other != null &&
				   x == other.x &&
				   y == other.y;
		}

		public override int GetHashCode() {
			int hashCode = 1502939027;
			hashCode = hashCode * -1521134295 + x.GetHashCode();
			hashCode = hashCode * -1521134295 + y.GetHashCode();
			return hashCode;
		}

		public override string ToString() {
			return "{" + x + ", " + y + "}";
		}
	}

	public class Finder {

		Dictionary<Point, bool> closedSet = new Dictionary<Point, bool>();
		Dictionary<Point, bool> openSet = new Dictionary<Point, bool>();

		// Computed cost of start to this point.
		Dictionary<Point, float> gScore = new Dictionary<Point, float>();
		// Projected cost of start to end, pasting through this point.
		Dictionary<Point, float> fScore = new Dictionary<Point, float>();

		bool allowDiagonal;

		Dictionary<Point, Point> nodeLinks = new Dictionary<Point, Point>();

		public List<Point> FindPath(float[,] graph, bool allowDiagonal, Point start, Point goal) {
			float minCost = float.MaxValue;
			for (int i = 0; i < graph.GetLength(0); i++) {
				for (int j = 0; j < graph.GetLength(1); j++) {
					if (graph[i,j] < minCost) {
						minCost = graph[i, j];
					}
				}
			}
			this.allowDiagonal = allowDiagonal;
			openSet[start] = true;
			gScore[start] = 0;
			fScore[start] = Heuristic(start, goal);

			while (openSet.Count > 0) {
				Point current = nextBest();

				// We're at the destination!
				if (current.Equals(goal)) {
					return Reconstruct(current);
				}

				openSet.Remove(current);
				closedSet[current] = true;

				foreach (Point neighbor in Neighbors(graph, current, allowDiagonal)) {
					if (closedSet.ContainsKey(neighbor)) continue;

					float projectedG = getGScore(current) + graph[current.y, current.x];

					if (!openSet.ContainsKey(neighbor)) openSet[neighbor] = true;
					else if (projectedG >= getGScore(neighbor)) continue;

					nodeLinks[neighbor] = current;
					gScore[neighbor] = projectedG;
					fScore[neighbor] = projectedG + Heuristic(neighbor, goal);
				}
			}

			return null;
		}

		private int Heuristic(Point start, Point goal) {
			int dx = goal.x - start.x;
			int dy = goal.y - start.y;
			return Math.Abs(dx) + Math.Abs(dy);
		}

		private float getGScore(Point pt) {
			float score = float.PositiveInfinity;
			gScore.TryGetValue(pt, out score);
			return score;
		}


		private float getFScore(Point pt) {
			float score = float.PositiveInfinity;
			fScore.TryGetValue(pt, out score);
			return score;
		}

		public static IEnumerable<Point> Neighbors(float[,] graph, Point center, bool allowDiagonal) {
			Point pt;

			if (allowDiagonal) {
				pt = new Point(center.x - 1, center.y - 1);
				if (IsValidNeighbor(graph, pt))
					yield return pt;
			}

			pt = new Point(center.x, center.y - 1);
			if (IsValidNeighbor(graph, pt))
				yield return pt;

			if (allowDiagonal) {
				pt = new Point(center.x + 1, center.y - 1);
				if (IsValidNeighbor(graph, pt))
					yield return pt;
			}

			//middle row
			pt = new Point(center.x - 1, center.y);
			if (IsValidNeighbor(graph, pt))
				yield return pt;

			pt = new Point(center.x + 1, center.y);
			if (IsValidNeighbor(graph, pt))
				yield return pt;


			//bottom row
			if (allowDiagonal) {
				pt = new Point(center.x - 1, center.y + 1);
				if (IsValidNeighbor(graph, pt))
					yield return pt;
			}

			pt = new Point(center.x, center.y + 1);
			if (IsValidNeighbor(graph, pt))
				yield return pt;

			if (allowDiagonal) {
				pt = new Point(center.x + 1, center.y + 1);
				if (IsValidNeighbor(graph, pt))
					yield return pt;
			}
		}

		public static bool IsValidNeighbor(float[,] matrix, Point pt) {
			if (pt.x < 0 || pt.x >= matrix.GetLength(1) || pt.y < 0 || pt.y >= matrix.GetLength(0))
				return false;

			return matrix[pt.y, pt.x] >= 0;
		}

		private List<Point> Reconstruct(Point current) {
			List<Point> path = new List<Point>();
			while (nodeLinks.ContainsKey(current)) {
				path.Add(current);
				current = nodeLinks[current];
			}

			path.Reverse();
			return path;
		}

		private Point nextBest() {
			float best = float.PositiveInfinity;
			Point bestPt = null;
			foreach (Point node in openSet.Keys) {
				float score = getFScore(node);
				if (score < best) {
					bestPt = node;
					best = score;
				}
			}

			return bestPt;
		}

		public static List<Point> Simplify(float[,] graph, List<Point> path) {
			if (path == null) return null;
			if (path.Count < 2) return path;

			List<Point> simplifiedPath = new List<Point> {
				path[0]
			};
			Point lastPoint = path[0];
			for (int i = 1; i < path.Count; i++) {
				if (!CanSquash(graph, lastPoint, path[i])) {
					lastPoint = path[i-1];
					simplifiedPath.Add(lastPoint);
				}
			}
			if (lastPoint != path[path.Count - 1]) simplifiedPath.Add(path[path.Count - 1]);
			return simplifiedPath;
		}

		/// <summary>
		/// Returns whether to points can be merged. If not adjacent (diagonally
		/// or cardinally), assumes that points leading up to that point could
		/// already be merged. E.g. if {0,0} and {0,2} are passed in, it assumes
		/// that {0,1} could be merged with {0,0} (traversable).
		/// </summary>
		public static bool CanSquash(float[,] graph, Point pt1, Point pt2) {
			if (pt1.x == pt2.x || pt1.y == pt2.y) return true;

			int fromX = Mathf.Min(pt1.x, pt2.x);
			int toX = Mathf.Max(pt1.x, pt2.x);
			int fromY = Mathf.Min(pt1.y, pt2.y);
			int toY = Mathf.Max(pt1.y, pt2.y);
			for (int x = fromX; x <= toX; x++) {
				for (int y = fromY; y <= toY; y++) {
					if (graph[y, x] < 0) return false;
				}
			}
			return true;
		}
	}
}