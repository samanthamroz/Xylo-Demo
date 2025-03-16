using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMarble : MonoBehaviour
{
    void FixedUpdate()
    {
        //Debug.Log(GetComponent<Rigidbody>().velocity);
    }

    void OnMouseDown() {
        GetComponent<Rigidbody>().isKinematic = false;
        WinManager.self.TriggerNewAttempt();
    }
    
    void OnCollisionEnter(Collision other)
    {
        GetComponent<AudioSource>().Play();
    }
}
