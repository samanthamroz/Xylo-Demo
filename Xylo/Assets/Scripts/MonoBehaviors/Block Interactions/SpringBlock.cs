using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SpringBlock : MonoBehaviour
{
    public float returnInBeats = 2f;
    [SerializeField] private GameObject sphere;
    private List<GameObject> spheres = new();
    [SerializeField] bool DEBUG_ShowSpheres = false;

    void Awake()
    {
        if (gameObject.TryGetComponent<BounceBlock>(out BounceBlock b)) throw new System.Exception("SpringBlock not compatible with BounceBlock. Remove one to avoid physics issues.");
    }

    private static bool IsWithinIntervalRange(float y, float tolerance) {
        float remainder = y % 0.25f;
        if (remainder < 0)
            remainder += 0.25f;
        return (remainder <= tolerance) || (remainder >= (0.25f - tolerance));
    }

    void OnCollisionEnter(Collision other) {
        if (!other.gameObject.CompareTag("Marble")) return;

        var currentVelocity = other.gameObject.GetComponent<PlayerMarble>().GetCurrentVelocity();
        var speed = currentVelocity.magnitude;
        var direction = Vector3.Reflect(currentVelocity.normalized, -other.contacts[0].normal);
        
        // Keep horizontal velocity (x and z) from realistic bounce
        Vector3 realisticHorizontalVelocity = new Vector3(
            direction.x * Mathf.Max(speed, 0f),
            0f,
            direction.z * Mathf.Max(speed, 0f)
        );

        float deltaY = 0;
        float time = returnInBeats * (float)BeatManager.self.secPerBeat;
        
        Vector3 perfectVelocity = new Vector3(
            realisticHorizontalVelocity.x,
            (deltaY - 0.5f * Physics.gravity.y * time * time) / time,
            realisticHorizontalVelocity.z
        );

        other.gameObject.GetComponent<PlayerMarble>().SetVelocity(perfectVelocity);

        // Calculate velocity needed for a 16th-note bounce
        float T = .25f * (float)BeatManager.self.secPerBeat;
        float vYNeededForFourthBeat = -Physics.gravity.y * T;

        Vector3 start = other.transform.position;
        float maxSearchTime = 5.0f;
        float tApex = perfectVelocity.y / -Physics.gravity.y;

        float dt = 0.05f;
        for (float t = tApex; t < maxSearchTime; t += dt) {
            float testX = start.x + perfectVelocity.x * t;
            float testY = start.y + (perfectVelocity.y * t) + (0.5f * Physics.gravity.y * t * t);
            float testZ = start.z + perfectVelocity.z * t;
            
            if (IsWithinIntervalRange(testY, 0.2f)) {
                float testYVelocity = perfectVelocity.y + (Physics.gravity.y * t);
                
                float multiple = Mathf.Abs(testYVelocity) / vYNeededForFourthBeat;
                float nearestInteger = Mathf.Round(multiple);
                float velocityTolerance = 0.2f;
                
                if (Mathf.Abs(multiple - nearestInteger) <= velocityTolerance && 
                    nearestInteger >= 1 && nearestInteger <= 4) {
                    
                    float beatPosition = t / (float)BeatManager.self.secPerBeat;
                    float fractionalBeat = beatPosition - Mathf.Floor(beatPosition);
                    float nearestSixteenth = Mathf.Round(fractionalBeat * 4) / 4f;
                    float beatTolerance = 0.2f;
                    
                    if (Mathf.Abs(fractionalBeat - nearestSixteenth) <= beatTolerance && IsWithinIntervalRange(testY, .2f)) {
                        Vector3 tryEnd = new Vector3(testX, testY, testZ);
                        if (DEBUG_ShowSpheres) spheres.Add(Instantiate(sphere, tryEnd, Quaternion.identity));
                    }
                }
            }
        }

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
