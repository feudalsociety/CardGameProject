using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;

// TODO : This logic might be go inside cloud code
public class Pathfinding 
{
    // Uses BFS. should be called only in server
    public static List<HexCoords> FindAllTilesWithinPathLength(Tile startNode, int pathLength)
    {
        var frontierQueue = new Queue<Tile>();
        frontierQueue.Enqueue(startNode);
        var distance = new Dictionary<Tile, int>();
        distance[startNode] = 0;

        var results = new List<HexCoords>();

        while(frontierQueue.Any())
        {
            var current = frontierQueue.Dequeue();
            foreach (var neighbor in current.Neighbors.Where(t => t.Walkable))
            {
                if(!distance.ContainsKey(neighbor))
                {
                    frontierQueue.Enqueue(neighbor);
                    distance[neighbor] = 1 + distance[current];
                    if(distance[neighbor] <= pathLength) results.Add(neighbor.Coord);
                }
            }
        }

        return results;
    }

    public static List<Tile> FindPath(Tile startNode, Tile targetNode)
    {
        // TODO : Use Tree to improve 
        var toSearch = new List<Tile>() { startNode };
        var processed = new List<Tile>();

        while(toSearch.Any())
        {
            var current = toSearch[0];
            foreach(var t in toSearch)
            {
                if(t.F < current.F || (t.F == current.F && t.H < current.H)) current = t;
            }

            processed.Add(current);
            toSearch.Remove(current);

            if(current == targetNode)
            {
                var currentPathTile = targetNode;
                var path = new List<Tile>();
                while(currentPathTile != startNode)
                {
                    path.Add(currentPathTile);
                    currentPathTile = currentPathTile.Connection;
                }
                path.Add(startNode);

                return path;
            }

            // current is best F cost node, loop over this neighbors
            foreach(var neighbor in current.Neighbors.Where(t => t.Walkable && !processed.Contains(t)))
            {
                var inSearch = toSearch.Contains(neighbor);

                var costToNeigbor = current.G + current.GetDistance(neighbor);

                if(!inSearch || costToNeigbor < neighbor.G)
                {
                    neighbor.SetG(costToNeigbor);
                    neighbor.SetConnection(current);

                    if(!inSearch)
                    {
                        neighbor.SetH(neighbor.GetDistance(targetNode));
                        toSearch.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }
}
