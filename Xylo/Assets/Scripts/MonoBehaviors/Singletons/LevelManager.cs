using System;
using System.Collections.Generic;
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

public class LevelManager : MonoBehaviour
{   
    public GameObject marbleReference;
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
    public static LevelManager self;
    public float songBpm, forgivenessBetweenBeats;
    private float secPerBeat, songPosInSec, songPosInBeats, dspSongTime;
    private bool attemptStarted = false;
    private AudioSource musicSource;

    void Awake() {
        self = this;
    }

    void Start() {
        GameObject[] marbles = GameObject.FindGameObjectsWithTag("Marble");
        if (marbles.Length == 0 || marbles.Length > 1) {
            throw new Exception("Too many marbles!");
        }

        musicSource = GetComponent<AudioSource>();
        secPerBeat = 60f / songBpm;
    }

    public void StartAttempt() {
        attemptList = new List<NoteTrigger>();
        //Record the time when the music starts
        dspSongTime = (float)AudioSettings.dspTime;

        //musicSource.Play();
        attemptStarted = true;
    }

    void Update()
    {
        if (!attemptStarted) {
            return;
        }
        
        songPosInSec = (float)(AudioSettings.dspTime - dspSongTime);
        songPosInBeats = songPosInSec / secPerBeat;
    }

    public void TriggerNote(Note note) {
        if (!attemptStarted) {
            return;
        }
        
        attemptList.Add(new NoteTrigger(note, songPosInBeats));
    }

    public void EndAttempt(bool isDeathPlane = false) {
        if (!attemptStarted) {
            if (isDeathPlane) {
                ControlsManager.self.ExitCinematicMode(isDeathPlane);
            }
            return;
        }

        attemptStarted = false;
        Debug.Log("Win = " + IsWin());
        attemptList = new();
        
        ControlsManager.self.ExitCinematicMode(isDeathPlane);
    }

    private void PrintNoteList(List<NoteTrigger> list) {
        string str = "";
        foreach (var thing in list) {
            str += "(" + thing.note + ", " + thing.beatTriggered + ") ";
        }
        print (str);
    }

    private bool IsWin() {
        PrintNoteList(solutionList);
        PrintNoteList(attemptList);
        if (attemptList[0].note != solutionList[0].note) {
            return false;
        }
        float distanceBetweenAttemptNotes, distanceBetweenSolutionNotes;

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

    public void RetryLevel() {
        marbleReference.transform.position = marbleReference.GetComponent<PlayerMarble>().resetPosition;
    }
}
