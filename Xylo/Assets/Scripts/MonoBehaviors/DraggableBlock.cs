using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DraggableBlock : MonoBehaviour
{
    public Note note;
    private Vector3 mousePosition;
    private Vector3 originalPos;
    private Vector3 direction = Vector3.one;
    void Start()
    {
        originalPos = GetRoundedVector(transform.localPosition);
    }
    private Vector3 GetRoundedVector(Vector3 vec) {
        return new Vector3((float)Math.Round(vec.x), (float)(Math.Round(vec.y * 2)/2), (float)Math.Round(vec.z));
    }
    private Vector3 GetMousePosition() {
        return Camera.main.WorldToScreenPoint(transform.position);
    }
    void OnMouseClick() {

    }
    private void OnMouseDown() {
        originalPos = GetRoundedVector(transform.localPosition);
        mousePosition = Input.mousePosition - GetMousePosition();
        GetComponent<AudioSource>().Play();
    }
    private void OnMouseDrag()
    {
        Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition - mousePosition);
        newPos = GetRoundedVector(newPos);

        if (direction == Vector3.zero) {
            direction = Vector3.one;
        }

        if (direction.x == 0) {
            newPos.x = originalPos.x;
        }
        if (direction.y == 0) {
            newPos.y = originalPos.y;
        }
        if (direction.z == 0) {
            newPos.z = originalPos.z;
        }

        if (direction == Vector3.one) {
            direction = newPos - originalPos;
        }
        if (!IsCollidingAtPosition(newPos) && IsValidMovement(newPos))
        {
            transform.localPosition = newPos;
        }
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
    void OnMouseUp()
    {
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
