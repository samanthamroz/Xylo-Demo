using System;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//inspo: https://www.youtube.com/watch?v=zo1dkYfIJVg

public class ControlsManager : MonoBehaviour
{
    public static ControlsManager self;
    
    private InputActionAsset inputActions;
    private InputActionMap mainMap;
    private InputActionMap menuMap;
    private InputActionMap cinematicMap;
    private InputActionMap currentActionMap;
    public string currentActionMapName { get { return currentActionMap.name; } }

    
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
            inputActions = GetComponent<PlayerInput>().actions;
            mainMap = inputActions.FindActionMap("Main");
            menuMap = inputActions.FindActionMap("Menus");
            cinematicMap = inputActions.FindActionMap("Cinematic");
            SceneManager.sceneLoaded += InitializeActionMap;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }
    
    void Start() {

    }
    
    private void InitializeActionMap(Scene scene, LoadSceneMode mode) {
        if (scene.buildIndex <= 1) {
			ChangeActionMap("menu");
		} else {
            ChangeActionMap("main");
        }
    }

    private void ChangeActionMap(string mapName) {
        mainMap.Disable();
        menuMap.Disable();
        cinematicMap.Disable();
        switch (mapName.ToLower()) {
            case "main":
                mainMap.Enable();
                currentActionMap = mainMap;
                break;
            case "menu":
                menuMap.Enable();
                currentActionMap = menuMap;
                break;
            case "cinematic":
                menuMap.Enable();
                currentActionMap = cinematicMap;
                break;
            default:
                break;
        }
    }

    public void EnterCinematicMode() {
        ChangeActionMap("cinematic");
    }

    public void ExitCinematicMode() {
        ChangeActionMap("main");
        CameraManager.self.ExitCinematicMode();
    }

    private void ToggleMenuActionMap() {
        if (mainMap.enabled) {
            ChangeActionMap("menu");
        } else {
            ChangeActionMap("main");
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
            ToggleMenuActionMap();
		}
    }

	void OnPause(InputValue value) {
		if (value.Get<float>() == 1) {
			GUIManager.self.TogglePause();
            ToggleMenuActionMap();
		}
	}

    void OnDebug1(InputValue value) {
        if (value.Get<float>() == 1) {
			EnterCinematicMode();
		}
    }
}