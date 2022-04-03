using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Fire : MonoBehaviour
{
    public float burnRadius = 1f;
    public float burnDuration = 10f;
    
    private Tile _tile;
    private float _burnTimer;

    private const int CheckRadiusTimeout = 30;
    private int _checkRadiusCooldown;

    private void Start()
    {
        _tile = Map.Current.GetTile(transform.position);
        _tile.isOnFire = true;
        _burnTimer = burnDuration + Random.Range(0, 5f);
    }

    private void Update()
    {
        burnDuration -= Time.deltaTime;
        if (burnDuration <= 0) Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        _checkRadiusCooldown--;
        if (_checkRadiusCooldown > 0) return;
        _checkRadiusCooldown = CheckRadiusTimeout;
        
        var colliders = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Ant ant))
            {
                ant.Fire();
                ant.health -= 0.1f * Time.deltaTime;
            }
        }
    }

    private void OnDestroy()
    {
        _tile.isOnFire = false;
    }
}
