using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace KH.Graph {
	public class DirectedGraph<N> {

		public class Edge {
			public Node N1;
			public Node N2;
			public float Cost;

			public Edge(Node n1, Node n2, float cost) {
				N1 = n1;
				N2 = n2;
				Cost = cost;
			}
		}

		public class Node {
			public N Element;
			public List<Edge> Edges = new List<Edge>();
			public List<Edge> IncomingEdges = new List<Edge>();
			public readonly int Index;

			public Node(N element, int idx) {
				Element = element;
				Index = idx;
			}

			public void AddEdge(Edge edge) {
				Edges.Add(edge);
			}

			public void AddIncomingEdge(Edge edge) {
				Edges.Add(edge);
			}

			public IEnumerable<Node> Neighbors() {
				foreach (Edge edge in Edges) {
					yield return edge.N2;
				}
			}
		}

		public List<Node> Nodes;
		private int _nextIndex;

		public DirectedGraph() {
			Nodes = new List<Node>();
		}

		public Node AddNode(N element) {
			Node node = new Node(element, _nextIndex++);
			Nodes.Add(node);
			return node;
		}

		/// <summary>
		/// Add a directed edge between two nodes.
		/// </summary>
		public void AddEdge(Node n1, Node n2, float cost) {
			Edge edge = new Edge(n1, n2, cost);
			n1.AddEdge(edge);
			n2.AddIncomingEdge(edge);
		}

		/// <summary>
		/// Returns the shortest path between two points, or null if none exists.
		/// </summary>
		/// <param name="start">The starting node.</param>
		/// <param name="goal">The terminal node.</param>
		/// <param name="heuristic">Lambda for computing projected cost between two nodes.</param>
		/// <returns></returns>
		public List<Node> FindPath(Node start, Node goal, Func<N, N, float> heuristic) {
			Dictionary<Node, bool> closedSet = new Dictionary<Node, bool>();
			Dictionary<Node, bool> openSet = new Dictionary<Node, bool>();

			// Computed cost of start to this point.
			Dictionary<Node, float> gScore = new Dictionary<Node, float>();
			// Projected cost of start to end, pasting through this point.
			Dictionary<Node, float> fScore = new Dictionary<Node, float>();

			Dictionary<Node, Node> nodeLinks = new Dictionary<Node, Node>();

			openSet[start] = true;
			gScore[start] = 0;
			fScore[start] = heuristic(start.Element, goal.Element);

			while (openSet.Count > 0) {
				float best = float.PositiveInfinity;
				Node bestNode = null;
				foreach (Node node in openSet.Keys) {
					float score = fScore.ContainsKey(node) ? fScore[node] : float.PositiveInfinity;
					if (score < best) {
						bestNode = node;
						best = score;
					}
				}

				Node current = bestNode;

				// We're at the destination!
				if (current.Equals(goal)) {
					List<Node> path = new List<Node>();
					while (nodeLinks.ContainsKey(current)) {
						path.Add(current);
						current = nodeLinks[current];
					}

					path.Reverse();
					return path;
				}

				openSet.Remove(current);
				closedSet[current] = true;

				foreach (Edge edge in current.Edges) {
					Node neighbor = edge.N2;
					if (closedSet.ContainsKey(neighbor)) continue;

					float currentG = gScore.ContainsKey(current) ? gScore[current] : float.PositiveInfinity;
					float projectedG = currentG + edge.Cost;

					float neighborG = gScore.ContainsKey(neighbor) ? gScore[neighbor] : float.PositiveInfinity;

					if (!openSet.ContainsKey(neighbor)) openSet[neighbor] = true;
					else if (projectedG >= neighborG) continue;

					nodeLinks[neighbor] = current;
					gScore[neighbor] = projectedG;
					fScore[neighbor] = projectedG + heuristic(neighbor.Element, goal.Element);
				}
			}

			return null;
		}

		public List<Node> FindPathToFarthestNode(Node start) {
			Dictionary<Node, bool> closedSet = new Dictionary<Node, bool>();
			Dictionary<Node, bool> openSet = new Dictionary<Node, bool>();

			// Computed cost of start to this point.
			Dictionary<Node, float> cost = new Dictionary<Node, float>();

			Dictionary<Node, Node> nodeLinks = new Dictionary<Node, Node>();

			openSet[start] = true;
			cost[start] = 0;

			Node farthestNode = start;
			float highestCost = 0;

			while (openSet.Count > 0) {
				Node current = openSet.Keys.First();
				float lowestCost = float.PositiveInfinity;

				// Evaluate the node with the lowest cost next.
				// This is required so that once we evaluate a
				// node, it cannot have come from a lower source
				// from a different node (barring negative cost keys).
				foreach (Node node in openSet.Keys) {
					float score = cost.ContainsKey(node) ? cost[node] : float.PositiveInfinity;
					if (score < lowestCost) {
						current = node;
						lowestCost = score;
					}
				}

				openSet.Remove(current);
				closedSet[current] = true;
				float currentCost = cost[current];
				if (currentCost > highestCost) {
					highestCost = currentCost;
					farthestNode = current;
				}

				foreach (Edge edge in current.Edges) {
					Node neighbor = edge.N2;
					if (closedSet.ContainsKey(neighbor)) continue;

					float currentG = cost.ContainsKey(current) ? cost[current] : float.PositiveInfinity;
					float projectedG = currentG + edge.Cost;

					float neighborG = cost.ContainsKey(neighbor) ? cost[neighbor] : float.PositiveInfinity;

					if (!openSet.ContainsKey(neighbor)) openSet[neighbor] = true;
					else if (projectedG >= neighborG) continue;

					nodeLinks[neighbor] = current;
					cost[neighbor] = projectedG;
				}
			}

			// Construct path from farthest to start
			List<Node> path = new List<Node>();
			while (nodeLinks.ContainsKey(farthestNode)) {
				path.Add(farthestNode);
				farthestNode = nodeLinks[farthestNode];
			}

			path.Reverse();
			return path;
		}
	}
}