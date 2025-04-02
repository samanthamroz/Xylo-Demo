using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DraggableBlock : MonoBehaviour
{
    public Note note;
    private Vector3 mousePosition { get { return MouseManager.self.mousePosition; } }
    private Vector3 originalPos;
    private Vector3 direction = Vector3.one;
    public bool isDragging;
    void Start()
    {
        isDragging = false;
        originalPos = GetRoundedVector(transform.localPosition);
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
        Vector3 originalPosition = GetRoundedVector(transform.localPosition);
        Vector3 newWorldPosition;
        while(isDragging) {
            float z = Camera.main.WorldToScreenPoint(transform.position).z;
            newWorldPosition = GetRoundedVector(Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, z)));
            print(newWorldPosition);
            
            //if we haven't moved yet, let it move in any direction
            if (direction == Vector3.zero) {
                direction = Vector3.one;
                direction = newWorldPosition - originalPos; //locks direction
            } else {
                //we must be currently moving
                if (direction.x == 0) {
                    newWorldPosition.x = originalPos.x;
                }
                if (direction.y == 0) {
                    newWorldPosition.y = originalPos.y;
                }
                if (direction.z == 0) {
                    newWorldPosition.z = originalPos.z;
                }
            }

            //check for collisions, etc, then move
            if (!IsCollidingAtPosition(newWorldPosition) && IsValidMovement(newWorldPosition))
            {
                transform.localPosition = newWorldPosition;
            }

            yield return null;
        }
        //drop
        direction = Vector3.one;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Marble")) {
            GetComponent<AudioSource>().Play();
            //WinManager.self.TriggerNote(note);
        }
    }
}
