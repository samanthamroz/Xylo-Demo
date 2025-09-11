
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMarble : InteractableObject {
    private Vector3 originalPosition, resetPosition, currentVelocity;
    private Vector3 launchVelocity = VectorUtils.nullVector;
    private Rigidbody rb;
    [SerializeField] private GameObject sphere;
    private List<GameObject> spheres = new();
    [SerializeField] bool DEBUG_ShowSpheres = false;
    
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
        isFirstNote = true;
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
    private static bool IsWithinIntervalRange(float y, float tolerance) {
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
        if (collision.gameObject.CompareTag("Untagged")) return;

        if (collision.gameObject.CompareTag("Bounces")) {
            DoBounceCollision(collision);  
        } else if (collision.gameObject.CompareTag("Springs")) {
            DoSpringCollision(collision);
        }
    }

    private void DoBounceCollision(Collision collision) {
        if (isFirstNote) {
            BeatManager.self.StartAttempt();
            isFirstNote = false;
        }

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
        float tApex = realisticVelocity.y / -Physics.gravity.y;

        for (int i = BeatManager.self.smallestBeatToCheck; i < maxSearchBeats * 2; i++) {
            float t = i / 2f * (float)BeatManager.self.secPerBeat;
            if (t <= tApex) continue;

            //given velocity and x value, what y do we get?
            float testX = start.x + realisticVelocity.x * t;
            float testY = start.y + (realisticVelocity.y * t) + (0.5f * Physics.gravity.y * t * t);

            Vector2 tryEnd = new(testX, testY);
            if (DEBUG_ShowSpheres) {
                spheres.Add(Instantiate(sphere, new(tryEnd.x, tryEnd.y, -10), Quaternion.identity));
                if (i % 4 == 0) {
                    spheres[^1].transform.localScale = new(.5f, .5f, .5f);
                }
            }

            //if y is within range, that point is valid
            //when a point is found, we keep searching to draw the curve
            //but don't save any further points, only use the closest
            if (!adjust && IsWithinIntervalRange(tryEnd.y, 0.25f)) {
                float roundedY = (float)(Math.Round(tryEnd.y * 2f) / 2f);
                end = new(tryEnd.x, roundedY, transform.position.z);  // Use absolute coordinates
                tPerfect = t;
                adjust = true;
                if (DEBUG_ShowSpheres) {
                    spheres.Add(Instantiate(sphere, new(end.x, end.y, -10), Quaternion.identity));
                    spheres[^1].transform.localScale = Vector3.one;
                }
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
        float time = tPerfect;

        Vector2 perfectVelocity = new(
            deltaX / time,
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

        rb.velocity = (Vector3)perfectVelocity;
    }

    private void DoSpringCollision(Collision collision) {
        if (!collision.gameObject.TryGetComponent<Springboard>(out Springboard sb)) return;

        foreach (GameObject sphere in spheres) {
            Destroy(sphere);
        }
        spheres.Clear();

        var speed = currentVelocity.magnitude;
        var direction = Vector3.Reflect(currentVelocity.normalized, collision.contacts[0].normal);
        float realisticVelocityX = direction.x * Mathf.Max(speed, 0f);

        float deltaY = 0;
        float time = sb.returnInBeats * (float)BeatManager.self.secPerBeat;
        Vector2 perfectVelocity = new(
            realisticVelocityX,
            (deltaY - 0.5f * Physics.gravity.y * time * time) / time);

        rb.velocity = perfectVelocity;

        //Calculate velocity needed for a 16th-note bounce
        float T = .25f * (float)BeatManager.self.secPerBeat;
        float vYNeededForFourthBeat = -Physics.gravity.y * T;

        Vector3 start = transform.position;
        float maxSearchTime = 5.0f;
        float maxSearchBeats = maxSearchTime / (float)BeatManager.self.secPerBeat;
        float tApex = perfectVelocity.y / -Physics.gravity.y;

        //Searches each 16th note after the apex for a landing point that matches the velocity needed for the next bounce
        float dt = 0.05f; // Small time step
        for (float t = tApex; t < maxSearchTime; t += dt) {
            float testY = start.y + (perfectVelocity.y * t) + (0.5f * Physics.gravity.y * t * t);
            
            // Check if ball is crossing through your landing zone
            if (IsWithinIntervalRange(testY, 0.2f)) {
                float testYVelocity = perfectVelocity.y + (Physics.gravity.y * t);
                
                // Check if velocity is close to an integer multiple of the base velocity
                float multiple = Mathf.Abs(testYVelocity) / vYNeededForFourthBeat;
                float nearestInteger = Mathf.Round(multiple);
                float velocityTolerance = 0.2f;
                
                if (Mathf.Abs(multiple - nearestInteger) <= velocityTolerance && 
                    nearestInteger >= 1 && nearestInteger <= 4) {
                    
                    // Check if this landing time aligns with a beat subdivision
                    float beatPosition = t / (float)BeatManager.self.secPerBeat;
                    float fractionalBeat = beatPosition - Mathf.Floor(beatPosition);
                    float nearestSixteenth = Mathf.Round(fractionalBeat * 4) / 4f;
                    float beatTolerance = 0.2f; // Tolerance for beat timing
                    
                    if (Mathf.Abs(fractionalBeat - nearestSixteenth) <= beatTolerance && IsWithinIntervalRange(testY, .2f)) {
                        float testX = start.x + perfectVelocity.x * t;
                        Vector2 tryEnd = new(testX, testY);
                        spheres.Add(Instantiate(sphere, new(tryEnd.x, tryEnd.y, 0), Quaternion.identity));
                    }
                }
            }
        }
    }
}