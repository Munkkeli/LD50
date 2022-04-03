using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum State
{
    INTRO = 1,
    TUTORIAL = 2,
    GAME = 3,
    FAIL = 4,
}

public class View : MonoBehaviour
{
    public static View Current;

    public AudioClip[] birdSounds;
    public AudioClip[] rustleSounds;
    public RectTransform intro;
    public GameObject fail;
    public Button startButton;
    public Button readyButton;
    public Button againButton;
    public float tutorialYPosition = 0f;
    
    public bool IsTracking => _mouseGrabPosition != null;

    private Camera _camera;
    private Vector2? _mouseGrabPosition;
    private float _birdSoundCooldown;
    private float _rustleSoundCooldown;
    public State state = State.INTRO;
    private float _introAnimationRef;

    private void Awake()
    {
        _birdSoundCooldown = 4f;
        _rustleSoundCooldown = 1f;
        _camera = GetComponent<Camera>();
        Current = this;
        
        fail.SetActive(false);
        
        startButton.onClick.AddListener((() =>
        {
            state = State.TUTORIAL;
        }));
        
        readyButton.onClick.AddListener((() =>
        {
            state = State.GAME;
            Destroy(intro.gameObject);
        }));
        
        againButton.onClick.AddListener((() =>
        {
            SceneManager.LoadScene( SceneManager.GetActiveScene().name );
        }));
    }

    private void Update()
    {
        if (Controller.Current.cakeHealth <= 0)
        {
            state = State.FAIL;
            Controller.Current.cakeHealth = 0;
            Time.timeScale = 0;
        }

        if (state == State.FAIL)
        {
            fail.SetActive(true);
            return;
        }

        _birdSoundCooldown -= Time.deltaTime;
        if (_birdSoundCooldown <= 0)
        {
            var position = new Vector2(Random.Range(-64f, 64f), Random.Range(-64f, 64f));
            AudioSource.PlayClipAtPoint(birdSounds[Random.Range(0, birdSounds.Length)], position, Random.Range(2f, 4f));
            _birdSoundCooldown = Random.Range(3f, 10f);
        }
        
        _rustleSoundCooldown -= Time.deltaTime;
        if (_rustleSoundCooldown <= 0)
        {
            var position = new Vector2(Random.Range(-64f, 64f), Random.Range(-64f, 64f));
            AudioSource.PlayClipAtPoint(rustleSounds[Random.Range(0, rustleSounds.Length)], position, Random.Range(1f, 2f));
            _rustleSoundCooldown = Random.Range(4f, 8f);
        }

        if (state == State.INTRO || state == State.TUTORIAL)
        {
            var y = Mathf.SmoothDamp(intro.localPosition.y, state == State.TUTORIAL ? tutorialYPosition : 0,
                ref _introAnimationRef, 0.5f);
            intro.localPosition = new Vector3(intro.localPosition.x, y, intro.localPosition.z);
        }
    }

    private void LateUpdate()
    {
        if (state != State.GAME) return;
            
        var mousePosition = (Vector2)Input.mousePosition;
        if (Input.GetMouseButtonDown(1) && _mouseGrabPosition == null)
        {
            _mouseGrabPosition = _camera.ScreenToWorldPoint(mousePosition);
        }
        if (Input.GetMouseButtonUp(1)) _mouseGrabPosition = null;

        if (_mouseGrabPosition != null && Input.GetMouseButton(1))
        {
            var mouseWorldTranslation = (Vector2)transform.position - (Vector2)_camera.ScreenToWorldPoint(mousePosition);
            var position = (Vector2)_mouseGrabPosition + mouseWorldTranslation;
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        var scale = (Map.Current.size / 2) * Map.Current.tileSize;
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -scale, scale),
            Mathf.Clamp(transform.position.y, -scale, scale), transform.position.z);

        var zoom = Mathf.Clamp(_camera.orthographicSize - Input.mouseScrollDelta.y * 2f, 10f, 26f);
        _camera.orthographicSize = zoom;
    }
}
