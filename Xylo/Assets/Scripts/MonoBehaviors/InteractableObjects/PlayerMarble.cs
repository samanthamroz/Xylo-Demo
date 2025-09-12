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
            print(currentVelocity);
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
            float T = BeatManager.self.beatsBetweenFirstTwoBeats * (float)BeatManager.self.secPerBeat; // airtime per bounce
            float g = -Physics.gravity.y;   // ~9.81
            float vY = g * T * 0.5f;        // vertical launch speed
            float vX = BeatManager.self.beatsBetweenFirstTwoBeats * BeatManager.self.xDistancePerBeat / T;             // horizontal speed (negative to go left)


            float mass = rb.mass;
            Vector3 impulse = mass * new Vector3(vX, vY, 0f);

            rb.AddForce(impulse, ForceMode.Impulse);
        }
        else {
            rb.velocity = launchVelocity;
        }

        LevelManager.self.StartCountingForAttempt();
    }
}