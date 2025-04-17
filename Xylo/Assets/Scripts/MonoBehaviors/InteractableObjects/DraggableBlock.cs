using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableBlock : InteractableObject
{
    [HideInInspector] public Vector3 parentPosition { 
        get { return transform.parent.position; }
        set { transform.parent.position }
    }
    [SerializeField] private List<DraggableBlockHandle> handles = new();
    [HideInInspector] public GameObject graphics;
    public bool isMultipleParts = false;
    [SerializeField] private bool startsAttempt = false;
    [SerializeField] private bool endsAttempt = false;
    [SerializeField] private Note note;
    private Vector3 mousePosition { get { return ControlsManager.self.mousePosition; } }
    [HideInInspector] public Vector3 originalPosition;
    private Vector3 direction = Vector3.one;
    private bool isDragging;
    void Start()
    {
        graphics = transform.GetChild(0).gameObject;
        isDragging = false;
        ToggleAllHandles(false);
        originalPosition = GetRoundedVector(transform.position);
    }
    public void ToggleAllHandles(bool isOn) {
        foreach (DraggableBlockHandle handle in handles) {
            handle.gameObject.SetActive(isOn);
        }
    }
    public void TurnOffHandlesNotInDirection(Vector3 direction) {
        foreach (DraggableBlockHandle handle in handles) {
            if (handle.direction != direction) {
                handle.gameObject.SetActive(false);
            }
        }
    }
    private Vector3 GetRoundedVector(Vector3 vec) {
        return new Vector3((float)Math.Round(vec.x), (float)(Math.Round(vec.y * 2)/2), (float)Math.Round(vec.z));
    }
    
    //Click & Drag Behavior
    public override void DoClick() {
        GetComponent<AudioSource>().Play();
        ToggleAllHandles(true);
    }

    public override void DoClickAway()
    {
        base.DoClickAway();
    }

    //Marble Collision Behavior
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Marble")) {
            GetComponent<AudioSource>().Play();
            if (startsAttempt) {
                WinManager.self.StartAttempt();
            }
            WinManager.self.TriggerNote(note);
            if (endsAttempt) {
                WinManager.self.EndAttempt();
            }
        }
    }
}