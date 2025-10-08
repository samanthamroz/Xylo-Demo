using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMarble : MonoBehaviour, IClickBehavior {
    private Vector3 originalPosition, resetPosition, currentVelocity;
    private Vector3 launchVelocity = VectorUtils.nullVector;
    private Rigidbody rb;
    
    public Vector3 GetCurrentVelocity() { return currentVelocity; }
    public void SetVelocity(Vector3 newVelocity) { 
        if (rb.isKinematic) throw new Exception("Error: Attempted to set velocity while marble is kinematic");

        rb.velocity = newVelocity;
        currentVelocity = rb.velocity;
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
        resetPosition = transform.position;
        originalPosition = transform.position;
        if (LevelManager.self.DEBUG_UseManualStart) currentVelocity = LevelManager.self.DEBUG_ManualVelocity;
    }
    void FixedUpdate() {
        if (!rb.isKinematic) {
            currentVelocity = rb.velocity;
            //print(currentVelocity);
        }
    }
    
    public void DoClick() {
        LevelManager.self.StartPlaying();
    }

    public void RunMarbleFromBeginning() {
        rb.isKinematic = true;
        transform.position = originalPosition;
        launchVelocity = VectorUtils.nullVector;
        
        RunMarble();
    }

    public void ResetSelf() {
        rb.isKinematic = true;
        LeanTween.move(gameObject, resetPosition, .5f).setEaseInOutSine();
        BeatManager.self.SetFirstNoteOccurred(false);
    }
    public void PlaceMarbleForSectionStart(Vector3 velocity, Vector3 position) {
        launchVelocity = velocity;
        currentVelocity = launchVelocity;

        resetPosition = position;

        ResetSelf();
    }

    public void RunMarble() {
        rb.isKinematic = false;
        
        if (LevelManager.self.DEBUG_UseManualStart) {
            launchVelocity = LevelManager.self.DEBUG_ManualVelocity;
        }

        if (VectorUtils.IsNullVector(launchVelocity)) {
            float timeBetweenPlatforms = BeatManager.self.beatsBetweenFirstTwoBeats * (float)BeatManager.self.secPerBeat; // Target timing
            float vX = BeatManager.self.xDistancePerBeat / (float)BeatManager.self.secPerBeat * LevelManager.self.directionMoving.x;

            float requiredVyAfterBounce = -0.5f * Physics.gravity.y * timeBetweenPlatforms;
            float requiredVyBeforeBounce = requiredVyAfterBounce / 0.85f; // Compensate for bounce loss

            Vector2 launchVelocity = new(vX, requiredVyBeforeBounce);
            SetVelocity(launchVelocity);
        }
        else {
            rb.velocity = launchVelocity;
        }

        LevelManager.self.StartCountingForAttempt();
    }
}