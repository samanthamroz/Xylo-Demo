using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestForWin : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        LevelManager.self.EndAttempt();
    }
}
