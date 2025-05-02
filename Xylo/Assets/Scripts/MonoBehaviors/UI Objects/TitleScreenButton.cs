using UnityEngine;

public class TitleScreenButton : MonoBehaviour
{
    public GameObject firstIsland;
    public void ButtonPressed() {
        CameraManager.self.SwitchLevelSelectIsland("level0");
        GUIManager.self.ToggleTitleScreen();
    }
}