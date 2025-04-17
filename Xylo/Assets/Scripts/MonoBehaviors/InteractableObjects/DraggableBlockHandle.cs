using System;
using System.Collections;
using UnityEngine;

public class DraggableBlockHandle : InteractableObject
{
    public DraggableBlock parentBlock;
    private Transform parentPosition;
    private Vector2 mousePosition { get { return ControlsManager.self.mousePosition; } }
    private Vector2 originalMousePosition;
    private Vector3 originalPosition;
    public Vector3 direction = Vector3.one;
    private bool isDragging;

    void Start()
    {
        parentPosition = parentBlock.gameObject;
        
        direction = direction.normalized;
        isDragging = false;
        originalPosition = GetRoundedVector(transform.position);
    }
    private Vector3 GetRoundedVector(Vector3 vec) {
        return new Vector3((float)Math.Round(vec.x), (float)(Math.Round(vec.y * 2)/2), (float)Math.Round(vec.z));
    }
    public override void DoClick() {
        parentBlock.TurnOffHandlesNotInDirection(direction);
        originalMousePosition = mousePosition;
        StartCoroutine(Drag());
    }
    public override void DoRelease()
    {
        originalPosition = GetRoundedVector(transform.position);
        parentBlock.originalPosition = GetRoundedVector(parentBlock.transform.position);
        parentBlock.ToggleAllHandles(true);
        isDragging = false;
    }
    private IEnumerator Drag() {
        isDragging = true;
        Vector3 newWorldPosition;
        while(isDragging) {
            /*float z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 unroundedNewWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, z));
            newWorldPosition = GetRoundedVector(unroundedNewWorldPos);

            //don't let it move except in correct direction
            if (direction.x == 0) {
                newWorldPosition.x = originalPosition.x;
            }
            if (direction.y == 0) {
                newWorldPosition.y = originalPosition.y;
            }
            if (direction.z == 0) {
                newWorldPosition.z = originalPosition.z;
            }
                
            Vector3 newBlockPosition = parentBlock.originalPosition; //should overwrite below
            float distanceFromOriginalPosition;
            if (direction.x == 1) {
                distanceFromOriginalPosition = newWorldPosition.x - originalPosition.x;
                newBlockPosition = new(parentBlock.originalPosition.x + distanceFromOriginalPosition, parentBlock.originalPosition.y, parentBlock.originalPosition.z);
            }
            if (direction.y == 1) {
                distanceFromOriginalPosition = newWorldPosition.y - originalPosition.y;
                newBlockPosition = new(parentBlock.originalPosition.x, parentBlock.originalPosition.y + distanceFromOriginalPosition, parentBlock.originalPosition.z);
            }
            if (direction.z == 1) {
                distanceFromOriginalPosition = newWorldPosition.z - originalPosition.z;
                newBlockPosition = new(parentBlock.originalPosition.x, parentBlock.originalPosition.y, parentBlock.originalPosition.z + distanceFromOriginalPosition);
            }*/

            float z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 originalMousePositionInWorld = Camera.main.ScreenToWorldPoint(new Vector3(originalMousePosition.x, originalMousePosition.y, z));
            Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, z));
            
            float mouseDelta;
            Vector3 newBlockPosition = parentBlock.originalPosition;

            if (direction.y == 1) {
                mouseDelta = mousePositionInWorld.y - originalMousePositionInWorld.y;
                newBlockPosition.y += mouseDelta;
            } else {
                mouseDelta = (mousePositionInWorld.x + mousePositionInWorld.z) - (originalMousePositionInWorld.x + originalMousePositionInWorld.z);
                if (direction.x == 1) {
                    newBlockPosition.x += mouseDelta;
                }
                if (direction.z == 1) {
                    newBlockPosition.z += mouseDelta;
                }
            }
            newBlockPosition = GetRoundedVector(newBlockPosition);
            print(newBlockPosition);

            if (!IsBlockCollidingAtPosition(newBlockPosition) && IsNotJumpingBlocks(newBlockPosition))
            {
                if (!parentBlock.isMultipleParts) {
                    parentBlock.transform.position = newBlockPosition;
                } else {
                    //STUFF FOR MULTIPLE PARTS
                    /*Vector3 offset = newWorldPosition - transform.position;
                    bool isAnyChildrenColliding = false;
                    foreach (Transform child in transform.parent) {
                        if (child.gameObject.GetComponent<DraggableBlock>().IsCollidingAtPosition(child.position + offset)) {
                            isAnyChildrenColliding = true;
                        }
                    }

                    if (!isAnyChildrenColliding)
                    {
                        foreach (Transform child in transform.parent) {
                            child.position += offset;
                        }
                        transform.position = newWorldPosition;
                    }*/
                }
            }

            yield return null;
        }
    }

    private bool IsBlockCollidingAtPosition(Vector3 targetPosition)
    {
        Collider[] colliders = Physics.OverlapBox(targetPosition, parentBlock.GetComponent<Collider>().bounds.extents, Quaternion.identity);

        bool isColliding = false;
        foreach (Collider c in colliders) {
            if (c.gameObject != parentBlock.gameObject /*&& !c.transform.IsChildOf(this.transform.parent)*/ && !c.isTrigger)
            {
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