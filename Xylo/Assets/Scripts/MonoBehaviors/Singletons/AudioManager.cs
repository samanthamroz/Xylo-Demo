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
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }

	void Start()
	{
		
	}

	public void LoadSounds(Scene scene) {
		audioSource = GetComponent<AudioSource>();
		try {
			audioSource.clip = melodies[scene.buildIndex - 1];
		} catch {}
	}

	public void PlayMelody() {
		audioSource.Play();
	}

	public void StopMelody() {
		
	}
}