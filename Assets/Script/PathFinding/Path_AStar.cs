using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class Path_AStar
{
    Queue<Tile> path;
    public Path_AStar(World world, Tile tileStart, Tile tileEnd, Func<Tile, Tile, bool> needDistance) {
        if (world.tileGraph == null) {
            world.tileGraph = new Path_TileGraph(world);
        }

        Dictionary<Tile, Path_Node<Tile>> nodes = world.tileGraph.nodes;

        if (!nodes.ContainsKey(tileStart)) {
            // Debug.LogError("Path_AStar: The starting tile isn't in the list of nodes.");
            return;
        }

        if (!nodes.ContainsKey(tileEnd)) {
            // Debug.LogError("Path_AStar: The ending tile isn't in the list of nodes.");
            return;
        }

        Path_Node<Tile> start = nodes[tileStart];
        Path_Node<Tile> end = nodes[tileEnd];

        List<Path_Node<Tile>> ClosedSet = new List<Path_Node<Tile>>();

        // List<Path_Node<Tile>> OpenSet = new List<Path_Node<Tile>>();
        // OpenSet.Add(start);
        SimplePriorityQueue<Path_Node<Tile>>  OpenSet = new SimplePriorityQueue<Path_Node<Tile>>();
        OpenSet.Enqueue(start, 0);


        Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From = new Dictionary<Path_Node<Tile>, Path_Node<Tile>>();

        Dictionary<Path_Node<Tile>, float> g_score = new Dictionary<Path_Node<Tile>, float>();

        foreach(Path_Node<Tile> n in nodes.Values) {
            g_score[n] = Mathf.Infinity;
        }

        g_score[start] = 0;

        Dictionary<Path_Node<Tile>, float> f_score = new Dictionary<Path_Node<Tile>, float>();
        foreach(Path_Node<Tile> n in nodes.Values) {
            f_score[n] = Mathf.Infinity;
        }
        f_score[start] = heuristic_cost_estimate(start, end);

        while(OpenSet.Count > 0) {
            Path_Node<Tile> current = OpenSet.Dequeue();

            if (needDistance(tileEnd, current.data)) {
                // TODO: return reconstruct path

                reconstruct_path(Came_From, current);

                return;
            }

            ClosedSet.Add(current);

            foreach(Path_Edge<Tile> edge_neighbor in current.edges) {
                Path_Node<Tile> neighbor = edge_neighbor.node;

                if (ClosedSet.Contains(neighbor)) continue;

                float tentative_g_score = g_score[current] + dist_between(current, neighbor) * neighbor.data.movementCost;

                bool needAdd = !OpenSet.Contains(neighbor);
                if (!needAdd && tentative_g_score >= g_score[neighbor])
                    continue;

                float heuristic_cost_neighbor = tentative_g_score + heuristic_cost_estimate(neighbor, end);

                Came_From[neighbor] = current;
                g_score[neighbor] = tentative_g_score;
                f_score[neighbor] = heuristic_cost_neighbor;

                if (needAdd)
                    OpenSet.Enqueue(neighbor, heuristic_cost_neighbor);
                else
                    OpenSet.UpdatePriority(neighbor, heuristic_cost_neighbor);
            }
        }
    }

    protected float heuristic_cost_estimate(Path_Node<Tile> a, Path_Node<Tile> b) {
        return Math.Abs(a.data.X - b.data.X) + Math.Abs(a.data.Y - b.data.Y);
    }

    protected float dist_between(Path_Node<Tile> a, Path_Node<Tile> b) {
        return Math.Abs(a.data.X - b.data.X) + Math.Abs(a.data.Y - b.data.Y);
    }

    protected void reconstruct_path(
        Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From,
        Path_Node<Tile> current
    ) {
        Queue<Tile> total_path = new Queue<Tile>();
        total_path.Enqueue(current.data);

        while ( Came_From.ContainsKey(current)) {
            current = Came_From[current];
            total_path.Enqueue(current.data);
        }

        path = new Queue<Tile>(total_path.Reverse());
    }

    public Tile Dequeue() {
        return path.Dequeue();
    }

    public int Length() {
        if (path == null)
            return 0;

        return path.Count;
    }
}
