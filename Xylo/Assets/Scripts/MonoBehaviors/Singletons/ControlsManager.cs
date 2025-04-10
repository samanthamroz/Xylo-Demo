using System;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//inspo: https://www.youtube.com/watch?v=zo1dkYfIJVg

public class ControlsManager : MonoBehaviour
{
    public static ControlsManager self;
    
    [SerializeField] private InputActionAsset inputActions;
    private InputActionMap mainMap;
    private InputActionMap menuMap;

    
    [HideInInspector] public Vector3 mousePosition;
    private DraggableBlock currentInteractable;
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
    
    void Start() {
        mainMap = inputActions.FindActionMap("Main");
        menuMap = inputActions.FindActionMap("Menus");
    }
    
    public void ToggleMenuMode() {
        if (mainMap.enabled) {
            mainMap.Disable();
            menuMap.Enable();
        } else {
            menuMap.Disable();
            mainMap.Enable();
        }
    }

    void OnMouseMove(InputValue value) {
        mousePosition = value.Get<Vector2>();
    }
    
    void OnMouseClick(InputValue value) {
        if (value.Get<float>() == 1) { //click down
            CameraManager.self.isRotating = false;
            CameraManager.self.isPanning = false;
			if (isInteractable) {
                currentInteractable.DoClick();
            }
		} else { //click released
            try {
                currentInteractable.isDragging = false;
            } catch {} finally {
                currentInteractable = null;
            }
        }
    }

    void OnMiddleMouseClick(InputValue value) {
        if (value.Get<float>() == 1) { //click down
			CameraManager.self.DoRotate();
		} else {
            CameraManager.self.isRotating = false;
        }
    }

    void OnRightMouseClick(InputValue value) {
        if (value.Get<float>() == 1) { //click down
			CameraManager.self.DoPan();
		} else {
            CameraManager.self.isPanning = false;
        }
    }

    void OnScroll(InputValue value) {
        float scrollInput = value.Get<float>();
        if (scrollInput != 0) {
            CameraManager.self.DoScroll(Math.Sign(scrollInput));
        }
    }

    void OnPiano(InputValue value) {
        if (value.Get<float>() == 1) {
			GUIManager.self.TogglePiano();
            ToggleMenuMode();
		}
    }

	void OnPause(InputValue value) {
		if (value.Get<float>() == 1) {
			GUIManager.self.TogglePause();
            ToggleMenuMode();
		}
	}
}