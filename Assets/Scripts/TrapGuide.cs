using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapGuide : MonoBehaviour
{
    private void Update()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, transform.position.z);
    }
}
