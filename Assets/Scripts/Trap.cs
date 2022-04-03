using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public float attractRadius = 5f;

    private const int CheckRadiusTimeout = 60;
    private int _checkRadiusCooldown;
    private HashSet<Tile> _tiles = new HashSet<Tile>();

    void Awake()
    {
        _checkRadiusCooldown = CheckRadiusTimeout;

        var position = transform.position;
        var avoidRadius = attractRadius + 3;
        for (var x = -avoidRadius; x < avoidRadius; x++)
        {
            for (var y = -avoidRadius; y < avoidRadius; y++)
            {
                var tilePosition = (Vector2) position + new Vector2(x * Map.Current.tileSize, y * Map.Current.tileSize);
                if (Vector2.Distance(position, tilePosition) > avoidRadius) continue;
                var tile = Map.Current.GetTile(tilePosition);
                tile.isUnderTrap = true;
                _tiles.Add(tile);
            }
        }
    }
    
    private void FixedUpdate()
    {
        _checkRadiusCooldown--;
        if (_checkRadiusCooldown > 0) return;
        _checkRadiusCooldown = CheckRadiusTimeout;
        
        var colliders = Physics2D.OverlapCircleAll(transform.position, attractRadius);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Ant ant))
            {
                ant.CatchInTrap(transform.position);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var tile in _tiles)
        {
            tile.isUnderTrap = false;
        }
    }
}
