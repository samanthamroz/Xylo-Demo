using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class DraggableBlock : InteractableObject
{
    [SerializeField] private List<DraggableBlockHandle> handles = new();
    public bool isMultipleParts = false;
    [SerializeField] private bool startsAttempt = false;
    [SerializeField] private bool endsAttempt = false;
    [SerializeField] private Note note;
    private Vector3 mousePosition { get { return ControlsManager.self.mousePosition; } }
    [HideInInspector] public Vector3 originalPosition;
    private Vector3 direction = Vector3.one;
    void Start()
    {
        ToggleAllHandles(false, true);
        originalPosition = GetRoundedVector(transform.position);
    }
    public void ToggleAllHandles(bool isOn, bool turnInvisible) {
        foreach (DraggableBlockHandle handle in handles) {
            handle.gameObject.SetActive(isOn);
            Vector3 testPosition = transform.position + handle.direction;
            if (turnInvisible) {
                handle.ToggleInvisible(handle.IsBlockCollidingAtPosition(testPosition));
            } else {
                handle.ToggleGrey(handle.IsBlockCollidingAtPosition(testPosition));
            }
        }
    }

    private Vector3 GetAbsVector(Vector3 vec) {
        return new Vector3(Math.Abs(vec.x), Math.Abs(vec.y), Math.Abs(vec.z));
    }

    public void TurnOffHandlesNotInDirection(Vector3 direction) {
        ToggleAllHandles(true, true);
        ToggleAllHandles(true, false);
        foreach (DraggableBlockHandle handle in handles) {
            if (GetAbsVector(handle.direction) != GetAbsVector(direction)) {
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
        ToggleAllHandles(true, false);
    }

    public override void DoClickAway()
    {
        ToggleAllHandles(false, true);
    }

    //Marble Collision Behavior
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Marble")) {
            GetComponent<AudioSource>().Play();
            if (startsAttempt) {
                LevelManager.self.StartCountingForAttempt();
            }
            LevelManager.self.TriggerNote(note);
            if (endsAttempt) {
                LevelManager.self.EndAttempt();
            }
        }
    }
}