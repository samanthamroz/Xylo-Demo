using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class DraggableBlockHandle : InteractableObject {
    public Material regularMaterial, greyedMaterial;
    public DraggableBlock parentBlock;
    private Vector2 mousePosition { get { return ControlsManager.self.mousePosition; } }
    private Vector2 originalMousePosition;
    private Vector3 originalPosition;
    public Vector3 direction = Vector3.one;
    private bool isDragging;

    void Start() {
        //direction = (parentBlock.transform.rotation * direction).normalized;
        direction = GetRoundedVector(direction);

        isDragging = false;

    }
    private static Vector3 GetAbsVector(Vector3 vec) {
        return new Vector3(Math.Abs(vec.x), Math.Abs(vec.y), Math.Abs(vec.z));
    }
    private static Vector3 GetRoundedVector(Vector3 vec) {
        return new Vector3((float)Math.Round(vec.x), (float)(Math.Round(vec.y * 2) / 2), (float)Math.Round(vec.z));
    }
    private static Vector3 GetSnapToGridVector(Vector3 originalPosition, Vector3 targetVector) {
        float Yincrement = .25f;
        float XZincrement = .25f;

        float snappedX = originalPosition.x + Mathf.Round((targetVector.x - originalPosition.x) / XZincrement) * XZincrement;
        float snappedY = originalPosition.y + Mathf.Round((targetVector.y - originalPosition.y) / Yincrement) * Yincrement;
        float snappedZ = originalPosition.z + Mathf.Round((targetVector.z - originalPosition.z) / XZincrement) * XZincrement;

        return new Vector3(snappedX, snappedY, snappedZ);
    }

    public override void DoClick() {
        if (parentBlock.gameObject.TryGetComponent<ConnectedBlock>(out ConnectedBlock connected)) {
            foreach (GameObject block in connected.connectedBlocks) {
                if (block.TryGetComponent<ConnectedBlock>(out ConnectedBlock temp)) {
                    temp.TurnOffHandlesNotInDirection(direction);
                }
            }
        }
        parentBlock.TurnOffHandlesNotInDirection(direction);
        originalMousePosition = mousePosition;
        StartCoroutine(Drag());
    }
    public override void DoClickAway() {
        parentBlock.DoClickAway();
    }
    public override void DoRelease() {
        if (parentBlock.gameObject.TryGetComponent<ConnectedBlock>(out ConnectedBlock connected)) {
            parentBlock.originalPosition = GetSnapToGridVector(parentBlock.originalPosition, parentBlock.transform.position);
            foreach (GameObject block in connected.connectedBlocks) {
                if (block.TryGetComponent<ConnectedBlock>(out ConnectedBlock temp)) {
                    temp.originalPosition = GetSnapToGridVector(temp.originalPosition, temp.transform.position);
                    temp.ToggleAllHandles(true, false);
                }
            }
        }
        else {
            parentBlock.originalPosition = GetSnapToGridVector(parentBlock.originalPosition, parentBlock.transform.position);
        }

        parentBlock.ToggleAllHandles(true, false);
        isDragging = false;
    }

    public void ToggleGrey(bool isGrey) {
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        if (isGrey) {
            gameObject.GetComponent<MeshRenderer>().material = greyedMaterial;
        }
        else {
            gameObject.GetComponent<MeshRenderer>().material = regularMaterial;
        }
    }
    public void ToggleInvisible(bool isInvisible) {
        if (isInvisible) {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
        else {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    private IEnumerator Drag() {
        isDragging = true;

        while (isDragging) {
            float z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 originalMousePositionInWorld = Camera.main.ScreenToWorldPoint(new Vector3(originalMousePosition.x, originalMousePosition.y, z));
            Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, z));

            float mouseDelta;
            Vector3 newBlockPosition = parentBlock.originalPosition;

            if (GetAbsVector(direction).y == 1) {
                mouseDelta = mousePositionInWorld.y - originalMousePositionInWorld.y;
                newBlockPosition.y += mouseDelta;
            }
            else {
                mouseDelta = (mousePositionInWorld.x + mousePositionInWorld.z) - (originalMousePositionInWorld.x + originalMousePositionInWorld.z);
                if (GetAbsVector(direction).x == 1) {
                    newBlockPosition.x += mouseDelta;
                }
                if (GetAbsVector(direction).z == 1) {
                    newBlockPosition.z += mouseDelta;
                }
            }

            newBlockPosition = GetSnapToGridVector(parentBlock.originalPosition, newBlockPosition);

            bool isConnectedBlock = parentBlock.gameObject.TryGetComponent<ConnectedBlock>(out ConnectedBlock connected);
            if (IsNotJumpingBlocks(newBlockPosition)) {
                Vector3 mdirection = newBlockPosition - parentBlock.transform.position;
                float distance = mdirection.magnitude;

                // Use BoxCast to match your block's shape
                if (!Physics.BoxCast(parentBlock.transform.position, 
                                    parentBlock.GetComponent<Collider>().bounds.extents, 
                                    mdirection.normalized, 
                                    out RaycastHit hit,
                                    parentBlock.transform.rotation,
                                    distance))
                {
                    parentBlock.GetComponent<Rigidbody>().MovePosition(newBlockPosition);
                }

                if (!isConnectedBlock) {
                    //parentBlock.GetComponent<Rigidbody>().MovePosition(newBlockPosition);
                    //parentBlock.transform.position = newBlockPosition;
                    parentBlock.TurnOffHandlesNotInDirection(direction);
                }
                else {
                    Vector3 howMuchToMove = newBlockPosition - parentBlock.transform.position;

                    bool isAnyChildrenColliding = false;
                    foreach (GameObject block in connected.connectedBlocks) {
                        if (DraggableBlock.IsObjectCollidingAtPosition(block.GetComponent<Collider>(), block.transform.position + howMuchToMove)) {
                            isAnyChildrenColliding = true;
                        }
                    }
                    if (!isAnyChildrenColliding) {
                        parentBlock.GetComponent<Rigidbody>().MovePosition(newBlockPosition);
                        //parentBlock.transform.position = newBlockPosition;
                        foreach (GameObject block in connected.connectedBlocks) {
                            block.transform.position += howMuchToMove;
                        }
                        parentBlock.TurnOffHandlesNotInDirection(direction);
                    }
                }
            }

            yield return null;
        }
    }

    public bool IsParentBlockCollidingAtPosition(Vector3 targetPosition) {
        Collider[] colliders = Physics.OverlapBox(targetPosition, parentBlock.GetComponent<Collider>().bounds.extents, Quaternion.identity);

        bool isColliding = false;
        foreach (Collider c in colliders) {
            if (c.gameObject != parentBlock.gameObject && !c.isTrigger) {
                isColliding = true;
                break;
            }
        }
        return isColliding;

    }
    private bool IsNotJumpingBlocks(Vector3 targetPosition) {
        return
            Math.Abs(parentBlock.transform.position.x - targetPosition.x) <= 1 &&
            Math.Abs(parentBlock.transform.position.y - targetPosition.y) <= .5 &&
            Math.Abs(parentBlock.transform.position.z - targetPosition.z) <= 1;
    }
}