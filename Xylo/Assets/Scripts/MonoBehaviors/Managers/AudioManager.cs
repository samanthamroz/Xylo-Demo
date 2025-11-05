using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour {
	[SerializeField] private AudioConfiguration audioConfiguration;
	private AudioClipList levelAudioClips;
	private AudioSource audioSource;
	public static AudioManager self;
	void Awake() {
		if (self == null) {
			self = this;
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
		}
	}

	void Start() {
		audioSource = GetComponent<AudioSource>();

		var currentAudioData = audioConfiguration.GetLevelAudioData(LoadingManager.self.GetCurrentLevelNumber());
        if (currentAudioData == null) {
            Debug.LogError("Failed to load audio configuration!");
            return;
        }

        levelAudioClips = currentAudioData.sectionClips;
	}

	public void PlayMelodyForSection(int sectionNum) {
		audioSource.clip = levelAudioClips[sectionNum];
		audioSource.Play();
	}

	public void PlayMelodyForCurrentSection() {
		audioSource.clip = levelAudioClips[LevelManager.self.sectionNum];
		audioSource.Play();
	}
}