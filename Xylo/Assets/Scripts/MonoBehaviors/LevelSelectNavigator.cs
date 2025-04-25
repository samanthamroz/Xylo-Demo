using System.Collections.Generic;
using UnityEngine;

public class LevelSelectNavigator : MonoBehaviour
{
    public void MoveToLevel(int levelNumber) {
        CameraManager.self.SwitchLookAtObject(transform.GetChild(levelNumber - 1).gameObject);
    }
}
