using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BounceBlock : MonoBehaviour
{
    [SerializeField] private GameObject sphere;
    private List<GameObject> spheres = new();
    [SerializeField] bool DEBUG_ShowSpheres = false;

    void Awake()
    {
        if (gameObject.TryGetComponent<SpringBlock>(out SpringBlock s)) throw new System.Exception("BounceBlock not compatible with SpringBlock. Remove one to avoid physics issues.");
    }

    private static bool IsWithinIntervalRange(float y, float tolerance) {
        float remainder = y % 0.25f;
        if (remainder < 0)
            remainder += 0.25f;
        return (remainder <= tolerance) || (remainder >= (0.25f - tolerance));
    }

    private void OnCollisionEnter(Collision other) {
        if (!other.gameObject.CompareTag("Marble")) {
            return;
        }

        //Get "realistic" velocity
        var currentVelocity = other.gameObject.GetComponent<PlayerMarble>().GetCurrentVelocity();
        var speed = currentVelocity.magnitude;
        var direction = Vector3.Reflect(currentVelocity.normalized, -other.contacts[0].normal);
        float bounciness = .85f;
        Vector2 realisticVelocity = new(direction.x * Mathf.Max(speed, 0f), direction.y * Mathf.Max(speed * bounciness, 0f));

        //Find "perfect" velocity - but only look for realistic landing points
        Vector3 start =  other.transform.position;
        float tPerfect = 0;
        Vector3 end = Vector3.zero;
        bool adjust = false;

        float maxSearchTime = 5.0f; // Maximum 5 seconds ahead
        float maxSearchBeats = maxSearchTime / (float)BeatManager.self.secPerBeat;
        float tApex = realisticVelocity.y / -Physics.gravity.y;

        for (int i = 2; i < maxSearchBeats * 2; i++) {
            float t = i / 4f * (float)BeatManager.self.secPerBeat;
            if (t < tApex) continue;

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
                float roundedY = (float)(Mathf.Round(tryEnd.y * 4f) / 4f);
                end = new(tryEnd.x, roundedY, other.transform.position.z);  // Use absolute coordinates
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
            other.gameObject.GetComponent<PlayerMarble>().SetVelocity(realisticVelocity);
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
        
        other.gameObject.GetComponent<PlayerMarble>().SetVelocity(perfectVelocity);
        StartCoroutine(ClearSpheres());
    }

    private IEnumerator ClearSpheres() {
        yield return new WaitForSeconds(1);
        foreach (GameObject sphere in spheres) {
            Destroy(sphere);
        }
        spheres.Clear();
    }
}