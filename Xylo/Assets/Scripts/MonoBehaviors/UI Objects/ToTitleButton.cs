using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToTitleButton : MonoBehaviour
{
    public void GoToTitle() {
        GUIManager.self.ToggleTitleScreen();
        CameraManager.self.SwitchTitleScreenPosition("title");
    }
}
