using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectButtons : MonoBehaviour
{
    public void Load(string sceneName) {
        LoadingManager.self.LoadNewScene(sceneName);
    }
}