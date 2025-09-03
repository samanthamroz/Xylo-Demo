using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DraggableBlock : InteractableObject {
    [SerializeField] private List<DraggableBlockHandle> handles = new();
    [SerializeField] private bool startsAttempt = false;
    [SerializeField] private Note note;
    [HideInInspector] public Vector3 originalPosition;

    void Start() {
        ToggleAllHandles(false, true);
        originalPosition = GetSnapToGridVector(transform.position, transform.position);
        print("start");
    }
    public void ToggleAllHandles(bool isOn, bool turnInvisible) {
        foreach (DraggableBlockHandle handle in handles) {
            handle.gameObject.SetActive(isOn);
            Vector3 testPosition = transform.position + new Vector3(handle.direction.x, handle.direction.y / 2, 0);
            if (turnInvisible) {
                handle.ToggleInvisible(IsSelfCollidingAtPosition(testPosition));
            }
            else {
                handle.ToggleGrey(IsSelfCollidingAtPosition(testPosition));
            }
        }
    }

    private static Vector3 GetAbsVector(Vector3 vec) {
        return new Vector3(Math.Abs(vec.x), Math.Abs(vec.y), Math.Abs(vec.z));
    }
    private static Vector3 GetSnapToGridVector(Vector3 originalPosition, Vector3 targetVector) {
        float Yincrement = 0.5f;
        float XZincrement = 1f;

        float snappedX = originalPosition.x + Mathf.Round((targetVector.x - originalPosition.x) / XZincrement) * XZincrement;
        float snappedY = originalPosition.y + Mathf.Round((targetVector.y - originalPosition.y) / Yincrement) * Yincrement;
        float snappedZ = originalPosition.z + Mathf.Round((targetVector.z - originalPosition.z) / XZincrement) * XZincrement;

        return new Vector3(snappedX, snappedY, snappedZ);
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

    private bool IsSelfCollidingAtPosition(Vector3 targetPosition) {
        Collider[] colliders = Physics.OverlapBox(targetPosition, GetComponent<Collider>().bounds.extents, Quaternion.identity);

        bool isColliding = false;
        foreach (Collider c in colliders) {
            if (c.gameObject != gameObject && !c.isTrigger) {
                //Debug.Log($"Collision detected with: {c.gameObject.name} at position {targetPosition}");
                isColliding = true;
                break;
            }
        }
        //Debug.Log($"Position {targetPosition} - Colliding: {isColliding}");
        return isColliding;
    }

    public static bool IsObjectCollidingAtPosition(Collider objCollider, Vector3 targetPosition) {
        Collider[] colliders = Physics.OverlapBox(targetPosition, objCollider.bounds.extents, Quaternion.identity);

        bool isColliding = false;
        foreach (Collider c in colliders) {
            if (c.gameObject != objCollider.gameObject && !c.isTrigger) {
                //Debug.Log($"Collision detected with: {c.gameObject.name} at position {targetPosition}");
                isColliding = true;
                break;
            }
        }
        //Debug.Log($"Position {targetPosition} - Colliding: {isColliding}");
        return isColliding;
    }

    public override void DoClick() {
        GetComponent<AudioSource>().Play();
        ToggleAllHandles(true, false);
    }
    public override void DoClickAway() {
        ToggleAllHandles(false, true);
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Marble")) {
            GetComponent<AudioSource>().PlayScheduled(AudioSettings.dspTime);
            LevelManager.self.TriggerNote(note);
        }
    }
}