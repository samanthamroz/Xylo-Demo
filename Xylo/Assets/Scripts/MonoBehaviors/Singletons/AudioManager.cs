using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
class AudioClipList {
	public List<AudioClip> list = new();

	public AudioClip this[int index] {
		get { return list[index]; }
		set { list[index] = value; }
	}
}

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour {
	[SerializeField] private List<AudioClipList> melodyFragments = new();
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

	public void PlayMelodyForSection(int levelNum, int sectionNum) {
		audioSource.clip = melodyFragments[levelNum][sectionNum];
		audioSource.Play();
	}

	public void PlayMelodyForCurrentSection() {
		audioSource.clip = melodyFragments[LoadingManager.self.GetCurrentLevelNumber()][LevelManager.self.sectionNum];
		audioSource.Play();
	}
}