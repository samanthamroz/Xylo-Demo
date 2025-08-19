using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    private TitleScreenCameraManager tscm;
    private CinematicCameraManager ccm;
    public static CameraManager self;

    public CamMode currentMode;
    public bool isCinematicCamera { get { return currentMode == CamMode.CINEMATIC; } }
    [SerializeField] private GameObject cameraPrefab, lookAtPrefab;
    private GameObject cameraObject, lookAtObject;
    private Camera cam;
    [HideInInspector] public Vector3 camPosition { get { return cameraObject.transform.position; } }
    private Vector3 mousePosition { get { return ControlsManager.self.mousePosition; } }
    [HideInInspector] public bool isRotating, isPanning;

    private Vector3 lookAtPointResetPos; //this is the position the lookAtObject is reset to
    private float startingZoom, cameraHeight; //this is the difference in height between the lookAtObject and the camera

    [SerializeField] private float panDistancePerFrame = .005f;
    [SerializeField] private float rotateDistancePerFrame = .1f;
    private Vector3 lastPositionInWorld, lastMousePosition;
    private float zoomMin = 5f;
    private float zoomMax = 40f;
    [SerializeField] private float zoomDistancePerFrame = 0.05f;
    private float zoomGoal, currentZoom;

    void Awake()
    {
        if (self == null)
        {
            self = this;
            ccm = new CinematicCameraManager();
            tscm = new TitleScreenCameraManager();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void InstantiateCamera(int levelNumber)
    {
        cameraObject = Instantiate(cameraPrefab);
        cam = cameraObject.GetComponent<Camera>();

        lookAtObject = Instantiate(lookAtPrefab);

        switch (levelNumber)
        {
            case 0:
                tscm.ResetCamera();
                break;
            case 1:
                lookAtPointResetPos = new Vector3(0, 0, 0);
                cameraHeight = 0f;
                startingZoom = 25f;
                break;
        }
        currentZoom = startingZoom;
        zoomGoal = currentZoom;

        ResetCamera(0f);
	}
    public void InstantiateTitleCamera() {
        cameraObject = Instantiate(cameraPrefab);
        cam = cameraObject.GetComponent<Camera>();
        lookAtObject = Instantiate(lookAtPrefab);

        tscm.ReturnToTitle();
    }

    private void ResetCamera(float time, bool resetRotation = false) {
        lookAtObject.transform.position = lookAtPointResetPos;
        if (resetRotation) lookAtObject.transform.LookAt(cam.transform);
        StartCoroutine(PlaceCamera(time));
    }

    //this function assumes the lookAtObject has been placed already
    private IEnumerator PlaceCamera(float time = 0f) { 
        //get rotation of lookAtObject
        float yRotationRadians = lookAtObject.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        // Calculate offset in x/z plane
        float xOffset = currentZoom * Mathf.Sin(yRotationRadians);
        float zOffset = currentZoom * Mathf.Cos(yRotationRadians);

        //get position to travel to
        Vector3 newCameraPosition = new Vector3(
            lookAtObject.transform.position.x + xOffset, 
            lookAtObject.transform.position.y + cameraHeight, 
            lookAtObject.transform.position.z + zOffset);

        //get rotation to travel to
        cam.transform.GetPositionAndRotation(out Vector3 originalPosition, out Quaternion originalRotation);
        cam.transform.position = newCameraPosition;
        cam.transform.LookAt(lookAtObject.transform);
        Vector3 newRotation = cam.transform.eulerAngles;
        cam.transform.SetPositionAndRotation(originalPosition, originalRotation);

        //do tween
        LeanTween.cancel(cameraObject);
        LeanTween.move(cameraObject, newCameraPosition, time).setEaseInOutSine();
        LeanTween.rotate(cameraObject, newRotation, time).setEaseInOutSine();
        yield return new WaitForSeconds(time);

        cam.transform.LookAt(lookAtObject.transform);
    }

    public void SwitchLookAtObject(GameObject newLookAtPoint)
    {
        lookAtObject = newLookAtPoint;
        StartCoroutine(PlaceCamera());
    }
    public void SwitchLookAtPosition(Vector3 newPostion)
    {
        lookAtObject.transform.position = newPostion;
        lookAtObject.transform.eulerAngles = new Vector3(0, 0, 0);
        StartCoroutine(PlaceCamera(1f));
    }
    public void SwitchTitleScreenPosition(string key)
    {
        tscm.MoveToIsland(key);
    }
    public void ManualZoom(float amountToZoom, float animationTime = .5f)
    {
        currentZoom += amountToZoom;
        StartCoroutine(PlaceCamera(animationTime));
    }
    public void SetCameraMode(CamMode mode)
    {
        if (currentMode == CamMode.CINEMATIC && mode != CamMode.CINEMATIC)
        {
            ExitCinematicMode();
        }
        if (mode == CamMode.CINEMATIC)
        {
            ccm.EnterCinematicMode();
        }
        currentMode = mode;
    }
    private void ExitCinematicMode(bool isDeathPlane = false)
    {
        StartCoroutine(GUIManager.self.DeactivateCinematicUI());
        if (isDeathPlane)
        {
            currentZoom = startingZoom;
            zoomGoal = currentZoom;
            StartCoroutine(PlaceCamera(.5f, true));
        }
        else
        {
            currentZoom = Vector3.Distance(cam.transform.position, lookAtObject.transform.position);
            zoomGoal = currentZoom;
            StartCoroutine(PlaceCamera(.5f));
        }
    }

    public void DoPan() {
        lastPositionInWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, currentZoom));;
        lastMousePosition = mousePosition;
        StartCoroutine(Pan());
    }
    private IEnumerator Pan() {
        isPanning = true;
        while(isPanning) {
            //if we haven't started moving
            if (lastMousePosition != mousePosition) {
                //Calculate new world position for camera at mouse point
                //float z = Camera.main.WorldToScreenPoint(lookAtWorldCoordinates).z;
                Vector3 newPositioninWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, currentZoom));

                //Calculate how far to move the camera to get to the new world position
                Vector3 deltaPosition = lastPositionInWorld - newPositioninWorld;
                
                //Calculate speed of mouse
                Vector2 mouseDelta = mousePosition - lastMousePosition;
                float mouseSpeed = mouseDelta.magnitude;

                //Scale by speed of mouse
                Vector3 howMuchToMove = deltaPosition * panDistancePerFrame;// * (zoomMax / currentZoom);

                //Move camera and the place it is facing
                lookAtObject.transform.position += howMuchToMove;
                StartCoroutine(PlaceCamera());

                //Update variables for next loop
                lastPositionInWorld = newPositioninWorld;
                
                //Loop
            }
            lastMousePosition = mousePosition;
            yield return null;    
        }
        isPanning = false;
    }

    public void DoRotate()
    {
        lastMousePosition = mousePosition;
        StartCoroutine(Rotate());
    }
    private IEnumerator Rotate()
    {
        isRotating = true;

        while (isRotating)
        {
            float mouseDeltaX = mousePosition.x - lastMousePosition.x;

            Vector3 screenRotationAxis = new Vector3(-mousePosition.y, mousePosition.x, 0).normalized;
            Vector3 worldRotationAxis = cam.transform.rotation * transform.TransformDirection(screenRotationAxis);

            cam.transform.RotateAround(lookAtObject.transform.position, new Vector3(0, 1, 0), rotateDistancePerFrame * mouseDeltaX);
            cam.transform.LookAt(lookAtObject.transform);
            lookAtObject.transform.LookAt(cam.transform);
            lastMousePosition = mousePosition;

            yield return null;
        }
        isRotating = false;
    }

    public void DoScroll(float scrollInput)
    {
        scrollInput *= -1;
        if ((scrollInput < 0 && zoomGoal >= zoomMin)
                || (scrollInput > 0 && zoomGoal <= zoomMax))
        {
            zoomGoal += scrollInput;
        }
        StartCoroutine(Scroll(scrollInput));
    }
    private IEnumerator Scroll(float scrollInput)
    {
        if (scrollInput == 1)
        {
            while (currentZoom <= zoomGoal)
            {
                currentZoom += zoomDistancePerFrame;
                StartCoroutine(PlaceCamera(0f));
                yield return null;
            }
        }
        else
        {
            while (currentZoom >= zoomGoal)
            {
                currentZoom -= zoomDistancePerFrame;
                StartCoroutine(PlaceCamera(0f));
                yield return null;
            }
        }
    }

    private class TitleScreenCameraManager
    {
        private Dictionary<string, Vector3> levelSelectCameraPositions = new() {
            {"title", new Vector3(.03f, 50f, 11.3f)},
            {"credits", new Vector3(32.1f, -3.75f, -44.7f)},
            {"level0", new Vector3(.03f, -0.25f, 11.3f)}
        };

        public void ResetCamera()
        {

        }

        public void ReturnToTitle(float time = 0f)
        {
            self.lookAtPointResetPos = levelSelectCameraPositions["title"];
            self.cameraHeight = 0f;
            self.startingZoom = 25f;
            self.currentZoom = self.startingZoom;
            self.zoomGoal = self.currentZoom;

            self.StartCoroutine(self.PlaceCamera(time, true));
        }

        public void MoveToIsland(string key)
        {
            self.lookAtObject.transform.position = levelSelectCameraPositions[key];
            self.lookAtObject.transform.eulerAngles = new Vector3(0, 0, 0);
            self.StartCoroutine(self.PlaceCamera(1f));
        }
    }

    private class CinematicCameraManager
    {
        public void EnterCinematicMode()
        {
            ControlsManager.self.ActivateCinematicMap();
            self.StartCoroutine(GUIManager.self.ActivateCinematicUI());
            self.StartCoroutine(DoCinematicCam());
        }
        public IEnumerator DoCinematicCam()
        {
            self.cam.transform.GetPositionAndRotation(out Vector3 originalPosition, out Quaternion originalRotation);
            float time = 1f;

            LeanTween.moveLocal(self.cameraObject, new Vector3(0, 5, 16), time).setEaseInOutSine();

            float elapsed = 0f;
            while (elapsed < time)
            {
                //what to update here
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / time);
                float easedT = LeanTween.easeInOutSine(0f, 1f, t);

                // Calculate desired rotation toward current lookHere position
                Vector3 direction = (self.lookAtObject.transform.position - self.transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Smoothly interpolate rotation
                self.transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, easedT);

                yield return null; // wait for next frame
            }

            //yield return new WaitForSeconds(1f);

            while (self.isCinematicCamera)
            {
                self.lookAtObject.transform.position = self.lookAtObject.transform.position;
                self.lookAtObject.transform.LookAt(self.cam.transform);
                self.cam.transform.LookAt(self.lookAtObject.transform);
                yield return null;
            }
        }
    }
}