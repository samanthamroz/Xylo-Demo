using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    private Dictionary<string, Vector3> levelSelectCameraPositions = new() {
        {"title", new Vector3(.03f, 100, 11.3f)},
        {"credits", new Vector3(32.1f, -3.75f, -44.7f)},
        {"level0", new Vector3(.03f, -0.25f, 11.3f)},
    };

    public static CameraManager self;
	[SerializeField] private GameObject cameraPrefab, lookAtPrefab;
    private GameObject cameraObject, lookAtObject, cinematicLookAtObject;
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
                startingZoom = 25f;
                break;
            case 1:
                lookAtPointResetPos = new Vector3(0, 0, 0);
                cameraHeight = 2f;
                startingZoom = 35f;
                break;
        }

        currentZoom = startingZoom;
        zoomGoal = currentZoom;

        ResetCamera();
	}
    public void InstantiateTitleCamera() {
        cameraObject = Instantiate(cameraPrefab);
        cam = cameraObject.GetComponent<Camera>();
        lookAtObject = Instantiate(lookAtPrefab);

        ReturnToTitle();
    }
    private void ReturnToTitle() {
        lookAtPointResetPos = levelSelectCameraPositions["title"];
        cameraHeight = 0f;
        startingZoom = 25f;
        currentZoom = startingZoom;
        zoomGoal = currentZoom;

        ResetCamera();
    }

    private void ResetCamera() {
        lookAtObject.transform.position = lookAtPointResetPos;
        lookAtObject.transform.LookAt(cam.transform);
        StartCoroutine(PlaceCamera());
    }
    private IEnumerator PlaceCamera() {
        //assumes lookAtObject has been placed already

        float yRotationRadians = lookAtObject.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        // Calculate offset in x/z plane
        float xOffset = currentZoom * Mathf.Sin(yRotationRadians);
        float zOffset = currentZoom * Mathf.Cos(yRotationRadians);


        Vector3 newCameraPosition = new Vector3(
            lookAtObject.transform.position.x + xOffset, 
            lookAtObject.transform.position.y + cameraHeight, 
            lookAtObject.transform.position.z + zOffset);

        float time = 1f;
        LeanTween.move(cameraObject, newCameraPosition, time);

        float elapsed = 0f;
        while (elapsed < time)
        {
            cam.transform.LookAt(lookAtObject.transform); // keep looking during movement
            elapsed += Time.deltaTime;
            yield return null; // wait for next frame
        }
        cam.transform.LookAt(lookAtObject.transform);
    }

    public void SwitchLookAtObject(GameObject newLookAtPoint) {
        lookAtObject = newLookAtPoint;
        StartCoroutine(PlaceCamera());
    }
    public void SwitchLookAtPosition(Vector3 newPostion) {
        lookAtObject.transform.position = newPostion;
        StartCoroutine(PlaceCamera());
    }
    public void SwitchLevelSelectIsland(string islandName) {
        SwitchLookAtPosition(levelSelectCameraPositions[islandName]);
    }

    public IEnumerator EnterCinematicMode(GameObject newLookAtObject = null) {
        ControlsManager.self.EnterCinematicMode();

        LeanTween.moveLocal(cameraObject, new Vector3(0, 5, 16), .25f);
        yield return new WaitForSeconds(.25f);

        if (newLookAtObject == null) {
            StartCoroutine(DoCinematicCam(lookAtObject));
        } else {
            cinematicLookAtObject = newLookAtObject;
            StartCoroutine(DoCinematicCam(cinematicLookAtObject));
        }
    }
    public void ExitCinematicMode(bool isDeathPlane = false) {
        isCinematicCamera = false;
        if (isDeathPlane) {
            ResetCamera();
        } else {
            currentZoom = Vector3.Distance(cam.transform.position, lookAtObject.transform.position);
            zoomGoal = currentZoom;
            StartCoroutine(PlaceCamera());
        }
    }
    private IEnumerator DoCinematicCam(GameObject lookHere) {
        isCinematicCamera = true;
        while(isCinematicCamera) {
            lookAtObject.transform.position = lookHere.transform.position;
            lookAtObject.transform.LookAt(cam.transform);
            cam.transform.LookAt(lookAtObject.transform);
            yield return null;    
        }
        
        isCinematicCamera = false;
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
                Vector3 howMuchToMove = deltaPosition * panDistancePerFrame;// * mouseSpeed;

                //Move camera and the place it is facing
                lookAtObject.transform.position += howMuchToMove;
                cam.transform.position += howMuchToMove;
                cam.transform.LookAt(lookAtObject.transform);

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
                StartCoroutine(PlaceCamera());
                yield return null;
            }
        } else {
            while (currentZoom >= zoomGoal) {
                currentZoom -= zoomDistancePerFrame;
                StartCoroutine(PlaceCamera());
                yield return null;
            }
        }
    }
}