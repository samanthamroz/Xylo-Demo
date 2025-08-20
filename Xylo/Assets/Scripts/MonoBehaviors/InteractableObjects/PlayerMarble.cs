using UnityEngine;

public class PlayerMarble : InteractableObject
{
    public Vector3 resetPosition;
    public Vector3 currentVelocity;
    void Start()
    {
        resetPosition = transform.position;
        currentVelocity = Vector3.zero;
    }
    public override void DoClick() {
        LevelManager.self.StartAttempt();
    }

    public void RunMarble() {
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().velocity = currentVelocity;
        Debug.Log(GetComponent<Rigidbody>().velocity);
    }
    
    public void ResetSelf() {
        GetComponent<Rigidbody>().isKinematic = true;
        LeanTween.move(gameObject, resetPosition, .5f).setEaseInOutSine();
    }

    public void ResetSelfToCurrentPosition() {
        resetPosition = transform.position;
        ResetSelf();
    }

    public void UpdateSavedVelocity() {
        currentVelocity = GetComponent<Rigidbody>().velocity;
        Debug.Log(currentVelocity);
    }
}
