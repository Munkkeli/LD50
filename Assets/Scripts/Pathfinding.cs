using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

internal struct PathRequest {
    public readonly Vector2 Start;
    public readonly Vector2 End;
    public readonly GameObject GameObject;
    public readonly Action<Vector2[], bool> Callback;

    public PathRequest(Vector2 start, Vector2 end, GameObject gameObject, Action<Vector2[], bool> callback) {
        this.Start = start;
        this.End = end;
        this.GameObject = gameObject;
        this.Callback = callback;
    }
}
public class Pathfinding : MonoBehaviour
{
    private static Pathfinding _current;
    
    private readonly Queue<PathRequest> _queue = new Queue<PathRequest>();
    private int _isProcessingCount = 0;
    
    public static void RequestPath(Vector2 start, Vector2 end, GameObject gameObject, Action<Vector2[], bool> callback) {
        var request = new PathRequest(start, end, gameObject, callback);
        _current.FindPath(request);
    }

    private void Awake()
    {
        _current = this;
    }

    private void FindPath(PathRequest request)
    {
        if (_isProcessingCount > 10 || _queue.Count > 0)
        {
            _queue.Enqueue(request);
        }
        else
        {
            StartCoroutine(_FindPathRoutine(request));
        }
    }

    private IEnumerator _FindPathRoutine(PathRequest request)
    {
        _isProcessingCount++;
        
        var startTile = Map.Current.GetTile(request.Start);
        var endTile = Map.Current.GetTile(request.End);
        var isSuccess = false;
        
        // Debug.Log($"Searching for path from ({request.Start}) to ({request.End}) ...");

        var tileToPathTile = new Dictionary<Tile, PathTile>();

        tileToPathTile.Add(startTile, new PathTile(startTile));
        tileToPathTile.Add(endTile, new PathTile(endTile));
        
        if (startTile.isWalkable && endTile.isWalkable) {
            var openSet = new Heap<PathTile>(Map.Current.Area);
            var closedSet = new HashSet<PathTile>();
            openSet.Add(tileToPathTile[startTile]);

            while (openSet.Count > 0) {
                var currentTile = openSet.RemoveFirst();
                closedSet.Add(currentTile);

                if (currentTile == tileToPathTile[endTile]) {
                    isSuccess = true;
                    break;
                }

                foreach (var neighbour in Map.Current.GetNeighbours(currentTile)) {
                    if (!tileToPathTile.ContainsKey(neighbour)) tileToPathTile.Add(neighbour, new PathTile(neighbour));

                    var pathTile = tileToPathTile[neighbour];
                    if (!neighbour.isWalkable || closedSet.Contains(pathTile)) continue;

                    var inUseCost = neighbour.isInUse ? 5 : 0;
                    var inFireCost = neighbour.isOnFire ? 100 : 0;
                    var isUnderTrapCost = neighbour.isUnderTrap ? Random.Range(15, 125) : 0;
                    
                    var costToNeighbour = currentTile.gCost + GetDistance(currentTile, pathTile) + neighbour.weight + inUseCost + inFireCost + isUnderTrapCost;
                    if (costToNeighbour >= pathTile.gCost && openSet.Contains(pathTile)) continue;
                    
                    pathTile.gCost = costToNeighbour;
                    pathTile.hCost = GetDistance(neighbour, endTile);
                    pathTile.parent = currentTile;

                    if (!openSet.Contains(pathTile)) {
                        openSet.Add(pathTile);
                    } else {
                        openSet.UpdateItem(pathTile);
                    }
                }
            }
        }
        
        yield return null;

        _isProcessingCount--;
        
        var waypoints = Array.Empty<Vector2>();
        if (isSuccess) {
            waypoints = RetracePath(tileToPathTile[startTile], tileToPathTile[endTile]);
        }

        // Check if the requesting gameObject still exists, if it does, return path
        if (request.GameObject != null) request.Callback(waypoints, isSuccess);
        
        // Check if queue still contains requests, if it does, do the next one
        if (_queue.Count > 0)
        {
            StartCoroutine(_FindPathRoutine(_queue.Dequeue()));
        }
    }

    private Vector2[] RetracePath(PathTile startTile, PathTile endTile) {
        var path = new List<Tile>();
        var currentTile = endTile;
        while (currentTile != startTile) {
            path.Add(currentTile.tile);
            currentTile = currentTile.parent;
        }

        // Debug.Log($"Found path {path.Count}");

        var waypoints = SimplifyPath(path);
        waypoints.Reverse();
        return waypoints.ToArray();
    }

    static int GetDistance(PathTile a, PathTile b)
    {
        return GetDistance(a.tile, b.tile);
    }
    
    static int GetDistance(Tile a, Tile b) {
        var dx = Mathf.Abs(a.x - b.x);
        var dy = Mathf.Abs(a.y - b.y);
        if (dx > dy) return 14 * dy + 10 * (dx - dy);
        return 14 * dx + 10 * (dy - dx);
    }

    static List<Vector2> SimplifyPath(List<Tile> path) {
        List<Vector2> waypoints = new List<Vector2>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++) {
            Vector2 directionNew = new Vector2(path[i - 1].x - path[i].x, path[i - 1].y - path[i].y);
            if (directionNew != directionOld) waypoints.Add(path[i].position);
            directionOld = directionNew;
        }

        return waypoints;
    }
}
