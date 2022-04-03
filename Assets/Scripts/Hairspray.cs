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

    public AudioClip sprayStart;
    public AudioClip sprayEnd;

    public GameObject fire;

    public Transform hairsprayEffect;
    private ParticleSystem _particleEffect;

    private AudioSource _audioSource;
    private float _sprayTimer;
    public float _sprayAmount;
    private float _sprayStartSoundDelay;
    private bool _hasStartPlayed = false;
    private bool _isSpraying;
    private Tile _tile;
    private HashSet<Tile> _tiles = new HashSet<Tile>();

    private void Awake()
    {
        _particleEffect = hairsprayEffect.gameObject.GetComponentInChildren<ParticleSystem>();
        _audioSource = GetComponent<AudioSource>();
        _sprayAmount = sprayAmount;
    }

    void Update()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        hairsprayEffect.transform.position = new Vector3(mousePosition.x, mousePosition.y, hairsprayEffect.transform.position.z);
        
        if (Controller.Current.tool == Tool.HAIRSPRAY && Input.GetMouseButton(0))
        {
            if (Controller.Current.buttonClickCooldown > 0) return;
            if (Controller.Current.isHoveringButton) return;

            _isSpraying = true;

            _sprayStartSoundDelay = Mathf.Max(0f, _sprayStartSoundDelay - Time.deltaTime);
            _sprayTimer -= Time.deltaTime;

            if (_sprayAmount <= 0) _isSpraying = false;
            
            if (_isSpraying && !_particleEffect.isPlaying) _particleEffect.Play();
            
            if (_isSpraying && _hasStartPlayed && !_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
            
            if (_isSpraying && !_hasStartPlayed)
            {
                _audioSource.PlayOneShot(sprayStart);
                _hasStartPlayed = true;
            }

            if (_sprayTimer <= 0)
            {
                _sprayTimer = spraySpeed;

                if (_sprayAmount <= 0f) return;
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
            _isSpraying = false;
            _sprayTimer = 0;
            _sprayAmount = Mathf.Min(sprayAmount, _sprayAmount + Time.deltaTime * sprayRecovery);
        }

        if (!_isSpraying)
        {
            if (_hasStartPlayed)
            {
                _audioSource.Stop();
                _audioSource.PlayOneShot(sprayEnd);
                _hasStartPlayed = false;
            }
            
            if (_particleEffect.isPlaying) _particleEffect.Stop();
        }
    }

    private void OnDestroy()
    {
        
    }
}
