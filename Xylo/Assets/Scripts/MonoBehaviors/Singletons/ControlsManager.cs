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
    private InputActionMap mainMap, menuMap, cinematicMap, currentActionMap;
    public string currentActionMapName { get { return currentActionMap.name; } }
    [HideInInspector] public Vector3 mousePosition;
    [SerializeField] private InteractableObject currentInteractable, lastInteractable;
    
    void Awake() {
		if (self == null) {
			self = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }
    
    void Start() {

    }
    
    public void InitializeActionMap(bool isLevelSelect) {
        inputActions = GetComponent<PlayerInput>().actions;
        mainMap = inputActions.FindActionMap("Main");
        menuMap = inputActions.FindActionMap("Menus");
        cinematicMap = inputActions.FindActionMap("Cinematic");

        if (isLevelSelect) {
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
                cinematicMap.Enable();
                currentActionMap = cinematicMap;
                break;
            default:
                break;
        }
    }

    public void EnterCinematicMode() {
        ChangeActionMap("cinematic");
    }

    public void ExitCinematicMode(bool isDeathPlane = false) {
        ChangeActionMap("main");
        CameraManager.self.ExitCinematicMode(isDeathPlane);
    }

    public void ToggleMenuActionMap() {
        if (mainMap.enabled) {
            ChangeActionMap("menu");
        } else {
            ChangeActionMap("main");
        }
    }

    void OnMouseMove(InputValue value) {
        mousePosition = value.Get<Vector2>();
    }
    
    private InteractableObject FindInteractableObjectAtMouse() {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            InteractableObject testIfInteractable = hit.collider.gameObject.GetComponent<InteractableObject>();
            if (testIfInteractable != null) {
                return testIfInteractable;
            } 
        }
        return null;
    }
    
    void OnMouseClick(InputValue value) {
        if (value.Get<float>() == 1) { //click down
            CameraManager.self.isRotating = false;
            CameraManager.self.isPanning = false;
            
            
            currentInteractable = FindInteractableObjectAtMouse();

            if (lastInteractable != null) {
                lastInteractable.DoClickAway();
            }

			if (currentInteractable != null) {
                currentInteractable.DoClick();
            } else { //clicked on empty space
                currentInteractable = null;
            }
		} else { //click released
            if (currentInteractable != null) {
                currentInteractable.DoRelease();
            } 
            lastInteractable = currentInteractable;
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
		}
    }

	void OnPause(InputValue value) {
		if (value.Get<float>() == 1) {
			GUIManager.self.TogglePause();
            ToggleMenuActionMap();
		}
	}

    void OnSpace(InputValue value) {
        if (value.Get<float>() == 1) {
			AudioManager.self.PlayMelody();
		}
    }

    void OnRestart(InputValue value){
        if (value.Get<float>() == 1) {
			LevelManager.self.RetryLevel();
		}
    }

    void OnDebug1(InputValue value) {
        if (value.Get<float>() == 1) {
			EnterCinematicMode();
		}
    }
}