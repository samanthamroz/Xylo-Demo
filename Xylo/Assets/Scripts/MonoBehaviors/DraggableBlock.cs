using System;
using UnityEngine;

public class DraggableBlock : MonoBehaviour
{
    private Vector3 mousePosition;
    private Vector3 originalPos;
    private Vector3 direction = Vector3.one;

    void Start()
    {
        originalPos = GetRoundedVector(transform.position);
    }

    private Vector3 GetRoundedVector(Vector3 vec) {
        return new Vector3((float)Math.Round(vec.x), (float)Math.Round(vec.y), (float)Math.Round(vec.z));
    }
    private Vector3 GetMousePosition() {
        return Camera.main.WorldToScreenPoint(transform.position);
    }

    private void OnMouseDown() {
        originalPos = GetRoundedVector(transform.position);
        mousePosition = Input.mousePosition - GetMousePosition();
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

        transform.position = newPos;
    }
    
    void OnMouseUp()
    {
        direction = Vector3.one;
    }
}
