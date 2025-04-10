using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager self;
	public GameObject cameraPrefab, lookAtPrefab;
    private GameObject cameraObject, lookAtObject;
	private Camera cam;

    private Vector3 mousePosition { get { return MouseManager.self.mousePosition; } }

    public bool isRotating;
    public bool isPanning;

    private Vector3 lastPositionInWorld;
    private Vector3 lastMousePosition;
    public float distanceFromLookAtCoordinates = 20f;

    public float panDistancePerFrame = .005f;
    public float rotateDistancePerFrame = .1f;

    private float zoomAllowance = 4;
    private float baseZoom;
    public float scrollDistancePerFrame = 0.05f;
    float scrollGoal;

	void Awake() {
		if (self == null) {
			self = this;
			SceneManager.sceneLoaded += InstantiateCamera;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }

    private void InstantiateCamera(Scene scene, LoadSceneMode mode) {
        cameraObject = Instantiate(cameraPrefab);
        lookAtObject = Instantiate(lookAtPrefab);

		cam = cameraObject.GetComponent<Camera>();
        baseZoom = cam.orthographicSize;
        scrollGoal = cam.orthographicSize;

        lookAtObject.transform.LookAt(cam.transform);
        PlaceCam();
        //x coordinate = lookAt.x + distance * cos (180 - angle)
        //y coordinate = lookAt.y + distance * sin (180 - angle)
	}

    private void PlaceCam() {
        //assumes look at point has been moved
        cam.transform.position = new Vector3(
            lookAtObject.transform.position.x + distanceFromLookAtCoordinates * (float)Math.Cos(lookAtObject.transform.rotation.eulerAngles.y), 
            cam.transform.position.y, 
            lookAtObject.transform.position.z + distanceFromLookAtCoordinates * (float)Math.Sin(lookAtObject.transform.rotation.eulerAngles.y));
        cam.transform.LookAt(lookAtObject.transform);

    }

    public void DoPan() {
        lastPositionInWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distanceFromLookAtCoordinates));;
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
                Vector3 newPositioninWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distanceFromLookAtCoordinates));

                print(newPositioninWorld);
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
                //PlaceCam();
                //lookAtObject.transform.LookAt(cam.transform);

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
        if ((scrollInput < 0 && scrollGoal >= baseZoom - zoomAllowance)
                || (scrollInput > 0 && scrollGoal <= baseZoom + zoomAllowance)) {
            scrollGoal += scrollInput;
        }
        StartCoroutine(Scroll(scrollInput));
    }
    private IEnumerator Scroll(float scrollInput) {
        if (scrollInput == 1) {
            while (cam.orthographicSize <= scrollGoal) {
                cam.orthographicSize += scrollDistancePerFrame;
                yield return null;
            }
        } else {
            while (cam.orthographicSize >= scrollGoal) {
                cam.orthographicSize -= scrollDistancePerFrame;
                yield return null;
            }
        }
    }
}