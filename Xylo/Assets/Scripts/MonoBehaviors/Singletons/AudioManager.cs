using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
	[SerializeField] private List<AudioClip> melodies;
	private AudioSource audioSource;
    public static AudioManager self;
    void Awake() {
		if (self == null) {
			self = this;
			audioSource = GetComponent<AudioSource>();
			SceneManager.sceneLoaded += LoadSounds;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }

	void LoadSounds(Scene scene, LoadSceneMode mode) {
		if (scene.buildIndex > 1) {
			audioSource.clip = melodies[scene.buildIndex - 2];
		}
	}

	public void PlayMelody() {
		audioSource.Play();
	}

	public void StopMelody() {
		
	}
}