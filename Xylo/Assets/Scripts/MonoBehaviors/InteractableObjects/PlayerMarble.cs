
using UnityEngine;

public class PlayerMarble : InteractableObject
{
    public Vector3 resetPosition;
    public Vector3 currentVelocity;
    private bool jumpQueued = false;
    void Start()
    {
        resetPosition = transform.position;
    }
    void Update()
    {
        currentVelocity = GetComponent<Rigidbody>().velocity;
        if (jumpQueued) {
            JumpToNextPlatform();
        }
    }
    
    public override void DoClick() {
        LevelManager.self.StartPlaying();
    }
    public void RunMarble() {
        GetComponent<Rigidbody>().isKinematic = false;
    }
    
    public void ResetSelf() {
        GetComponent<Rigidbody>().isKinematic = true;
        LeanTween.move(gameObject, resetPosition, .5f).setEaseInOutSine();
    }

    public void ResetSelfToCurrentPosition() {
        resetPosition = transform.position;
        ResetSelf();
    }

    void JumpToNextPlatform() {
        float T = LevelManager.self.GetSecToNextBeat();

        Vector3 start = transform.position;
        Vector3 end = new (transform.position.x + 2, transform.position.y - .5f, transform.position.z);

        float vx = (end.x - start.x) / T;
        float vy = (end.y - start.y + 0.5f * -Physics.gravity.y * T * T) / T;

        // apply velocity
        GetComponent<Rigidbody>().velocity = new Vector3(vx, vy, 0);
        jumpQueued = false;
    }

    void OnCollisionEnter(Collision col) {
        jumpQueued = true;
    }
}
