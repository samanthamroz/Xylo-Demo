using System;
using System.Collections;
using UnityEngine;

public class DraggableBlock : MonoBehaviour
{
    public Note note;
    private Vector3 mousePosition { get { return MouseManager.self.mousePosition; } }
    private Vector3 originalPosition;
    private Vector3 direction = Vector3.one;
    public bool isDragging;
    void Start()
    {
        isDragging = false;
        originalPosition = GetRoundedVector(transform.localPosition);
    }
    private Vector3 GetRoundedVector(Vector3 vec) {
        return new Vector3((float)Math.Round(vec.x), (float)(Math.Round(vec.y * 2)/2), (float)Math.Round(vec.z));
    }
    public void OnMouseDown() {
        GetComponent<AudioSource>().Play();
    }
    private bool IsCollidingAtPosition(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapBox(position, GetComponent<Collider>().bounds.extents, Quaternion.identity);
        if (colliders.Length == 0) {
            return false;
        }

        int count = colliders.Length;
        foreach (Collider c in colliders) {
            if (c.gameObject == this.gameObject) {
                count -= 1;
            }
        }
        return count > 0;
    }
    private bool IsValidMovement(Vector3 position) {
        return 
            Math.Abs(transform.position.x - position.x) <= 1 &&
            Math.Abs(transform.position.y - position.y) <= .5 &&
            Math.Abs(transform.position.z - position.z) <= 1;
    }

    public IEnumerator Drag() {
        isDragging = true;
        Vector3 newWorldPosition;
        while(isDragging) {
            float z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 unroundedNewWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, z));
            newWorldPosition = GetRoundedVector(unroundedNewWorldPos);

            if (direction != Vector3.one) {
                //we must be currently moving
                if (direction.x == 0) {
                    newWorldPosition.x = originalPosition.x;
                }
                if (direction.y == 0) {
                    newWorldPosition.y = originalPosition.y;
                }
                if (direction.z == 0) {
                    newWorldPosition.z = originalPosition.z;
                }

                if (!IsCollidingAtPosition(newWorldPosition) && IsValidMovement(newWorldPosition))
                {
                    transform.localPosition = newWorldPosition;
                }
            } else { //we need to figure out our direction
                if (newWorldPosition - originalPosition != Vector3.zero) { //check that mouse has moved enough from origin of click
                    Vector3 positionIfX = new(newWorldPosition.x, originalPosition.y, originalPosition.z);
                    Vector3 positionIfY = new(originalPosition.x, newWorldPosition.y, originalPosition.z);
                    Vector3 positionIfZ = new(originalPosition.x, originalPosition.y, newWorldPosition.z);

                    float distanceToMouseIfX = Math.Abs((Camera.main.WorldToScreenPoint(positionIfX) - mousePosition).magnitude);
                    float distanceToMouseIfY = Math.Abs((Camera.main.WorldToScreenPoint(positionIfY) - mousePosition).magnitude);
                    float distanceToMouseIfZ = Math.Abs((Camera.main.WorldToScreenPoint(positionIfZ) - mousePosition).magnitude);

                    if (distanceToMouseIfX < distanceToMouseIfY && distanceToMouseIfX < distanceToMouseIfZ) {
                        direction = Vector3.right;
                    } else if (distanceToMouseIfY < distanceToMouseIfZ) {
                        direction = Vector3.up;
                    } else {
                        direction = Vector3.forward;
                    }
                }
            }
            yield return null;
        }
        //drop
        direction = Vector3.one;
        originalPosition = GetRoundedVector(transform.localPosition);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Marble")) {
            GetComponent<AudioSource>().Play();
            //WinManager.self.TriggerNote(note);
        }
    }
}