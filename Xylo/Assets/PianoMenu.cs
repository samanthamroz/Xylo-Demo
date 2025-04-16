using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoMenu : MonoBehaviour
{
    public List<AudioClip> sounds = new();
    public AudioSource audioSource;
    void Start() {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlayNote(string note) {
        switch (note.ToLower()) {
            case "a":
                audioSource.clip = sounds[0];
                break;
            default:
                break;
        }
    }
}
