using UnityEngine;

public class PlayerMarble : MonoBehaviour
{
    void OnMouseDown() {
        GetComponent<Rigidbody>().isKinematic = false;
        ControlsManager.self.EnterCinematicMode();
        CameraManager.self.EnterCinematicMode(gameObject);
    }
    
    void OnCollisionEnter(Collision other)
    {
        //GetComponent<AudioSource>().Play();
    }
}
