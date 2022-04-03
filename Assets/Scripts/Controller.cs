using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Tool
{
    FINGER = 1,
    HAIRSPRAY = 2,
    TRAP = 3,
}

public class Controller : MonoBehaviour
{
    public static Controller Current;
    
    public Tool tool = Tool.FINGER;

    public Finger finger;
    public Hairspray hairspray;
    public GameObject trap;

    public Button fingerButton;
    public Button hairsprayButton;
    public Button trapButton;
    
    private int _availableTraps = 3;
    private HashSet<Trap> _trapsInUse = new HashSet<Trap>();

    private void Awake()
    {
        Current = this;
    }

    private void Start()
    {
        fingerButton.onClick.AddListener(() =>
        {
            tool = Tool.FINGER;
        });
        
        hairsprayButton.onClick.AddListener(() =>
        {
            tool = Tool.HAIRSPRAY;
        });
        
        trapButton.onClick.AddListener(() =>
        {
            tool = Tool.TRAP;
        });
    }

    private void LateUpdate()
    {
        if (tool == Tool.FINGER)
        {
            if (Input.GetMouseButtonDown(0)) finger.Tap(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        if (tool == Tool.TRAP)
        {
            var freeTrapSlots = _availableTraps - _trapsInUse.Count;
            if (Input.GetMouseButtonUp(0) && !View.Current.IsTracking && freeTrapSlots > 0)
            {
                var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var trapGameObject = Instantiate(trap, new Vector3(position.x, position.y, 0), Quaternion.identity);
                _trapsInUse.Add(trapGameObject.GetComponent<Trap>());
            }
        }
    }
}
