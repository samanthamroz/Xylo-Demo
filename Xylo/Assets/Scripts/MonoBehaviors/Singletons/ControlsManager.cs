using System;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//inspo: https://www.youtube.com/watch?v=zo1dkYfIJVg

public class ControlsManager : MonoBehaviour {
    public static ControlsManager self;

    private InputActionAsset inputActions;
    private InputActionMap mainMap, levelSelectMap, menuMap, cinematicMap, currentActionMap, lastActionMap;
    public string currentActionMapName { get { return currentActionMap.name; } }
    [HideInInspector] public Vector3 mousePosition;
    [SerializeField] private InteractableObject currentInteractable, lastInteractable;

    public bool isGamePaused;


    void Awake() {
        if (self == null) {
            self = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    void Start() {

    }

    public void InitializeActionMap(string map) {
        inputActions = GetComponent<PlayerInput>().actions;
        mainMap = inputActions.FindActionMap("Main");
        levelSelectMap = inputActions.FindActionMap("LevelSelect");
        menuMap = inputActions.FindActionMap("LevelMenus");
        cinematicMap = inputActions.FindActionMap("Cinematic");

        ChangeActionMap(map);
    }

    private void ChangeActionMap(string mapName) {
        if (currentActionMap != null) {
            lastActionMap = currentActionMap;
        }

        mainMap.Disable();
        levelSelectMap.Disable();
        menuMap.Disable();
        cinematicMap.Disable();
        switch (mapName.ToLower()) {
            case "main":
                mainMap.Enable();
                currentActionMap = mainMap;
                break;
            case "levelselect":
                levelSelectMap.Enable();
                currentActionMap = levelSelectMap;
                break;
            case "levelmenus":
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

        if (lastActionMap == null) {
            lastActionMap = currentActionMap;
        }
    }

    public void ActivateCinematicMap() {
        ChangeActionMap("cinematic");
    }

    public void ActivateMenuMap() {
        ChangeActionMap("levelmenus");
    }

    public void ActivateMainMap() {
        ChangeActionMap("main");
    }

    public void RevertToLastMap() {
        ChangeActionMap(lastActionMap.name);
    }

    public void PauseGameTime(bool doPause) {
        if (doPause) {
            Time.timeScale = 0f;
        }
        else {
            Time.timeScale = 1f;
        }
        isGamePaused = doPause;
    }

    void OnMouseMove(InputValue value) {
        mousePosition = value.Get<Vector2>();
    }

    private InteractableObject FindInteractableObjectAtMouse() {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
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
            }
            else { //clicked on empty space
                currentInteractable = null;
            }
        }
        else { //click released
            if (currentInteractable != null) {
                currentInteractable.DoRelease();
            }
            lastInteractable = currentInteractable;
        }
    }

    void OnRightMouseClick(InputValue value) {
        if (value.Get<float>() == 1) { //click down
            CameraManager.self.DoPan();
        }
        else {
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
            GUIManager.self.TogglePianoPosition();
        }
    }

    void OnPause(InputValue value) {
        if (value.Get<float>() == 1) {
            GUIManager.self.TogglePause();
        }
    }

    void OnSpace(InputValue value) {
        if (value.Get<float>() == 1) {
            AudioManager.self.PlayMelodyForCurrentSection();
        }
    }

    void OnRestart(InputValue value) {
        if (value.Get<float>() == 1) {
            LoadingManager.self.ReloadCurrentScene();
        }
    }

    void OnDebug1(InputValue value) { //shift + D + 1
        if (value.Get<float>() == 1) {
            SaveManager.DeleteAll();
        }
    }

    void OnDebug2(InputValue value) { //shift + D + 2
        if (value.Get<float>() == 1) {

        }
    }
}