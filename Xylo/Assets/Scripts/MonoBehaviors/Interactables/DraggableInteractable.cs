using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class NameIdRegistry
{
    private static readonly Dictionary<string, int> nameToId = new();
    private static int nextId = 1;

    public static int GetId(string name)
    {
        if (!nameToId.TryGetValue(name, out int id))
        {
            id = nextId++;
            nameToId[name] = id;
        }

        return id;
    }
}

[RequireComponent(typeof(GridCollisionHandler))]
[RequireComponent(typeof(Rigidbody))]
public class DraggableInteractable : MonoBehaviour, IClickBehavior, IReleaseBehavior, IClickAwayBehavior {
    [HideInInspector] public int uniqueId = -1;
    [HideInInspector] public Vector3 originalPosition;
    private GridCollisionHandler collisionHandler;
    private Vector2 MousePosition { get { return ControlsManager.self.mousePosition; } }
    private Vector2 originalMousePosition;
    private bool _isDragging;

    void Awake() {
        if (uniqueId == -1) {
            // Generate a unique ID based on scene path
            uniqueId = NameIdRegistry.GetId(gameObject.name);
        }
    }

    void Start() {
        collisionHandler = GetComponent<GridCollisionHandler>();

        originalPosition = VectorUtils.GetSnapToGridVector(transform.position, transform.position);
    
        _isDragging = false;
    }
    
    public bool IsCollidingAtPosition(Vector3 testPosition) {
        return collisionHandler.IsCollidingAtPosition(transform.position, testPosition);
    }

    public void DoClick() {
        originalMousePosition = MousePosition;
        StartCoroutine(Drag());
    }
    public void DoClickAway() {
        
    }
    public void DoRelease() {
        originalPosition = VectorUtils.GetSnapToGridVector(originalPosition, transform.position);
        _isDragging = false;

        LoadingManager.self.SaveInteractablePosition(uniqueId, transform.position);
    }

    private IEnumerator Drag() {
        _isDragging = true;
        bool setDirectionYet = false;
        Vector3 directionMoving = Vector3.one;

        while (_isDragging) {
            float z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 originalMousePositionInWorld = Camera.main.ScreenToWorldPoint(new Vector3(originalMousePosition.x, originalMousePosition.y, z));
            Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(new Vector3(MousePosition.x, MousePosition.y, z));

            if (!setDirectionYet) {
                Vector3 mouseDelta = VectorUtils.GetAbsVector(originalMousePositionInWorld - mousePositionInWorld);
                float max = Mathf.Max(mouseDelta.x, mouseDelta.y, mouseDelta.z);
                directionMoving = new Vector3(mouseDelta.x == max ? mouseDelta.x : 0, mouseDelta.y == max ? mouseDelta.y : 0, mouseDelta.z == max ? mouseDelta.z : 0).normalized;
            }

            Vector3 newBlockPosition = originalPosition;

            float amountToMove;

            if (VectorUtils.GetAbsVector(directionMoving).y == 1) {
                amountToMove = mousePositionInWorld.y - originalMousePositionInWorld.y;
                newBlockPosition.y += amountToMove;
            }
            else {
                amountToMove = (mousePositionInWorld.x + mousePositionInWorld.z) - (originalMousePositionInWorld.x + originalMousePositionInWorld.z);
                if (VectorUtils.GetAbsVector(directionMoving).x == 1) {
                    newBlockPosition.x += amountToMove;
                }
                if (VectorUtils.GetAbsVector(directionMoving).z == 1) {
                    newBlockPosition.z += amountToMove;
                }
            }

            newBlockPosition = VectorUtils.GetSnapToGridVector(originalPosition, newBlockPosition);

            if (IsNotJumpingBlocks(newBlockPosition) && !IsCollidingAtPosition(newBlockPosition)) {
                GetComponent<Rigidbody>().MovePosition(newBlockPosition);
                if (newBlockPosition != originalPosition) {
                    setDirectionYet = true;
                }
            }

            yield return null;
        }
    }

    private bool IsNotJumpingBlocks(Vector3 targetPosition) {
        return
            Mathf.Abs(transform.position.x - targetPosition.x) <= .5 &&
            Mathf.Abs(transform.position.y - targetPosition.y) <= .25 &&
            Mathf.Abs(transform.position.z - targetPosition.z) <= .5;
    }
}