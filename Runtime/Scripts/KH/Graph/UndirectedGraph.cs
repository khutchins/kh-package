using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace KH.Graph {
    public class UndirectedGraph<N> {

        public class Edge {
            public Node N1;
            public Node N2;
            public float Cost;

			public Edge(Node n1, Node n2, float cost) {
				N1 = n1;
				N2 = n2;
				Cost = cost;
			}

			public Node OtherNode(Node node) {
				return N1 == node ? N2 : N1;
			}
		}

        public class Node {
            public N Element;
            public List<Edge> Edges = new List<Edge>();
			public readonly int Index;

            public Node(N element, int idx) {
                Element = element;
				Index = idx;
            }

            public void AddEdge(Edge edge) {
                Edges.Add(edge);
			}

			public IEnumerable<Node> Neighbors() {
				foreach(Edge edge in Edges) {
					yield return edge.OtherNode(this);
				}
			}
        }

        public List<Node> Nodes;
		private int _nextIndex;

        public UndirectedGraph() {
            Nodes = new List<Node>();
        }

        public Node AddNode(N element) {
            Node node = new Node(element, _nextIndex++);
            Nodes.Add(node);
            return node;
        }

		/// <summary>
		/// Add an undirected edge between two nodes.
		/// </summary>
        public void AddEdge(Node n1, Node n2, float cost) {
            Edge edge = new Edge(n1, n2, cost);
            n1.AddEdge(edge);
            n2.AddEdge(edge);
		}

		/// <summary>
		/// Returns a list of edges that define the minimum spanning tree for the graph.
		/// </summary>
		/// <returns>A list of tuples of {coord 1, coord2}</returns>
		public List<Edge> MinimumSpanningTree() {
			HashSet<Edge> edges = new HashSet<Edge>();
			List<int> regions = new List<int>();

			foreach(Node n in Nodes) {
				edges.UnionWith(n.Edges);
				regions.Add(n.Index);
			}

			List<Edge> priorityEdges = edges.OrderBy(x => x.Cost).ToList();

			List<Edge> chosenEdges = new List<Edge>();

			foreach (Edge edge in edges.OrderBy(x => x.Cost)) {
				// Nodes already connected.
				if (regions[edge.N1.Index] == regions[edge.N2.Index]) {
					continue;
				}

				chosenEdges.Add(edge);
				int newRegion = regions[edge.N1.Index];
				int oldRegion = regions[edge.N2.Index];
				for (int i = 0; i < regions.Count; i++) {
					if (regions[i] == oldRegion) {
						regions[i] = newRegion;
					}
				}

				bool differentRegions = false;
				foreach (int region in regions) {
					if (region != newRegion) {
						differentRegions = true;
						break;
					}
				}
				if (!differentRegions) break;
			}

			return chosenEdges;
		}
	}
}