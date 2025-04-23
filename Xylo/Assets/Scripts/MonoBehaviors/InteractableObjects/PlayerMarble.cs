using UnityEngine;

public class PlayerMarble : InteractableObject
{
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
