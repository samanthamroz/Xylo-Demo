using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager self;
	public GameObject cameraPrefab, cameraObject;
	private Camera cam;

    public bool isRotating;
    public bool isPanning;

    public float speed = .5f;
    Vector3 direction = Vector3.zero;

    public float zoomAllowance = 3;
    private float baseZoom = 9.5f;
    public float scrollDistancePerFrame = 0.01f;
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
        cam.orthographicSize = baseZoom;
        scrollGoal = cam.orthographicSize;
        cam.transform.position = new Vector3(20, 0, -20);
        cam.transform.LookAt(Vector3.zero);
	}

    public void DoPan() {
        StartCoroutine(Pan());
    }

    private IEnumerator Pan() {
        isPanning = true;
        while(isPanning) {
            yield return null;
        }
    }

    public void DoRotate() {

    }
    private IEnumerator Rotate() {
        isPanning = true;
        while(isRotating) {
            yield return null;
        }
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