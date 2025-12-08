using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LockedUntilLevelCompleteButton : MonoBehaviour
{
    [SerializeField] int unlockWhenLevelComplete;

    void Start() {
        GetComponent<Button>().interactable = LoadingManager.self.IsLevelCompleted(0, unlockWhenLevelComplete);
    }
}
