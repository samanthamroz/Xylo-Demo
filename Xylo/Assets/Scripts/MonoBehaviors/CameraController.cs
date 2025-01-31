using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;
    public int speed = 20;
    Vector3 direction = Vector3.zero;
    public Transform zero;
    public float zoomAllowance = 3;
    private float baseZoom = 9.5f; 

    void Awake()
    {   
        cam = Camera.main;
        cam.orthographicSize = baseZoom;
        cam.transform.LookAt(zero);
    }
    void Update() {
        cam.transform.RotateAround(zero.position, direction, speed * Time.deltaTime);
        cam.transform.LookAt(zero);
    }
    public void LeftPan(){
        direction = Vector3.up;
    }
    public void RightPan(){
        direction = Vector3.down;
    }
    public void UpPan(){
        Debug.Log(cam.transform.rotation.eulerAngles.y);
        float xaxis = 90 / cam.transform.rotation.eulerAngles.y;
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
