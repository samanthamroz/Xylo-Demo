using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Security.Cryptography.X509Certificates;

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

    private float panDistancePerFrame = .05f;
    private float rotateDistancePerFrame = .1f;

    private float zoomAllowance = 4;
    private float baseZoom;
    private float scrollDistancePerFrame = 0.05f;
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
            float z = Camera.main.WorldToScreenPoint(lookAtWorldCoordinates).z;
            Vector3 newPositioninWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, z));

            Vector3 delta = newPositioninWorld - lastPositionInWorld;
            lastPositionInWorld = newPositioninWorld;
            
            float mouseDelta = (lastMousePosition - mousePosition).magnitude;
            Vector3 move = new Vector3(-delta.x, -delta.y, -delta.z) * panDistancePerFrame * mouseDelta;
            lookAtWorldCoordinates += move;
            cam.transform.position += move;
            cam.transform.LookAt(lookAtWorldCoordinates);

            lastMousePosition = mousePosition;
            yield return null;
        }
        isPanning = false;
    }

    public void DoRotate() {
        lastPositionInWorld = mousePosition;
        lastMousePosition = mousePosition;
        StartCoroutine(Rotate());
    }
    private IEnumerator Rotate() {
        isRotating = true;
        
        while(isRotating) {
            float z = Camera.main.WorldToScreenPoint(lookAtWorldCoordinates).z;
            Vector3 newPositioninWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, z));
            newPositioninWorld = newPositioninWorld.normalized * distanceFromLookAtCoordinates;

            Vector3 delta = newPositioninWorld - lastPositionInWorld;
            lastPositionInWorld = newPositioninWorld;
            
            float mouseDelta = (lastMousePosition - mousePosition).magnitude;
            Vector3 move = new Vector3(-delta.x, -delta.y, -delta.z) * rotateDistancePerFrame * mouseDelta;

            cam.transform.position += move;
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
        print(scrollInput);
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