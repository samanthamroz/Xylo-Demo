using UnityEngine;
using System.Collections;
using System;

public partial class CameraManager : MonoBehaviour {
    private TitleScreenCameraManager tscm;
    private CinematicCameraManager ccm;
    public static CameraManager self;

    [SerializeField] CameraConfiguration cameraConfiguration;
    private Vector3 instantiateCameraPoint;


    private CamMode currentMode;
    
    [SerializeField] private GameObject cameraPrefab, lookAtPrefab;
    private GameObject cameraObject, lookAtObject, currentlookAtObject;
    private Camera cam;

    [HideInInspector] public bool isRotating, isPanning;

    private Vector3 lookAtPointResetPos, lastPositionInWorld, lastMousePosition;
    private float startingZoom, cameraHeight; //this is the difference in height between the lookAtObject and the camera
    private Vector2 cameraPlacementRadius = new(3, 2);

    private float panDistancePerFrame = .05f;
    private float rotateDistancePerFrame = .1f;
    private float zoomDistancePerFrame = 0.05f;
    private float zoomMin = 5f;
    private float zoomMax = 20f;
    private float zoomGoal, currentZoom;

    private Vector3 mousePosition => ControlsManager.self.mousePosition;

    [HideInInspector] public bool isCinematicCamera => currentMode == CamMode.CINEMATIC;
    [HideInInspector] public Vector3 camPosition => cameraObject.transform.position;


    void Awake() {
        if (self == null) {
            self = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
    public void InstantiateCamera() {
        cameraObject = Instantiate(cameraPrefab);
        cam = cameraObject.GetComponent<Camera>();

        int currentLevelNum = LoadingManager.self.GetCurrentLevelNumber();
        var cameraData = cameraConfiguration.GetLevelCameraData(currentLevelNum);

        var cinamaticPoints = cameraData.sectionCinematicViewPoints;
        var gamePoints = cameraData.sectionGameViewPoints;
        ccm = new CinematicCameraManager(cinamaticPoints, gamePoints);

        tscm = new TitleScreenCameraManager();

        lookAtObject = Instantiate(lookAtPrefab);
        lookAtObject.transform.rotation = Quaternion.Euler(cameraData.startingRotation);
        currentlookAtObject = lookAtObject;

        SetCameraMode(CamMode.NORMAL);

        lookAtPointResetPos = cameraData.startingPoint;
        cameraHeight = cameraData.startingHeight;
        startingZoom = cameraData.startingZoom;
        
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
        if (currentMode == CamMode.CINEMATIC) {
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
    public void DoEndOfLevel(GameObject lookAtNewObj) {
        SetCameraMode(CamMode.CINEMATIC);
        SwitchLookAtObject(lookAtNewObj, false);

        StartCoroutine(ccm.DoCinematicLevelOne());
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
}