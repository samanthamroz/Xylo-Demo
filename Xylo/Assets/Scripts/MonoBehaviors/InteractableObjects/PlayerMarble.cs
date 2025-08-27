
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMarble : InteractableObject {
    public Vector3 resetPosition;
    public Vector3 currentVelocity;
    public GameObject sphere;
    public List<GameObject> spheres = new();
    void Start() {
        resetPosition = transform.position;
    }
    void FixedUpdate() {
        currentVelocity = GetComponent<Rigidbody>().velocity;
    }

    public override void DoClick() {
        LevelManager.self.StartPlaying();
    }

    public void ResetSelf() {
        GetComponent<Rigidbody>().isKinematic = true;
        LeanTween.move(gameObject, resetPosition, .5f).setEaseInOutSine();
    }
    public void ResetSelfToCurrentPosition() {
        resetPosition = transform.position;
        ResetSelf();
    }

    public void RunMarble() {
        GetComponent<Rigidbody>().isKinematic = false;
        float T = .75f;                // airtime per bounce
        float g = -Physics.gravity.y;   // ~9.81
        float vY = g * T * 0.5f;        // vertical launch speed
        float vX = 1.9f / T;             // horizontal speed (negative to go left)

        Rigidbody rb = GetComponent<Rigidbody>();
        float mass = rb.mass;
        Vector3 impulse = mass * new Vector3(vX, vY, 0f);

        rb.AddForce(impulse, ForceMode.Impulse);

        //begin attempt
        LevelManager.self.StartCountingForAttempt();
    }
    public static bool IsWithinIntervalRange(float y, float tolerance) {
        // Find remainder when divided by 0.5 (since intervals are every 0.5 units)
        float remainder = y % 0.5f;

        // Handle negative numbers properly
        if (remainder < 0)
            remainder += 0.5f;

        // Check if we're within tolerance of either end of the 0.5 interval
        // remainder near 0 means we're close to a 0.5 boundary (0, 0.5, 1.0, 1.5, etc.)
        // remainder near 0.5 means we're close to the next 0.5 boundary
        return (remainder <= tolerance) || (remainder >= (0.5f - tolerance));
    }
    private bool isFirstNote = true;
    void OnCollisionEnter(Collision collision) {
        if (!collision.gameObject.CompareTag("Bounces")) return;

        if (isFirstNote) {
            BeatManager.self.StartAttempt();
            isFirstNote = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        foreach (GameObject sphere in spheres) {
            Destroy(sphere);
        }
        spheres.Clear();

        //Get "realistic" velocity
        var speed = currentVelocity.magnitude;
        var direction = Vector3.Reflect(currentVelocity.normalized, collision.contacts[0].normal);
        float bounciness = .85f;

        Vector2 realisticVelocity = new(direction.x * Mathf.Max(speed, 0f), direction.y * Mathf.Max(speed * bounciness, 0f));

        //Find "perfect" velocity - but only look for realistic landing points
        Vector3 start = transform.position;
        float tPerfect = 0;
        Vector3 end = Vector3.zero;
        bool adjust = false;

        float maxSearchTime = 5.0f; // Maximum 5 seconds ahead
        float maxSearchBeats = maxSearchTime / (float)BeatManager.self.secPerBeat;
        int maxSearchDistance = Mathf.Min(20, Mathf.RoundToInt(Mathf.Abs(realisticVelocity.x) * maxSearchTime));

        float tApex = realisticVelocity.y / -Physics.gravity.y;
        float xApex = start.x + realisticVelocity.x * tApex;
        float yApex = start.y + (realisticVelocity.y * realisticVelocity.y) / (2f * -Physics.gravity.y);

        for (int i = 2; i < maxSearchBeats * 4; i++) { //searches on the 16th note
            //how long to the apex plus how many beats out from there
            float t = tApex + (i / 4f) * (float)BeatManager.self.secPerBeat;

            //given velocity and x value, what y do we get?
            float testX = start.x + realisticVelocity.x * t;
            float testY = start.y + (realisticVelocity.y * t) + (0.5f * Physics.gravity.y * t * t);

            Vector2 tryEnd = new(testX, testY);  // This is absolute position
            spheres.Add(Instantiate(sphere, new(tryEnd.x, tryEnd.y, -10), Quaternion.identity));
            if (i % 4 == 0) {
                spheres[^1].transform.localScale = new(.5f, .5f, .5f);
            }

            //if y is within range, that point is valid
            //when a point is found, we keep searching to draw the curve
            //but don't save any further points, only use the closest
            if (!adjust && IsWithinIntervalRange(tryEnd.y, 0.25f)) {
                float roundedY = (float)(Math.Round(tryEnd.y * 2f) / 2f);
                end = new(tryEnd.x, roundedY, transform.position.z);  // Use absolute coordinates
                tPerfect = t;
                adjust = true;
                spheres.Add(Instantiate(sphere, new(end.x, end.y, -10), Quaternion.identity));
                spheres[^1].transform.localScale = Vector3.one;
            }
        }

        if (!adjust) {
            print("No valid points found, using realistic velocity");
            rb.velocity = (Vector3)realisticVelocity;
            return;
        }

        //Calculate the perfect velocity to get to the new end point
        float deltaX = end.x - start.x;
        float deltaY = end.y - start.y;
        //tPerfect = amount of time between now and perfect spot
        //adjust so that it is tPerfect + an offset so that it ends on a solid beat
        int perfectBeat = (int)Mathf.Round(BeatManager.self.GetBeatInNSeconds(deltaX / realisticVelocity.x));
        //float time = BeatManager.self.GetSecsToBeat(perfectBeat);
        float time = (float)BeatManager.self.secPerBeat;

        Vector2 perfectVelocity = new(
            realisticVelocity.x,
            (deltaY - 0.5f * Physics.gravity.y * time * time) / time);

        // Add velocity limits to prevent extreme adjustments
        float maxVelocityChange = 3.0f;  // Maximum change in velocity magnitude
        float realisticSpeed = realisticVelocity.magnitude;
        float perfectSpeed = perfectVelocity.magnitude;

        //Second check - cannot change more than _ in magnitude
        if (perfectSpeed > realisticSpeed + maxVelocityChange) {
            perfectVelocity = perfectVelocity.normalized * (realisticSpeed + maxVelocityChange);
        }
        else if (perfectSpeed < realisticSpeed - maxVelocityChange) {
            perfectVelocity = perfectVelocity.normalized * Mathf.Max(0.1f, realisticSpeed - maxVelocityChange);
        }

        //print($"Realistic: {realisticVelocity} | Perfect: {perfectVelocity} | Delta: {new Vector2(deltaX, deltaY)} | Time: {time}");

        rb.velocity = (Vector3)perfectVelocity;

        print($"Measure {BeatManager.self.currentMeasure} | Beat {BeatManager.self.currentBeat} | Offset {(float)BeatManager.self.GetCurrentBeatOffset()}");
    }
}