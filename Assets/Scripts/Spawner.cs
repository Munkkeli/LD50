using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject Entity;
    public float interval = 1f;

    private float _timer;
    
    // Start is called before the first frame update
    void Await()
    {
        _timer = interval;
    }

    // Update is called once per frame
    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer > 0f) return;

        Instantiate(Entity, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
        _timer = interval;
    }
}
