using UnityEngine;
using System.Collections.Generic;

public class SpringBlock : MonoBehaviour
{
    public float returnInBeats = 2f;
    [SerializeField] private GameObject sphere;
    private List<GameObject> spheres = new();
    [SerializeField] bool DEBUG_ShowSpheres = false;

    private static bool IsWithinIntervalRange(float y, float tolerance) {
        float remainder = y % 0.25f;
        if (remainder < 0)
            remainder += 0.25f;
        return (remainder <= tolerance) || (remainder >= (0.25f - tolerance));
    }

    void OnCollisionEnter(Collision other) {
        if (!other.gameObject.CompareTag("marble")) return;

        foreach (GameObject sphere in spheres) {
            Destroy(sphere);
        }
        spheres.Clear();

        var currentVelocity = other.gameObject.GetComponent<PlayerMarble>().GetCurrentVelocity();
        var speed = currentVelocity.magnitude;
        var direction = Vector3.Reflect(currentVelocity.normalized, other.contacts[0].normal);
        float realisticVelocityX = direction.x * Mathf.Max(speed, 0f);

        float deltaY = 0;
        float time = returnInBeats * (float)BeatManager.self.secPerBeat;
        Vector2 perfectVelocity = new(
            realisticVelocityX,
            (deltaY - 0.5f * Physics.gravity.y * time * time) / time);

        other.gameObject.GetComponent<PlayerMarble>().SetVelocity(perfectVelocity);

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
                        if (DEBUG_ShowSpheres) spheres.Add(Instantiate(sphere, new(tryEnd.x, tryEnd.y, 0), Quaternion.identity));
                    }
                }
            }
        }
    }
}
