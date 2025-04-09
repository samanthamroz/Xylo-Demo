using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class CameraManager : MonoBehaviour
{
    public static CameraManager self;
	public GameObject cameraPrefab, cameraObject;
	private Camera cam;

    private Vector3 mousePosition { get { return MouseManager.self.mousePosition; } }

    public bool isRotating;
    public bool isPanning;

    private Vector3 lastPositionInWorld;
    private Vector3 lastMousePosition;
    private Vector3 lookAtWorldCoordinates = Vector3.zero;
    private float distanceFromLookAtCoordinates;

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
        GameObject camera = Instantiate(cameraPrefab);
		cam = camera.GetComponent<Camera>();
        baseZoom = cam.orthographicSize;
        scrollGoal = cam.orthographicSize;
        cam.transform.position = new Vector3(20, 0, -20);
        cam.transform.LookAt(Vector3.zero);
        distanceFromLookAtCoordinates = Vector3.Distance(cam.transform.position, lookAtWorldCoordinates);
	}

    public void DoPan() {
        lastPositionInWorld = cam.transform.position;
        lastMousePosition = mousePosition;
        StartCoroutine(Pan());
    }

    private IEnumerator Pan() {
        isPanning = true;
        while(isPanning) {
            //if we haven't started moving
            if (lastMousePosition == mousePosition) {
                yield return null;
            }

            //Calculate new world position for camera at mouse point
            float z = Camera.main.WorldToScreenPoint(lookAtWorldCoordinates).z;
            Vector3 newPositioninWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, z));

            //Calculate how far to move the camera to get to the new world position
            Vector3 deltaPosition = lastPositionInWorld - newPositioninWorld;
            
            //Calculate speed of mouse
            Vector2 mouseDelta = mousePosition - lastMousePosition;
            float mouseSpeed = mouseDelta.magnitude;

            //Scale by speed of mouse
            Vector3 howMuchToMove = deltaPosition * panDistancePerFrame * mouseSpeed;

            //Move camera and the place it is facing
            cam.transform.position += howMuchToMove;
            lookAtWorldCoordinates += howMuchToMove;

            //Update direction of camera
            cam.transform.LookAt(lookAtWorldCoordinates);

            //Update variables for next loop
            lastMousePosition = mousePosition;
            lastPositionInWorld = newPositioninWorld;

            //Loop
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
            
            cam.transform.RotateAround(lookAtWorldCoordinates, new Vector3(0, 1, 0), rotateDistancePerFrame * mouseDeltaX);
            cam.transform.LookAt(lookAtWorldCoordinates);

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