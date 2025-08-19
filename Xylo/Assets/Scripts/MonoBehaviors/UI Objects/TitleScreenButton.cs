using UnityEngine;

public class TitleScreenButton : MonoBehaviour
{
    public void ButtonPressed() {
        CameraManager.self.SwitchTitleScreenPosition("level0");
        GUIManager.self.ToggleTitleScreen();
    }
}