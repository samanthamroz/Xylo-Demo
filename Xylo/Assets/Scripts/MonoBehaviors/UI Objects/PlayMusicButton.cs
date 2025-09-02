using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusicButton : MonoBehaviour {
    public void PlayMusic() {
        AudioManager.self.PlayMelodyForCurrentSection();
    }
}
