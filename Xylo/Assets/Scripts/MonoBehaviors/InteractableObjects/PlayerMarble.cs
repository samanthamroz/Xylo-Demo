using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMarble : InteractableObject {
    private Vector3 originalPosition, resetPosition, currentVelocity;
    private Vector3 launchVelocity = VectorUtils.nullVector;
    private Rigidbody rb;
    
    public Vector3 GetCurrentVelocity() { return currentVelocity; }
    public void SetVelocity(Vector3 newVelocity) { 
        rb.velocity = newVelocity;
        currentVelocity = rb.velocity;
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
        resetPosition = transform.position;
        originalPosition = transform.position;
    }
    void FixedUpdate() {
        if (!rb.isKinematic) {
            currentVelocity = rb.velocity;
            //print(currentVelocity);
        }
    }
    
    public override void DoClick() {
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
        BeatManager.self.hasFirstNoteOccurred = false;
    }
    public void PlaceMarbleForSectionStart(Vector3 velocity, Vector3 position) {
        launchVelocity = velocity;
        currentVelocity = launchVelocity;

        resetPosition = position;

        ResetSelf();
    }

    public void RunMarble() {
        rb.isKinematic = false;
        
        if (VectorUtils.IsNullVector(launchVelocity)) {
            // For launching the ball initially to hit the first platform
            float timeToFirstPlatform = 1.5f * (float)BeatManager.self.secPerBeat; // Your current timing
            float timeBetweenPlatforms = 0.5f * (float)BeatManager.self.secPerBeat; // Target timing

            // Distance calculations
            float horizontalDistanceToFirst = 1;
            float horizontalDistanceBetween = 1;
            float verticalDistanceToFirst = 0;

            // Calculate initial velocity
            float vX = horizontalDistanceToFirst / timeBetweenPlatforms;

            // For the vertical component, account for the bounce velocity loss
            // After bouncing with 0.85 bounciness, you need the right velocity for 0.5 beat flight
            float requiredVyAfterBounce = -0.5f * Physics.gravity.y * timeBetweenPlatforms;
            float requiredVyBeforeBounce = requiredVyAfterBounce / 0.85f; // Compensate for bounce loss

            // Calculate initial vY to achieve the required velocity at first platform
            // Using: vY_final = vY_initial + gravity * time
            float vY = requiredVyBeforeBounce - (Physics.gravity.y * timeBetweenPlatforms);

            Vector2 launchVelocity = new(vX, requiredVyBeforeBounce);
            SetVelocity(launchVelocity);
        }
        else {
            rb.velocity = launchVelocity;
        }

        LevelManager.self.StartCountingForAttempt();
    }
}