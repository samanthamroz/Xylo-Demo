using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
	[SerializeField] private List<AudioClip> melodies;
	private AudioSource audioSource;
    public static AudioManager self;
    void Awake() {
		if (self == null) {
			self = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }

	void Start()
	{
		
	}

	public void LoadSounds(int sceneNumber) {
		audioSource = GetComponent<AudioSource>();
		try {
			audioSource.clip = melodies[sceneNumber - 1];
		} catch {}
	}

	public void PlayMelody() {
		audioSource.Play();
	}

	public void StopMelody() {
		
	}
}