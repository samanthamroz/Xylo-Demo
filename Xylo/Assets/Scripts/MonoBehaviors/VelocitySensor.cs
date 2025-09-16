using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocitySensor : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        other.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb);
        if (rb is null) return;
        print(rb.velocity);
        print("pos:" + other.transform.position);
    }
}
