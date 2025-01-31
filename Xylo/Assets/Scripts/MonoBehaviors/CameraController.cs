using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private Camera cam;
    public int speed = 20;
    Vector3 direction = Vector3.zero;
    public Transform zero;
    public float zoomAllowance = 3;
    private float baseZoom = 9.5f; 
    private float scrollInput = 0f;
    public float scrollSpeed = 0.1f;

    void Awake()
    {   
        cam = Camera.main;
        cam.orthographicSize = baseZoom;
        cam.transform.LookAt(zero);
    }
    void Update() {
        cam.transform.RotateAround(zero.position, direction, speed * Time.deltaTime);
        cam.transform.LookAt(zero);
        if ((scrollInput < 0 && !(cam.orthographicSize <= baseZoom - zoomAllowance))
                || (scrollInput > 0 && !(cam.orthographicSize >= baseZoom + zoomAllowance))) {
            cam.orthographicSize += scrollInput * scrollSpeed * Time.deltaTime;
        }
    }
    void OnScroll(InputValue value) {
        scrollInput = value.Get<float>();
        Debug.Log("scroll me");
    }
    public void LeftPan(){
        direction = Vector3.up;
    }
    public void RightPan(){
        direction = Vector3.down;
    }
    public void UpPan(){
        Debug.Log(cam.transform.rotation.eulerAngles.y);
        

        float angle = 0.0f;
        Vector3 axis = Vector3.zero;
        transform.rotation.ToAngleAxis(out angle, out axis);

        float xaxis = 360 / cam.transform.rotation.eulerAngles.y;
        Debug.Log(new Vector3(xaxis, 0, xaxis));
        direction = new Vector3(xaxis, 0, xaxis);
    }
    public void DownPan(){
        float axis = 90 / cam.transform.rotation.eulerAngles.y;
        direction = new Vector3(-axis, 0, -axis);
    }
    public void StopPan() {
        direction = Vector3.zero;
    }
}
