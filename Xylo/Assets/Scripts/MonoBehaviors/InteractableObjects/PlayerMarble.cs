using UnityEngine;

public class PlayerMarble : InteractableObject
{
    public Vector3 resetPosition;
    void Start()
    {
        resetPosition = transform.position;
    }
    public override void DoClick() {
        GetComponent<Rigidbody>().isKinematic = false;
        ControlsManager.self.EnterCinematicMode();
        CameraManager.self.EnterCinematicMode(gameObject);
    }
    
    void OnCollisionEnter(Collision other)
    {
        //GetComponent<AudioSource>().Play();
    }
}
