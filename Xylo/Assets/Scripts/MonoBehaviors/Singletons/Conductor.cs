using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//inspo: https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity

class NoteTrigger
{
    public Note note { get; set; }
    public float beatTriggered { get; set; }
    public NoteTrigger(Note _note, float _beatTriggered)
    {
        note = _note;
        beatTriggered = _beatTriggered;
    }
}

public class Conductor : MonoBehaviour
{   
    private List<NoteTrigger> solutionList = new() 
    {
        new NoteTrigger(Note.G, 0f),
        new NoteTrigger(Note.Gb, 1.5f),
        new NoteTrigger(Note.D, 3f),
        new NoteTrigger(Note.C, 5f),
        new NoteTrigger(Note.D, 5.5f),
        new NoteTrigger(Note.E, 6f),
        new NoteTrigger(Note.G, 7f),
        new NoteTrigger(Note.C, 8f),
        new NoteTrigger(Note.D, 9f),
    };
    private List<NoteTrigger> attemptList;
    public static Conductor self;
    public float songBpm, forgivenessBetweenBeats;
    private float secPerBeat, songPosInSec, songPosInBeats, dspSongTime;
    private bool isPlaying = false;
    private AudioSource musicSource;

    void Awake() {
        self = this;
    }

    void Start() {
        musicSource = GetComponent<AudioSource>();
        secPerBeat = 60f / songBpm;
    }

    public void StartAttempt() {
        attemptList = new List<NoteTrigger>();
        //Record the time when the music starts
        dspSongTime = (float)AudioSettings.dspTime;
        //musicSource.Play();
        isPlaying = true;
    }

    void Update()
    {
        if (isPlaying) {
            songPosInSec = (float)(AudioSettings.dspTime - dspSongTime);
            songPosInBeats = songPosInSec / secPerBeat;
        }
    }

    public void TriggerNote(Note note) {
        attemptList.Add(new NoteTrigger(note, songPosInBeats));
    }

    public void EndAttempt() {
        Debug.Log("Win = " + IsWin());
    }

    private void printList(List<NoteTrigger> list) {
        string str = "";
        foreach (var thing in list) {
            str += "(" + thing.note + ", " + thing.beatTriggered + ") ";
        }
        print (str);
    }

    private bool IsWin() {
        float distanceBetweenAttemptNotes, distanceBetweenSolutionNotes;
        
        if (attemptList[0].note != solutionList[0].note) {
            return false;
        }
        for (int i = 1; i < attemptList.Count; i++) {
            if (attemptList[i].note != solutionList[i].note) {
                return false;
            }
            distanceBetweenAttemptNotes = attemptList[i].beatTriggered - attemptList[i - 1].beatTriggered;
            distanceBetweenSolutionNotes = solutionList[i].beatTriggered - solutionList[i - 1].beatTriggered;
            
            if (Math.Abs(distanceBetweenAttemptNotes - distanceBetweenSolutionNotes) >= forgivenessBetweenBeats) {
                return false;
            }
        }
        return true;
    }
}
