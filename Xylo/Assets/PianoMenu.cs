using System.Collections.Generic;
using UnityEngine;

public class PianoMenu : MonoBehaviour
{
    public List<AudioClip> sounds = new();
    private AudioSource audioSource;
    void Start() {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlayNote(string note) {
        switch (note.ToLower()) {
            case "ab":
                audioSource.clip = sounds[11];
                break;
            case "a":
                audioSource.clip = sounds[0];
                break;
            case "bb":
                audioSource.clip = sounds[1];
                break;
            case "b":
                audioSource.clip = sounds[2];
                break;
            case "c":
                audioSource.clip = sounds[3];
                break;
            case "db":
                audioSource.clip = sounds[4];
                break;
            case "d":
                audioSource.clip = sounds[5];
                break;
            case "eb":
                audioSource.clip = sounds[6];
                break;
            case "e":
                audioSource.clip = sounds[7];
                break;
            case "f":
                audioSource.clip = sounds[8];
                break;
            case "gb":
                audioSource.clip = sounds[9];
                break;
            case "g":
                audioSource.clip = sounds[10];
                break;
            default:
                throw new System.Exception("note not assigned");
        }
        audioSource.Play();
    }
}
