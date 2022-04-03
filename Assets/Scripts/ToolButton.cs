using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToolButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public bool isHovering = false;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
}