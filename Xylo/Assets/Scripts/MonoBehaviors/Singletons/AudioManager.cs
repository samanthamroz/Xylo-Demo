using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager self;
    void Awake() {
		if (self == null) {
			self = this;
			//SceneManager.sceneLoaded += InstantiateLevelUI;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }
}