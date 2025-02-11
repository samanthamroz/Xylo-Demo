using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Springboard : MonoBehaviour
{
    public bool bounceX = true, bounceY = true, bounceZ = true;
    public float springiness = 5f;

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent<Rigidbody>(out var rb)) {
            Debug.Log(rb.velocity.z);
            rb.AddForce(rb.velocity * springiness);
        }
    }
}
