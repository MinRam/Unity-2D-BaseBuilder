using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_TileGraph
{

    public Dictionary<Tile, Path_Node<Tile>> nodes;
    public Path_TileGraph(World world) {
        Debug.Log("Path_TileGraph");

        nodes = new Dictionary<Tile, Path_Node<Tile>>();

        for (int x = 0; x < world.Width; ++ x) {
            for (int y = 0; y < world.Height; ++ y) {
                Tile t = world.GetTileAt(x, y);

                if (t.movementCost > 0) {
                    Path_Node<Tile> n = new Path_Node<Tile>();

                    n.data = t;

                    nodes.Add(t, n);
                }
            }
        }

        // Debug.Log("Path_TileGraph: Created" + nodes.Count + " nodes.");

        // int edgeCount  = 0;

        foreach (Path_Node<Tile> n in nodes.Values) {
            getEdges(n);
        }

        // Debug.Log("Path_TileGraph: Created " + edgeCount + " nodes.");
    }

    public void Update_PathTileGraph(Tile t) {
        if ((t.movementCost > 0) && (!nodes.ContainsKey(t))) {
            Path_Node<Tile> n = new Path_Node<Tile>();
            n.data = t;
            nodes.Add(t, n);

            getEdges(n);

        } else if ((t.movementCost < 0) && (nodes.ContainsKey(t))) {
            nodes.Remove(t);
        }

        // update neighbours  node edges
        Tile[] neighbours = t.GetNeighbours(false);
        for (int i = 0; i < neighbours.Length; ++ i) {
            if (nodes.ContainsKey(neighbours[i])) {
                getEdges(nodes[neighbours[i]]);
            }
        }
    }

    private void getEdges(Path_Node<Tile> n) {
        if (n == null || n.data == null || n.data.movementCost <= 0) return;

        List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>>();

        Tile[] neighbours = n.data.GetNeighbours(false);
        for (int i = 0; i < neighbours.Length; ++ i) {
            if (neighbours[i] != null && neighbours[i].movementCost > 0) {
                Path_Edge<Tile> edge = new Path_Edge<Tile>();
                edge.cost = neighbours[i].movementCost;
                edge.node = nodes[neighbours[i]];
                edges.Add(edge);
            }
        }

        n.edges = edges.ToArray();
    }
}
