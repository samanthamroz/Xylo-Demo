using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    void OnCollisionEnter(Collision other)
    {
        WinManager.self.EndAttempt();
    }
}
