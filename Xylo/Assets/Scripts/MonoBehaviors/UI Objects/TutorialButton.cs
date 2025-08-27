using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialButton : MonoBehaviour
{
    public List<GameObject> frames;
    private GameObject currentFrame;
    private int currentFrameNumber = 0;
    public void Start() {
        foreach (GameObject g in frames) {
            g.SetActive(false);
        }

        currentFrame = frames[currentFrameNumber];
        AdvanceTutorial();
    }
    public void AdvanceTutorial() {
        if (currentFrameNumber == frames.Count) {
            ControlsManager.self.ActivateMainMap();
            GUIManager.self.ActivateLevelUI();
            Destroy(transform.parent.gameObject);
            return;
        }
        if (currentFrameNumber == 3) {
            //AudioManager.self.PlayMelody();
        }
        currentFrame = frames[currentFrameNumber];
        currentFrame.SetActive(true);
        currentFrameNumber += 1;
    }
}
