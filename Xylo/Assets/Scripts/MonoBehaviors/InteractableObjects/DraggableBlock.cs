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
        originalPosition = GetSnapToGridVector(transform.position, transform.position);
    }
    public void ToggleAllHandles(bool isOn, bool turnInvisible) {
        foreach (DraggableBlockHandle handle in handles) {
            handle.gameObject.SetActive(isOn);
            Vector3 testPosition = transform.position + new Vector3(handle.direction.x, handle.direction.y / 2, handle.direction.z);
            if (turnInvisible) {
                handle.ToggleInvisible(handle.IsParentBlockCollidingAtPosition(testPosition));
            } else {
                handle.ToggleGrey(handle.IsParentBlockCollidingAtPosition(testPosition));
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

    private Vector3 GetSnapToGridVector(Vector3 originalPosition, Vector3 targetVector) {
        float Yincrement = 0.5f;
        float XZincrement = 1f;

        float snappedX = originalPosition.x + Mathf.Round((targetVector.x - originalPosition.x) / XZincrement) * XZincrement;
        float snappedY = originalPosition.y + Mathf.Round((targetVector.y - originalPosition.y) / Yincrement) * Yincrement;
        float snappedZ = originalPosition.z + Mathf.Round((targetVector.z - originalPosition.z) / XZincrement) * XZincrement;

        return new Vector3(snappedX, snappedY, snappedZ);
    }
    
    public bool IsBlockCollidingAtPosition(Vector3 targetPosition)
    {
        Collider[] colliders = Physics.OverlapBox(targetPosition, GetComponent<Collider>().bounds.extents, Quaternion.identity);

        bool isColliding = false;
        foreach (Collider c in colliders) {
            if (c.gameObject != gameObject && !c.transform.IsChildOf(transform.parent) && !c.isTrigger)
            {
                isColliding = true;
                break;
            }
        }
        return isColliding;
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
            StartCoroutine(TriggerNote());
        }
    }

    private IEnumerator TriggerNote() {
        GetComponent<AudioSource>().Play();
        if (startsAttempt) {
            LevelManager.self.StartCountingForAttempt();
        }
        yield return null;
        LevelManager.self.TriggerNote(note);
        if (endsAttempt) {
            
        }
    }
}