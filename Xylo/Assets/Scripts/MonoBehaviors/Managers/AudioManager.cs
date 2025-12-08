using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour {
	[SerializeField] private AudioConfiguration audioConfiguration;
	private AudioClipList levelAudioClips { get { return audioConfiguration.GetLevelAudioData(LoadingManager.self.GetCurrentLevelNumber()).sectionClips; } }
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