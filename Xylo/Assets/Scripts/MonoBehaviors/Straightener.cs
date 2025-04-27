using UnityEngine;

public class Straightener : MonoBehaviour
{
    public Vector3 straightenTo;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Rigidbody>(out var rb)) {
            if (rb.isKinematic) {
                return;
            }

            //Debug.Log("velocity at enter: " + rb.velocity + " magnitude: " + rb.velocity.magnitude);
            Vector3 maskedVelocity = new Vector3(
                rb.velocity.x * straightenTo.x, 
                rb.velocity.y * straightenTo.y, 
                rb.velocity.z * straightenTo.z);

            Vector3 newVelocity = maskedVelocity.normalized * rb.velocity.magnitude;
            rb.velocity = newVelocity;
            
            
            //Debug.Log("new velocity: " + rb.velocity);
        }
    }
}