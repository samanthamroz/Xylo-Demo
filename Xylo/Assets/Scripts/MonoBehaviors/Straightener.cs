using UnityEngine;

public class Straightener : MonoBehaviour
{
    public Vector3 straightenTo;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Rigidbody>(out var rb)) {
            //Debug.Log("velocity at enter: " + rb.velocity);
            Vector3 newVelocity = new Vector3(rb.velocity.x * straightenTo.x, rb.velocity.y * straightenTo.y, rb.velocity.z * straightenTo.z).normalized;
            newVelocity *= rb.velocity.magnitude;
            rb.velocity = newVelocity;
            //Debug.Log("new velocity: " + rb.velocity);
        }
    }
}