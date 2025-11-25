using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridCollisionHandler))]
public class DraggableInteractable : MonoBehaviour, IClickBehavior, IClickAwayBehavior {
    [SerializeField] GameObject handlePrefab;
    private List<DraggableHandleInteractable> handles = new();
    [HideInInspector] public Vector3 originalPosition;
    private GridCollisionHandler collisionHandler;


    void Start() {
        collisionHandler = GetComponent<GridCollisionHandler>();

        CreateHandle(Vector3.left, transform);
        CreateHandle(Vector3.right, transform);
        CreateHandle(Vector3.up, transform);
        CreateHandle(Vector3.down, transform);

        TurnAllHandlesOff();
        originalPosition = VectorUtils.GetSnapToGridVector(transform.position, transform.position);
    }
    private void CreateHandle(Vector3 baseDirection, Transform parent) {
        DraggableHandleInteractable handle = Instantiate(handlePrefab, parent).GetComponent<DraggableHandleInteractable>();
        handle.transform.localScale = new(handle.transform.localScale.x / parent.localScale.x, handle.transform.localScale.y / parent.localScale.y, handle.transform.localScale.z / parent.localScale.z);

        handle.Initialize(this, this.transform.localRotation * baseDirection);
        print(gameObject.name + " - " + this.transform.localRotation * baseDirection);
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

    public bool IsCollidingAtPosition(Vector3 testPosition) {
        return collisionHandler.IsCollidingAtPosition(transform.position, testPosition);
    }

    public void DoClick() {
        TurnAllHandlesOn();
    }
    public void DoClickAway() {
        TurnAllHandlesOff();
    }
}