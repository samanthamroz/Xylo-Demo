using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;

public class BounceBlock : MonoBehaviour
{
    [SerializeField] private GameObject sphere;
    private List<GameObject> spheres = new();
    [SerializeField] bool DEBUG_ShowSpheres = false;
    [SerializeField] bool DEBUG_DoAdjustments = true;

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

        if (!DEBUG_DoAdjustments) {
            print($"Warning, {gameObject.name} has DEBUG_DoAdjustments = false");
            return;
        }

        // Get "realistic" velocity
        var currentVelocity = other.gameObject.GetComponent<PlayerMarble>().GetCurrentVelocity();
        var speed = currentVelocity.magnitude;
        var direction = Vector3.Reflect(currentVelocity.normalized, -other.contacts[0].normal);
        float bounciness = .85f;
        
        // Use full 3D velocity instead of Vector2
        Vector3 realisticVelocity = new Vector3(
            direction.x * Mathf.Max(speed, 0f),
            direction.y * Mathf.Max(speed * bounciness, 0f),
            direction.z * Mathf.Max(speed, 0f)
        );

        // Find "perfect" velocity
        Vector3 start = other.transform.position;
        float tPerfect = 0;
        Vector3 end = Vector3.zero;
        bool adjust = false;

        float maxSearchTime = 5.0f;
        float maxSearchBeats = maxSearchTime / (float)BeatManager.self.secPerBeat;
        float tApex = realisticVelocity.y / -Physics.gravity.y;

        for (int i = 2; i < maxSearchBeats * 2; i++) {
            float t = i / 4f * (float)BeatManager.self.secPerBeat;
            if (t < tApex) continue;

            // Calculate position in 3D
            float testX = start.x + realisticVelocity.x * t;
            float testY = start.y + (realisticVelocity.y * t) + (0.5f * Physics.gravity.y * t * t);
            float testZ = start.z + realisticVelocity.z * t;

            Vector3 tryEnd = new Vector3(testX, testY, testZ);
            
            if (DEBUG_ShowSpheres) {
                spheres.Add(Instantiate(sphere, tryEnd, Quaternion.identity));
                if (i % 4 == 0) {
                    spheres[^1].transform.localScale = new(.5f, .5f, .5f);
                }
            }

            if (!adjust && IsWithinIntervalRange(tryEnd.y, 0.25f)) {
                float roundedY = (float)(Mathf.Round(tryEnd.y * 4f) / 4f);
                end = new Vector3(tryEnd.x, roundedY, tryEnd.z);
                tPerfect = t;
                adjust = true;
                if (DEBUG_ShowSpheres) {
                    spheres.Add(Instantiate(sphere, end, Quaternion.identity));
                    spheres[^1].transform.localScale = Vector3.one;
                }
            }
        }

        if (!adjust) {
            print("No valid points found, using realistic velocity");
            other.gameObject.GetComponent<PlayerMarble>().SetVelocity(realisticVelocity);
            return;
        }

        // Calculate perfect velocity in 3D
        float deltaX = end.x - start.x;
        float deltaY = end.y - start.y;
        float deltaZ = end.z - start.z;
        float time = tPerfect;

        Vector3 perfectVelocity = new Vector3(
            deltaX / time,
            (deltaY - 0.5f * Physics.gravity.y * time * time) / time,
            deltaZ / time
        );

        // Velocity limits
        float maxVelocityChange = 3.0f;
        float realisticSpeed = realisticVelocity.magnitude;
        float perfectSpeed = perfectVelocity.magnitude;

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