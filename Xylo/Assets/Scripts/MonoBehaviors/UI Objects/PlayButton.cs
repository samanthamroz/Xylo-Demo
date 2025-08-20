using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    public void PressButton() {
        if (!LevelManager.self.attemptStarted) {
            LevelManager.self.StartAttempt();
        } else {
            LevelManager.self.EndAttempt();
        }
    }
}
