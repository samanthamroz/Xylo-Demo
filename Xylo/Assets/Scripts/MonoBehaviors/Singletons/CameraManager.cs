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

    private Vector3 lastMousePosition;

    private float panDistancePerFrame = 0.05f;

    private Vector3 lookAtWorldCoordinates = Vector3.zero;
    private float rotateDistancePerFrame = .1f;
    private Vector3 rotationDirection = Vector3.zero;

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
	}

    public void DoPan() {
        lastMousePosition = mousePosition;
        StartCoroutine(Pan());
    }

    private IEnumerator Pan() {
        isPanning = true;
        while(isPanning) {
            Vector3 delta = mousePosition - lastMousePosition;

            // Convert screen movement to world movement
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * panDistancePerFrame;

            lookAtWorldCoordinates += move;
            cam.transform.position += move;
            cam.transform.LookAt(lookAtWorldCoordinates);

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
        //CHANGE ME!!!!
        while(isRotating) {
            Vector3 delta = mousePosition - lastMousePosition;

            // Convert screen movement to world movement
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * rotateDistancePerFrame;
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