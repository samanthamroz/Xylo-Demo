using System.Collections.Generic;
using UnityEngine;

public class PianoKey : MonoBehaviour
{
    private AudioSource audioSource;
    void Start() {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlayNote() {
        audioSource.Play();
    }
}
