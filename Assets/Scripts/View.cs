using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour
{
    public static View Current;

    public bool IsTracking => _mouseGrabPosition != null;

    private Camera _camera;
    private Vector2? _mouseGrabPosition;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        Current = this;
    }

    private void LateUpdate()
    {
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

        var zoom = Mathf.Clamp(_camera.orthographicSize - Input.mouseScrollDelta.y * 2f, 10f, 30f);
        _camera.orthographicSize = zoom;
    }
}
