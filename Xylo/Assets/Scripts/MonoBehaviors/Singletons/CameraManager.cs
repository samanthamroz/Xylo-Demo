using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager self;
	[SerializeField] private GameObject cameraPrefab, lookAtPrefab;
    private GameObject cameraObject, lookAtObject, cinematicLookAtObject;
	private Camera cam;

    private Vector3 mousePosition { get { return ControlsManager.self.mousePosition; } }

    [HideInInspector] public bool isCinematicCamera;
    [HideInInspector] public bool isRotating;
    [HideInInspector] public bool isPanning;

    private Vector3 lookAtPointResetPos = Vector3.zero;
    [SerializeField] float cameraHeight = 2f;

    private Vector3 lastPositionInWorld;
    private Vector3 lastMousePosition;
    [SerializeField] private float currentZoom = 35f;

    [SerializeField] private float panDistancePerFrame = .005f;    
    [SerializeField] private float rotateDistancePerFrame = .1f;

    private float zoomMin = 5f;
    private float zoomMax = 40f;
    [SerializeField] private float zoomDistancePerFrame = 0.05f;
    private float zoomGoal;

	void Awake() {
		if (self == null) {
			self = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }
    public void InstantiateCamera() {
        cameraObject = Instantiate(cameraPrefab);
        lookAtObject = Instantiate(lookAtPrefab);

        cam = cameraObject.GetComponent<Camera>();
        zoomGoal = currentZoom;

        ResetCamera();
	}

    private void ResetCamera() {
        lookAtObject.transform.position = lookAtPointResetPos;
        lookAtObject.transform.LookAt(cam.transform);
        PlaceCamera();
    }
    private void PlaceCamera() {
        //assumes lookAtObject has been placed already

        float yRotationRadians = lookAtObject.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        // Calculate offset in x/z plane
        float xOffset = currentZoom * Mathf.Sin(yRotationRadians);
        float zOffset = currentZoom * Mathf.Cos(yRotationRadians);

        cam.transform.position = new Vector3(
            lookAtObject.transform.position.x + xOffset, 
            lookAtObject.transform.position.y + cameraHeight, 
            lookAtObject.transform.position.z + zOffset);
        cam.transform.LookAt(lookAtObject.transform);
    }

    void Update() {

    }

    public void EnterCinematicMode(GameObject newLookAtObject = null) {
        cam.transform.position = new Vector3(0, 5, 16);
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
            PlaceCamera();
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
                PlaceCamera();

                yield return null;
            }
        } else {
            while (currentZoom >= zoomGoal) {
                currentZoom -= zoomDistancePerFrame;
                PlaceCamera();

                yield return null;
            }
        }
    }
}