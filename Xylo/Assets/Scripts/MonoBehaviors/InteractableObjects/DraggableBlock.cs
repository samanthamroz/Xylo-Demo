using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DraggableBlock : InteractableObject {
    [SerializeField] protected List<DraggableBlockHandle> handles = new();
    [SerializeField] private Note note;
    [HideInInspector] public Vector3 originalPosition;

    void Start() {
        ToggleAllHandles(false, true);
        originalPosition = VectorUtils.GetSnapToGridVector(transform.position, transform.position);
    }
    public virtual void ToggleAllHandles(bool isOn, bool turnInvisible) {
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

    public void TurnOffHandlesNotInDirection(Vector3 direction) {
        ToggleAllHandles(true, true);
        ToggleAllHandles(true, false);
        foreach (DraggableBlockHandle handle in handles) {
            if (VectorUtils.GetAbsVector(handle.direction) != VectorUtils.GetAbsVector(direction)) {
                handle.gameObject.SetActive(false);
            }
        }
    }

    protected bool IsSelfCollidingAtPosition(Vector3 targetPosition) {
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
        if (objCollider == null) return false;

        Collider[] colliders = Physics.OverlapBox(targetPosition, objCollider.bounds.extents, Quaternion.identity);

        bool isColliding = false;
        foreach (Collider c in colliders) {
            bool layerIsExcluded = ((1 << c.gameObject.layer) & objCollider.excludeLayers) != 0;

            if (c.gameObject != objCollider.gameObject && !c.isTrigger && !layerIsExcluded) {
                //Debug.Log($"{objCollider.gameObject.name} Collision detected with: {c.gameObject.name} at position {targetPosition}");
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