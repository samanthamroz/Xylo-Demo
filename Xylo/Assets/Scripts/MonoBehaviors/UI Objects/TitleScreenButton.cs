using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenButton : MonoBehaviour
{
    public void DeactivateTitleScreen() {
        ControlsManager.self.ToggleMenuActionMap();
        transform.parent.gameObject.SetActive(false);
    }
}
