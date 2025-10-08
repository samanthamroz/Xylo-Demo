using UnityEngine;

public class DeathPlane : MonoBehaviour {
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.TryGetComponent<PlayerMarble>(out _)) {
            LevelManager.self.EndAttempt();
        }
    }
}
