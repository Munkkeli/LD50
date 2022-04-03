using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static Map Current;
    
    public int Area => size * size;

    public int size = 64;
    public float tileSize = 1;

    private Tile[,] _tiles;
    
    // Start is called before the first frame update
    private void Awake()
    {
        var center = new Vector2(tileSize * (size / 2f), tileSize * (size / 2f));
        
        _tiles = new Tile[size, size];
        
        for (var x = 0; x < size; x++) {
            for (var y = 0; y < size; y++)
            {
                var position = center - new Vector2(x * tileSize, y * tileSize);
                var isObstacle = Physics.Raycast((Vector3)position + new Vector3(0, 0, -50), Vector3.forward, 50);
                _tiles[x, y] = new Tile(x, y, !isObstacle, position, 10);
            }
        }
        Map.Current = this;
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    public Tile GetTile(Vector2 point) {
        var center = new Vector2(tileSize * (size / 2f), tileSize * (size / 2f));
        var position = point + center;
        var percent = (position / tileSize) / size;
        var x = (int)((1f - Mathf.Clamp01(percent.x)) * size);
        var y = (int)((1f - Mathf.Clamp01(percent.y)) * size);
        return _tiles[Mathf.Clamp(x, 0, size - 1), Mathf.Clamp(y, 0, size - 1)];
    }

    public List<Tile> GetNeighbours(PathTile tile)
    {
        return GetNeighbours(tile.tile);
    }

    public List<Tile> GetNeighbours(Tile tile) {
        List<Tile> neighbours = new List<Tile>();
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 && y == 0) continue;
                int cx = tile.x + x;
                int cy = tile.y + y;
                if (cx >= 0 && cx < size && cy >= 0 && cy < size) {
                    neighbours.Add(_tiles[cx, cy]);
                }
            }
        }
        return neighbours;
    }

    private void OnDrawGizmos()
    {
        if (_tiles == null) return;
        
        foreach (var tile in _tiles)
        {
            if (tile.isWalkable)
            {
                Gizmos.color = tile.isInUse ? Color.yellow : tile.isUnderTrap ? Color.green : Color.gray;
                Gizmos.DrawWireCube(tile.position, new Vector3(tileSize, tileSize, 0));
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(tile.position, new Vector3(tileSize, tileSize, 0));
            }

        }
    }
}
