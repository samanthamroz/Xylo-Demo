using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectIslandStar : MonoBehaviour
{
    public int unlockWhenLevelCompleted;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(LoadingManager.self.IsLevelCompleted(unlockWhenLevelCompleted));
    }
}
