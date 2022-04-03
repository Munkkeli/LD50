using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Hairspray : MonoBehaviour
{
    public int sprayRadius = 10;
    public float spraySpeed = 1f;
    public float sprayAmount = 20f;
    public float sprayRecovery = 0.2f;

    public GameObject fire;
    
    private float _sprayTimer;
    private float _sprayAmount;
    private Tile _tile;
    private HashSet<Tile> _tiles = new HashSet<Tile>();

    private void Awake()
    {
        _sprayAmount = sprayAmount;
    }

    void Update()
    {
        if (Controller.Current.tool == Tool.HAIRSPRAY && Input.GetMouseButton(0))
        {
            _sprayTimer -= Time.deltaTime;

            if (_sprayTimer <= 0)
            {
                _sprayTimer = spraySpeed;

                if (_sprayAmount <= 1f) return;
                _sprayAmount -= 1f;
                
                var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var lastTile = _tile;
                _tile = Map.Current.GetTile(position);

                if (_tile != lastTile)
                {
                    _tiles.Clear();
                    for (var x = -sprayRadius; x < sprayRadius; x++)
                    {
                        for (var y = -sprayRadius; y < sprayRadius; y++)
                        {
                            _tiles.Add(Map.Current.GetTile((Vector2)position + new Vector2(x * Map.Current.tileSize, y * Map.Current.tileSize)));
                        }
                    }
                }

                var tiles = _tiles.Where(tile => !tile.isOnFire).ToArray();
                if (!tiles.Any()) return;

                var tile = tiles[Random.Range(0, tiles.Length)];
                Instantiate(fire, tile.position, Quaternion.identity);
                tile.isOnFire = true;
            }
        }
        else
        {
            _sprayTimer = 0;
        }
        
            _sprayAmount = Mathf.Min(sprayAmount, _sprayAmount + Time.deltaTime * sprayRecovery);
    }

    private void OnDestroy()
    {
        
    }
}
