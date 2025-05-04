using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    private Dictionary<string, Vector3> levelSelectCameraPositions = new() {
        {"title", new Vector3(.03f, 50f, 11.3f)},
        {"credits", new Vector3(32.1f, -3.75f, -44.7f)},
        {"level0", new Vector3(.03f, -0.25f, 11.3f)}
    };

    public static CameraManager self;
	[SerializeField] private GameObject cameraPrefab, lookAtPrefab;
    private GameObject cameraObject, lookAtObject;
	private Camera cam;
    [HideInInspector] public Vector3 camPosition { get { return cameraObject.transform.position; } }

    private Vector3 mousePosition { get { return ControlsManager.self.mousePosition; } }

    [HideInInspector] public bool isCinematicCamera, isRotating, isPanning;

    private Vector3 lookAtPointResetPos; //this is the position the lookAtObject is reset to
    private float startingZoom, cameraHeight; //this is the difference in height between the lookAtObject and the camera

    [SerializeField] private float panDistancePerFrame = .005f;    
    [SerializeField] private float rotateDistancePerFrame = .1f;
    private Vector3 lastPositionInWorld, lastMousePosition;

    private float zoomMin = 5f;
    private float zoomMax = 40f;
    [SerializeField] private float zoomDistancePerFrame = 0.05f;
    private float zoomGoal, currentZoom;

	void Awake() {
		if (self == null) {
			self = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }
    public void InstantiateCamera(int levelNumber) {
        cameraObject = Instantiate(cameraPrefab);
        cam = cameraObject.GetComponent<Camera>();

        lookAtObject = Instantiate(lookAtPrefab);

        switch(levelNumber) {
            case 0:
                lookAtPointResetPos = levelSelectCameraPositions["level0"];
                cameraHeight = 0f;
                startingZoom = 30f;
                break;
            case 1:
                lookAtPointResetPos = new Vector3(0, 0, 0);
                cameraHeight = 0f;
                startingZoom = 35f;
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

        ReturnToTitle();
    }
    private void ReturnToTitle(float time = 0f) {
        lookAtPointResetPos = levelSelectCameraPositions["title"];
        cameraHeight = 0f;
        startingZoom = 25f;
        currentZoom = startingZoom;
        zoomGoal = currentZoom;

        ResetCamera(time, true);
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

    public void SwitchLookAtObject(GameObject newLookAtPoint) {
        lookAtObject = newLookAtPoint;
        StartCoroutine(PlaceCamera());
    }
    public void SwitchLookAtPosition(Vector3 newPostion) {
        lookAtObject.transform.position = newPostion;
        lookAtObject.transform.eulerAngles = new Vector3(0, 0, 0);
        StartCoroutine(PlaceCamera(1f));
    }
    public void SwitchLevelSelectIsland(string islandName) {
        SwitchLookAtPosition(levelSelectCameraPositions[islandName]);
    }
    public void ManualZoom(float amountToZoom, float animationTime = .5f) {
        currentZoom += amountToZoom;
        StartCoroutine(PlaceCamera(animationTime));
    }
    public void EnterCinematicMode(GameObject newLookAtObject = null) {
        ControlsManager.self.EnterCinematicMode();
        StartCoroutine(GUIManager.self.ActivateCinematicUI());
        if (newLookAtObject == null) {
            StartCoroutine(DoCinematicCam(lookAtObject));
        } else {
            StartCoroutine(DoCinematicCam(newLookAtObject));
        }
    }
    private IEnumerator DoCinematicCam(GameObject lookHere) {
        isCinematicCamera = true;

        cam.transform.GetPositionAndRotation(out Vector3 originalPosition, out Quaternion originalRotation);
        float time = 1f;

        LeanTween.moveLocal(cameraObject, new Vector3(0, 5, 16), time).setEaseInOutSine();

        float elapsed = 0f;
        while (elapsed < time)
        {
            //what to update here
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / time);
            float easedT = LeanTween.easeInOutSine(0f, 1f, t);

            // Calculate desired rotation toward current lookHere position
            Vector3 direction = (lookHere.transform.position - cam.transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly interpolate rotation
            cam.transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, easedT);

            yield return null; // wait for next frame
        }
        
        //yield return new WaitForSeconds(1f);

        while(isCinematicCamera) {
            lookAtObject.transform.position = lookHere.transform.position;
            lookAtObject.transform.LookAt(cam.transform);
            cam.transform.LookAt(lookAtObject.transform);
            yield return null;    
        }
    }
    public void ExitCinematicMode(bool isDeathPlane = false) {
        isCinematicCamera = false;
        StartCoroutine(GUIManager.self.DeactivateCinematicUI());
        if (isDeathPlane) {
            currentZoom = startingZoom;
            zoomGoal = currentZoom;
            ResetCamera(.5f, true);
        } else {
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

    public void DoRotate() {
        lastMousePosition = mousePosition;
        StartCoroutine(Rotate());
    }
    private IEnumerator Rotate() {
        isRotating = true;

        while(isRotating) {
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
        } else {
            while (currentZoom >= zoomGoal) {
                currentZoom -= zoomDistancePerFrame;
                StartCoroutine(PlaceCamera(0f));
                yield return null;
            }
        }
    }
}