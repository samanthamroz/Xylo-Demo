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

    public void ResetSelf(Vector3 newResetPosition) {
        GetComponent<Rigidbody>().isKinematic = true;
        LeanTween.move(gameObject, newResetPosition, .5f).setEaseInOutSine();
    }
}
