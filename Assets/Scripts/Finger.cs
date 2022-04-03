using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finger : MonoBehaviour
{
    public SpriteRenderer finger;
    public bool isTapping = false;
    public Vector2 animationPosition;

    private Vector2 _initialPosition;
    private float _tapAnimation = 0f;
    
    public void Tap(Vector2 position)
    {
        if (isTapping) return;
        transform.position = position;
        isTapping = true;
        
        var colliders = Physics2D.OverlapCircleAll(position, 0.2f);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Ant ant))
            {
                ant.health -= 1f;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _initialPosition = finger.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (isTapping)
        {
            _tapAnimation += Time.deltaTime * 10f;
            
            var curve = 1f - _tapAnimation;
            finger.color = new Color(1f, 1f, 1f, 1f - Mathf.Abs(curve));
            finger.transform.localPosition = Vector2.Lerp(_initialPosition, animationPosition, Mathf.Abs(curve));

            if (_tapAnimation >= 2f)
            {
                isTapping = false;
                _tapAnimation = 0f;
            }
        }
    }
}
