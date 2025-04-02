using UnityEngine;
using UnityEngine.InputSystem;



public class CameraController : MonoBehaviour
{
    private Camera cam;
    public int speed = 20;
    Vector3 direction = Vector3.zero;
    public float zoomAllowance = 3;
    private float baseZoom = 9.5f; 
    private float scrollInput = 0f;
    public float scrollSpeed = 0.1f;
    private int verticalDirection = 0;

    void Awake()
    {   
        cam = Camera.main;
    }
    void Start()
    {
        cam.orthographicSize = baseZoom;
        cam.transform.LookAt(Vector3.zero);
    }
    void Update() {
        if ((cam.transform.position.y >= 29.999f && verticalDirection == 1) || (cam.transform.position.y <= -29.999f && verticalDirection == -1)) {
            direction = Vector3.zero;
        }
        cam.transform.RotateAround(Vector3.zero, direction, speed * Time.deltaTime);
        cam.transform.LookAt(Vector3.zero);

        //Zoom Camera
        if ((scrollInput < 0 && !(cam.orthographicSize <= baseZoom - zoomAllowance))
                || (scrollInput > 0 && !(cam.orthographicSize >= baseZoom + zoomAllowance))) {
            cam.orthographicSize += scrollInput * scrollSpeed * Time.deltaTime;
        }
    }
    void OnScroll(InputValue value) {
        scrollInput = value.Get<float>();
    }
    public void LeftPan() {
        direction = Vector3.up;
    }
    public void RightPan() {
        direction = Vector3.down;
    }
    public void UpPan() {
        verticalDirection = 1;
        Vector3 axis = Quaternion.Euler(0f, cam.transform.rotation.eulerAngles.y, 0f) * Vector3.right;
        direction = axis;
    }
    public void DownPan() {
        verticalDirection = -1;
        Vector3 axis = Quaternion.Euler(0f, cam.transform.rotation.eulerAngles.y, 0f) * Vector3.right;
        direction = -axis;
    }
    public void StopPan() {
        verticalDirection = 0;
        direction = Vector3.zero;
    }
}
