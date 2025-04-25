using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoMenuButton : MonoBehaviour
{
    public void TogglePiano() {
        GUIManager.self.TogglePiano();
    }
}
