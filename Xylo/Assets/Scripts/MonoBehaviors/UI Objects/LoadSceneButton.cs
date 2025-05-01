using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneButton : MonoBehaviour
{
    public void Load(string sceneName) {
        StartCoroutine(LoadingManager.self.LoadNewScene(sceneName));
    }
}