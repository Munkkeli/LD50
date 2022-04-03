using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject Entity;
    public float interval = 1f;
    public bool isNest = false;
    
    private Collider2D _spawnArea;
    private float _timer;
    
    private void Awake()
    {
        _spawnArea = GetComponent<Collider2D>();
        _timer = interval;
    }
    
    private void Update()
    {
        var score = Controller.Current.score;
        interval = Mathf.Max(0.5f - (score / 1000f), 0.3f);
        
        if (isNest) interval = Mathf.Max(5f - (score / 3000f), 4f);
        
        _timer -= Time.deltaTime;
        if (_timer > 0f) return;

        var bounds = _spawnArea.bounds;
        var position = new Vector2(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y));

        var antGameObject = Instantiate(Entity, position, Quaternion.identity);
        antGameObject.GetComponent<Ant>().isFromNest = isNest;
        _timer = interval;
    }
}
