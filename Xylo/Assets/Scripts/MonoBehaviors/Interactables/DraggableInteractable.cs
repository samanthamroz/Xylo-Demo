using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DraggableInteractable : MonoBehaviour, IClickBehavior, IClickAwayBehavior {
    [SerializeField] GameObject handlePrefab;
    private List<DraggableHandleInteractable> handles = new();
    [HideInInspector] public Vector3 originalPosition;


    void Start() {
        Transform scaleAdjust = new GameObject("ScaleAdjust").transform;
        scaleAdjust.SetParent(transform);
        scaleAdjust.SetLocalPositionAndRotation(transform.rotation * new Vector3(0f, 0f, -0.4f), Quaternion.identity);
        scaleAdjust.localScale = new(1f / transform.localScale.x, 1f / transform.localScale.y, 1f / transform.localScale.z);

        CreateHandle(Vector3.left, scaleAdjust);
        CreateHandle(Vector3.right, scaleAdjust);
        CreateHandle(Vector3.up, scaleAdjust);
        CreateHandle(Vector3.down, scaleAdjust);

        TurnAllHandlesOff();
        originalPosition = VectorUtils.GetSnapToGridVector(transform.position, transform.position);
    }
    private void CreateHandle(Vector3 baseDirection, Transform parent) {
        DraggableHandleInteractable handle = Instantiate(handlePrefab, parent).GetComponent<DraggableHandleInteractable>();
        handle.Initialize(this, transform.rotation * baseDirection);
        handles.Add(handle);
    }
    
    private bool ShouldHandleBeGrey(DraggableHandleInteractable handle) {
        Vector3 testPosition = transform.position - new Vector3(handle.Direction.x / 4, handle.Direction.y / 4, handle.Direction.z / 4);
        return IsCollidingAtPosition(testPosition);
    }
    private void TurnAllHandlesOn() {
        foreach (DraggableHandleInteractable handle in handles) {
            handle.SetHandleGraphics(true, ShouldHandleBeGrey(handle));
        }
    }
    private void TurnAllHandlesOff() {
        foreach (DraggableHandleInteractable handle in handles) {
            handle.SetHandleGraphics(false);
        }
    }
    public void TurnHandlesOffExceptInDirection(Vector3 direction) {
        TurnAllHandlesOff();
        foreach (DraggableHandleInteractable handle in handles) {
            if (VectorUtils.GetAbsVector(handle.Direction) == VectorUtils.GetAbsVector(direction)) {
                handle.SetHandleGraphics(true, ShouldHandleBeGrey(handle));
            }
        }
    }


    public bool IsCollidingAtPosition(Vector3 targetPosition) {
        Collider[] colliders = Physics.OverlapBox(targetPosition, GetComponent<Collider>().bounds.extents, Quaternion.identity);

        bool isColliding = false;
        foreach (Collider c in colliders) {
            if (c.gameObject != gameObject && !c.isTrigger && !c.transform.IsChildOf(transform)) {
                //Debug.Log($"Collision detected with: {c.gameObject.name} at position {targetPosition}");
                isColliding = true;
                break;
            }
        }
        //Debug.Log($"Position {targetPosition} - Colliding: {isColliding}");
        return isColliding;
        /*

        Vector3 moveVector = targetPosition - transform.position;
        float distance = moveVector.magnitude;

        return Physics.BoxCast(transform.position, 
                            GetComponent<Collider>().bounds.extents, 
                            moveVector.normalized, 
                            out RaycastHit hit,
                            transform.rotation,
                            distance); */
    }


    public void DoClick() {
        TurnAllHandlesOn();
    }
    public void DoClickAway() {
        TurnAllHandlesOff();
    }
}