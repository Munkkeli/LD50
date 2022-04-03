using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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

    public int cakeHealth = 20;
    public int score = 0;
    
    public Tool tool = Tool.FINGER;

    public Finger finger;
    public Hairspray hairspray;
    public GameObject trap;

    public GameObject trapGuide;
    
    public Text cakeText;
    public Text scoreText;
    public ToolButton fingerButton;
    public ToolButton hairsprayButton;
    public ToolButton trapButton;
    
    public Sprite buttonNormal;
    public Sprite buttonActive;

    public bool isHoveringButton;
    public float buttonClickCooldown = 0f;
    private int _availableTraps {
        get
        {
            if (score > 700) return 3;
            if (score > 400) return 2;
            if (score > 150) return 1;
            return 0;
        }
    }
    private HashSet<Trap> _trapsInUse = new HashSet<Trap>();

    private void Awake() {
        Current = this;
    }

    private void Start()
    {
        trapGuide.SetActive(false);
        fingerButton.GetComponent<Image>().sprite = buttonActive;
        
        fingerButton.button.onClick.AddListener(() =>
        {
            tool = Tool.FINGER;
            buttonClickCooldown = 0.2f;

            fingerButton.GetComponent<Image>().sprite = buttonActive;
            hairsprayButton.GetComponent<Image>().sprite = buttonNormal;
            trapButton.GetComponent<Image>().sprite = buttonNormal;
        });

        hairsprayButton.button.onClick.AddListener(() =>
        {
            tool = Tool.HAIRSPRAY;
            buttonClickCooldown = 0.2f;
            
            fingerButton.GetComponent<Image>().sprite = buttonNormal;
            hairsprayButton.GetComponent<Image>().sprite = buttonActive;
            trapButton.GetComponent<Image>().sprite = buttonNormal;
        });
        
        trapButton.button.onClick.AddListener(() =>
        {
            tool = Tool.TRAP;
            buttonClickCooldown = 0.2f;
            
            fingerButton.GetComponent<Image>().sprite = buttonNormal;
            hairsprayButton.GetComponent<Image>().sprite = buttonNormal;
            trapButton.GetComponent<Image>().sprite = buttonActive;
        });
        
        fingerButton.gameObject.SetActive(false);
        hairsprayButton.gameObject.SetActive(false);
        trapButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        isHoveringButton = fingerButton.isHovering || hairsprayButton.isHovering || trapButton.isHovering;
        cakeText.text = "" + cakeHealth;
        scoreText.text = "" + score;
    }

    private void LateUpdate()
    {
        if (View.Current.state != State.GAME) return;
        
        fingerButton.gameObject.SetActive(true);
        

        var freeTrapSlots = _availableTraps - _trapsInUse.Count;
        trapButton.gameObject.SetActive( freeTrapSlots > 0);
        trapGuide.SetActive(tool == Tool.TRAP && freeTrapSlots > 0);
        
        hairsprayButton.gameObject.SetActive(Controller.Current.score > 50);
        
        buttonClickCooldown = Mathf.Max(0, buttonClickCooldown - Time.deltaTime);
        if (buttonClickCooldown > 0) return;
        if (isHoveringButton) return;
        
        if (tool == Tool.FINGER)
        {
            if (Input.GetMouseButtonDown(0)) finger.Tap(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        if (tool == Tool.TRAP)
        {
            if (Input.GetMouseButtonUp(0) && !View.Current.IsTracking && freeTrapSlots > 0)
            {
                var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var trapGameObject = Instantiate(trap, new Vector3(position.x, position.y, 0), Quaternion.identity);
                _trapsInUse.Add(trapGameObject.GetComponent<Trap>());
            }
        }
    }
}
