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
        LevelManager.self.StartAttempt();
    }
    
    public void ResetSelf() {
        GetComponent<Rigidbody>().isKinematic = true;
        LeanTween.move(gameObject, resetPosition, .5f).setEaseInOutSine();
    }
    
    void OnCollisionEnter(Collision other)
    {
        //GetComponent<AudioSource>().Play();
    }
}
