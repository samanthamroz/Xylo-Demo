using UnityEngine;

public class RetryButton : MonoBehaviour
{
    public void retryLevel() {
        LevelManager.self.retryLevel();
    }
}