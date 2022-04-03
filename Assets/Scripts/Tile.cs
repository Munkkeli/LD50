using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTile : IHeapItem<PathTile>
{
    public Tile tile;
    
    public int gCost;
    public int hCost;
    public int fCost { get { return gCost + hCost; } }
    public PathTile parent;

    public int HeapIndex { get; set; }
    
    public PathTile(Tile tile)
    {
        this.tile = tile;
    }

    public int CompareTo(PathTile target) {
        var compare = fCost.CompareTo(target.fCost);
        if (compare == 0) {
            compare = hCost.CompareTo(target.hCost);
        }
        return -compare;
    }
}

public class Tile
{
    public int x;
    public int y;
    public bool isWalkable;
    public bool isInUse = false;
    public bool isOnFire = false;
    public bool isUnderTrap = false;
    public int weight;
    
    public Vector2 position;

    public Tile(int x, int y, bool isWalkable, Vector2 position, int weight) {
        this.x = x;
        this.y = y;
        this.isWalkable = isWalkable;
        this.position = position;
        this.weight = weight;
    }
}