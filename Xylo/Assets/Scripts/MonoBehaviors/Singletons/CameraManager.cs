using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class CameraManager : MonoBehaviour {
    private TitleScreenCameraManager tscm;
    private CinematicCameraManager ccm;
    public static CameraManager self;

    public CamMode currentMode;
    public bool isCinematicCamera { get { return currentMode == CamMode.CINEMATIC; } }
    [SerializeField] private GameObject cameraPrefab, lookAtPrefab;
    private GameObject cameraObject, lookAtObject, currentlookAtObject;
    private Camera cam;
    [HideInInspector] public Vector3 camPosition { get { return cameraObject.transform.position; } }
    private Vector3 mousePosition { get { return ControlsManager.self.mousePosition; } }
    [HideInInspector] public bool isRotating, isPanning;

    private Vector3 lookAtPointResetPos; //this is the position the lookAtObject is reset to
    private float startingZoom, cameraHeight; //this is the difference in height between the lookAtObject and the camera
    [SerializeField] private Vector2 cameraPlacementRadius;

    [SerializeField] private float panDistancePerFrame = .01f;
    [SerializeField] private float rotateDistancePerFrame = .1f;
    private Vector3 lastPositionInWorld, lastMousePosition;
    private float zoomMin = 5f;
    private float zoomMax = 40f;
    [SerializeField] private float zoomDistancePerFrame = 0.05f;
    private float zoomGoal, currentZoom;

    void Awake() {
        if (self == null) {
            self = this;
            ccm = new CinematicCameraManager();
            tscm = new TitleScreenCameraManager();
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
    public void InstantiateCamera(int levelNumber) {
        cameraObject = Instantiate(cameraPrefab);
        cam = cameraObject.GetComponent<Camera>();

        lookAtObject = Instantiate(lookAtPrefab);
        currentlookAtObject = lookAtObject;

        SetCameraMode(CamMode.NORMAL);

        switch (levelNumber) {
            case 1:
                lookAtPointResetPos = new Vector3(6, 12, -6);
                cameraHeight = 2f;
                startingZoom = 17f;
                break;
            default:
                break;
        }
        currentZoom = startingZoom;
        zoomGoal = currentZoom;

        StartCoroutine(PlaceCamera(0f, true));
    }
    public void InstantiateTitleCamera() {
        cameraObject = Instantiate(cameraPrefab);
        cam = cameraObject.GetComponent<Camera>();
        lookAtObject = Instantiate(lookAtPrefab);
        currentlookAtObject = lookAtObject;
        SetCameraMode(CamMode.TITLESCREEN);

        tscm.ReturnToTitle();
    }

    //this function assumes the lookAtObject has been placed already
    private Vector3 GetNewCameraPosition() {
        float yRotationRadians = currentlookAtObject.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        // Calculate offset in x/z plane
        float xOffset = currentZoom * Mathf.Sin(yRotationRadians);
        float zOffset = currentZoom * Mathf.Cos(yRotationRadians);

        //get position to travel to
        return new Vector3(
            currentlookAtObject.transform.position.x + xOffset,
            currentlookAtObject.transform.position.y + cameraHeight,
            currentlookAtObject.transform.position.z + zOffset);
    }
    private Vector3 GetNewCameraPosition(Vector3 newHypothetical) {
        float yRotationRadians = currentlookAtObject.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        // Calculate offset in x/z plane
        float xOffset = currentZoom * Mathf.Sin(yRotationRadians);
        float zOffset = currentZoom * Mathf.Cos(yRotationRadians);

        //get position to travel to
        return new Vector3(
            newHypothetical.x + xOffset,
            newHypothetical.y + cameraHeight,
            newHypothetical.z + zOffset);
    }
    private IEnumerator PlaceCamera(float time = 0f, bool reset = false) {
        if (reset) {
            currentlookAtObject.transform.position = lookAtPointResetPos;
            //currentlookAtObject.transform.LookAt(cam.transform);
        }

        //get position to travel to
        Vector3 newCameraPosition = GetNewCameraPosition();

        //get rotation to travel to
        cam.transform.GetPositionAndRotation(out Vector3 originalPosition, out Quaternion originalRotation);
        cam.transform.position = newCameraPosition;
        cam.transform.LookAt(currentlookAtObject.transform);
        Vector3 newRotation = cam.transform.eulerAngles;
        cam.transform.SetPositionAndRotation(originalPosition, originalRotation);

        //do tween
        LeanTween.cancel(cameraObject);
        LeanTween.move(cameraObject, newCameraPosition, time).setEaseInOutSine();
        LeanTween.rotate(cameraObject, newRotation, time).setEaseInOutSine();
        yield return new WaitForSeconds(time);

        cam.transform.LookAt(currentlookAtObject.transform);
    }

    private void SwitchLookAtObject(GameObject newLookAtPoint, bool replace = true) {
        currentlookAtObject = newLookAtPoint;
        if (replace) {
            StartCoroutine(PlaceCamera());
        }
    }
    private void MoveLookAtPosition(Vector3 newPostion) {
        currentlookAtObject.transform.position = newPostion;
        currentlookAtObject.transform.eulerAngles = new Vector3(0, 0, 0);
        StartCoroutine(PlaceCamera(1f));
    }
    public void SwitchTitleScreenPosition(string key) {
        tscm.MoveToIsland(key);
    }

    private void SetCameraMode(CamMode mode) {
        if (mode == currentMode) return;

        if (currentMode != CamMode.NORMAL && mode == CamMode.NORMAL) {
            ControlsManager.self.ActivateMainMap();
            GUIManager.self.TogglePlayButtonImage(true);
            currentlookAtObject = lookAtObject;
        }
        if (currentMode == CamMode.CINEMATIC && mode != CamMode.CINEMATIC) {
            StartCoroutine(GUIManager.self.DeactivateCinematicUI());
            currentZoom = Vector3.Distance(cam.transform.position, currentlookAtObject.transform.position);
            zoomGoal = currentZoom;
            StartCoroutine(PlaceCamera(.5f));
        }

        currentMode = mode;

        if (mode == CamMode.CINEMATIC) {
            ControlsManager.self.ActivateCinematicMap();
            StartCoroutine(GUIManager.self.ActivateCinematicUI());
        }

    }

    public void DoBeginAttempt(int sectionNum, GameObject lookAtNewObj) {
        GUIManager.self.TogglePlayButtonImage(false);

        SetCameraMode(CamMode.CINEMATIC);
        SwitchLookAtObject(lookAtNewObj, false);

        StartCoroutine(ccm.DoSectionView(sectionNum));
    }
    public void DoMoveToNextSection(int sectionNum) {
        GUIManager.self.TogglePlayButtonImage(true);

        SetCameraMode(CamMode.CINEMATIC);
        SwitchLookAtObject(lookAtObject, false);

        StartCoroutine(ccm.DoMoveToNextSection(sectionNum));
    }
    public void DoEndOfLevel() {
        GUIManager.self.ActivateWinMenuUI();
        SetCameraMode(CamMode.NORMAL);
    }
    public void DoPan() {
        float distance = Vector3.Distance(cam.transform.position, currentlookAtObject.transform.position);
        lastPositionInWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distance));
        lastMousePosition = mousePosition;
        StartCoroutine(Pan());
    }
    private IEnumerator Pan() {
        isPanning = true;
        while (isPanning) {
            //if we haven't started moving
            if (lastMousePosition != mousePosition) {
                //Calculate new world position for camera at mouse point
                //float z = Camera.main.WorldToScreenPoint(lookAtWorldCoordinates).z;
                float distance = Vector3.Distance(cam.transform.position, currentlookAtObject.transform.position);
                Vector3 newPositioninWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distance));

                //Calculate how far to move the camera to get to the new world position
                Vector3 deltaPosition = lastPositionInWorld - newPositioninWorld;

                //Calculate speed of mouse
                Vector2 mouseDelta = mousePosition - lastMousePosition;
                float mouseSpeed = mouseDelta.magnitude;

                //Scale by speed of mouse
                Vector3 howMuchToMove = deltaPosition * panDistancePerFrame;// * (zoomMax / currentZoom);

                //Move camera and the place it is facing
                Vector3 placeToMoveLookAtPoint = currentlookAtObject.transform.position + howMuchToMove;
                if (Math.Abs(placeToMoveLookAtPoint.x - lookAtPointResetPos.x) < cameraPlacementRadius.x &&
                    Math.Abs(placeToMoveLookAtPoint.y - lookAtPointResetPos.y) < cameraPlacementRadius.y) {
                    currentlookAtObject.transform.position += howMuchToMove;
                    StartCoroutine(PlaceCamera(0f));
                    lastPositionInWorld = newPositioninWorld;
                }
            }
            lastMousePosition = mousePosition;
            yield return null;
        }
        isPanning = false;
    }
    public void DoRotate() {
        lastMousePosition = mousePosition;
        StartCoroutine(Rotate());
    }
    private IEnumerator Rotate() {
        isRotating = true;

        while (isRotating) {
            float mouseDeltaX = mousePosition.x - lastMousePosition.x;

            Vector3 screenRotationAxis = new Vector3(-mousePosition.y, mousePosition.x, 0).normalized;
            Vector3 worldRotationAxis = cam.transform.rotation * transform.TransformDirection(screenRotationAxis);

            cam.transform.RotateAround(currentlookAtObject.transform.position, new Vector3(0, 1, 0), rotateDistancePerFrame * mouseDeltaX);
            cam.transform.LookAt(currentlookAtObject.transform);
            currentlookAtObject.transform.LookAt(cam.transform);
            lastMousePosition = mousePosition;

            yield return null;
        }
        isRotating = false;
    }
    public void DoScroll(float scrollInput) {
        scrollInput *= -1;
        if ((scrollInput < 0 && zoomGoal >= zoomMin)
                || (scrollInput > 0 && zoomGoal <= zoomMax)) {
            zoomGoal += scrollInput;
        }
        StartCoroutine(Scroll(scrollInput));
    }
    private IEnumerator Scroll(float scrollInput) {
        if (scrollInput == 1) {
            while (currentZoom <= zoomGoal) {
                currentZoom += zoomDistancePerFrame;
                StartCoroutine(PlaceCamera(0f));
                yield return null;
            }
        }
        else {
            while (currentZoom >= zoomGoal) {
                currentZoom -= zoomDistancePerFrame;
                StartCoroutine(PlaceCamera(0f));
                yield return null;
            }
        }
    }
    public void ManualZoom(float amountToZoom, float animationTime = .5f) {
        currentZoom += amountToZoom;
        StartCoroutine(PlaceCamera(animationTime));
    }

    //-TSCM-------------------------------------------------------------------------------
    private class TitleScreenCameraManager {
        private readonly Dictionary<string, Vector3> levelSelectCameraPositions = new() {
            {"title", new Vector3(.03f, 50f, 11.3f)},
            {"credits", new Vector3(32.1f, -3.75f, -44.7f)},
            {"level0", new Vector3(.03f, -0.25f, 11.3f)}
        };
        public void ReturnToTitle(float time = 0f) {
            self.lookAtPointResetPos = levelSelectCameraPositions["title"];
            self.cameraHeight = 0f;
            self.startingZoom = 25f;
            self.currentZoom = self.startingZoom;
            self.zoomGoal = self.currentZoom;

            self.StartCoroutine(self.PlaceCamera(time, true));
        }

        public void MoveToIsland(string key) {
            self.currentlookAtObject.transform.position = levelSelectCameraPositions[key];
            self.currentlookAtObject.transform.eulerAngles = new Vector3(0, 0, 0);
            self.StartCoroutine(self.PlaceCamera(1f));
        }
    }

    //-----CCM-------------------------------------------------------------------------------------------
    private class CinematicCameraManager {
        private readonly List<List<Vector3>> sectionCinematicViewPoints = new()
        {
            //level 1
            new() {
                new Vector3(8, 15, -15), new Vector3(16, 14, -15), new Vector3(22, 13, -15), new Vector3(30, 12, -15)
            }
        };

        private readonly Vector3[][] sectionGameViewPoints = {
            //level 1
            new Vector3[] {new(6, 12, -6), new(14, 11, -6), new(22, 10, -6), new(30, 9, -6)}
        };

        public IEnumerator DoSectionView(int sectionNum) {
            self.cam.transform.GetPositionAndRotation(out Vector3 originalPosition, out Quaternion originalRotation);
            float time = .5f;

            LeanTween.moveLocal(self.cameraObject, sectionCinematicViewPoints[LoadingManager.self.GetCurrentLevelNumber()][sectionNum], time).setEaseInOutSine();

            float elapsed = 0f;
            while (elapsed < time) {
                //what to update here
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / time);
                float easedT = LeanTween.easeInOutSine(0f, 1f, t);



                // Calculate desired rotation toward current lookHere position
                Vector3 direction = (self.currentlookAtObject.transform.position - self.cam.transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Smoothly interpolate rotation
                self.cam.transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, easedT);

                yield return null; // wait for next frame
            }

            while (self.currentlookAtObject != self.lookAtObject) {
                self.currentlookAtObject.transform.LookAt(self.cam.transform);
                self.cam.transform.LookAt(self.currentlookAtObject.transform);
                yield return null;
            }
        }

        public IEnumerator DoMoveToNextSection(int sectionNum) {
            self.cam.transform.GetPositionAndRotation(out Vector3 originalPosition, out Quaternion originalRotation);
            float time = 1f;

            LeanTween.moveLocal(self.lookAtObject, sectionGameViewPoints[LoadingManager.self.GetCurrentLevelNumber()][sectionNum], time).setEaseInOutSine();
            LeanTween.moveLocal(self.cam.gameObject, self.GetNewCameraPosition(sectionGameViewPoints[LoadingManager.self.GetCurrentLevelNumber()][sectionNum]), time).setEaseInOutSine();

            float elapsed = 0f;
            while (elapsed < time) {
                //what to update here
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / time);
                float easedT = LeanTween.easeInOutSine(0f, 1f, t);

                // Calculate desired rotation toward current lookHere position
                Vector3 direction = (self.currentlookAtObject.transform.position - self.cam.transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Smoothly interpolate rotation
                self.cam.transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, easedT);


                yield return null; // wait for next frame
            }
            self.lookAtPointResetPos = sectionGameViewPoints[LoadingManager.self.GetCurrentLevelNumber()][sectionNum];
            self.SetCameraMode(CamMode.NORMAL);
            AudioManager.self.PlayMelodyForCurrentSection();
        }
    }
}