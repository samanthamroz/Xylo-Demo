using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//source: https://www.youtube.com/watch?v=zo1dkYfIJVg

public class MouseManager : MonoBehaviour
{
    public static MouseManager self;
    public Vector3 mousePosition;
    public DraggableBlock currentInteractable;
    private bool isInteractable {
        get {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                try
                {
                    currentInteractable = hit.collider.gameObject.GetComponent<DraggableBlock>();
                    return true;
                } finally {}
            }
            return false;
        }
    }
    
    void Awake() {
		if (self == null) {
			self = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }
    
    void OnMouseMove(InputValue value) {
        mousePosition = value.Get<Vector2>();
    }
    
    void OnMouseClick(InputValue value) {
        if (value.Get<float>() == 1) { //click down
			if (isInteractable) {
                StartCoroutine(currentInteractable.Drag());
            }
		} else { //click released
            currentInteractable.isDragging = false;
            currentInteractable = null;
        }
    }
}