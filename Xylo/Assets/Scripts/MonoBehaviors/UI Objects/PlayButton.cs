using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    public void PressButton() {
        if (!LevelManager.self.attemptStarted) {
            print("launching");
            LevelManager.self.StartAttempt();
        } else {
            LevelManager.self.EndAttempt(true);
        }
    }
}
