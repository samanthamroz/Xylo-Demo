using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneButton : MonoBehaviour {
    public void Load(string sceneName) {
        ControlsManager.self.PauseGameTime(false);
        StartCoroutine(LoadingManager.self.LoadNewScene(sceneName));
    }
}