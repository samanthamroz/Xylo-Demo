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
    public static LevelManager self;

    public GameObject marblePrefab;
    private GameObject marble;
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
    [HideInInspector] public Vector3 marbleStartPos = new(-14.67694f, 7.25f, 1.5f);
    public float songBpm, forgivenessBetweenBeats; //idk why but these have to be public
    private float secPerBeat, songPosInSec, songPosInBeats, dspSongTime;
    [HideInInspector] public bool attemptStarted;
    private bool attemptCountingStarted, hasWon;

    void Awake() {
        self = this;
    }
    void Start() {
        attemptStarted = false;

        marble = Instantiate(marblePrefab, marbleStartPos, Quaternion.identity);

        secPerBeat = 60f / songBpm;
    }
    void Update()
    {
        if (!attemptCountingStarted) {
            return;
        }
        
        songPosInSec = (float)(AudioSettings.dspTime - dspSongTime);
        songPosInBeats = songPosInSec / secPerBeat;
    }
    
    public void StartAttempt() {
        attemptStarted = true;
        attemptList = new List<NoteTrigger>();

        GUIManager.self.TogglePlayButtonImage(false);
    
        StartCoroutine(CameraManager.self.EnterCinematicMode(marble));

        //for when StartAttempt() isn't called by the marble itself
        marble.GetComponent<Rigidbody>().isKinematic = false;
    }
    public void StartCountingForAttempt() {
        //Record the time when the music starts
        dspSongTime = (float)AudioSettings.dspTime;
        attemptCountingStarted = true;
    }
    public void EndAttempt(bool retryLevel = false, bool resetCamera = false) {
        if (!attemptStarted) {
            if (!hasWon) {
                ControlsManager.self.ExitCinematicMode(resetCamera);
            }
            return;
        }

        attemptStarted = false;
        attemptCountingStarted = false;

        try {
            hasWon = IsWin();
        } catch (NullReferenceException) {} //occurs when restart is triggered before first note block is triggered

        if (hasWon) {
            LoadingManager.self.SetLevelCompleted(0);
            ControlsManager.self.ExitCinematicMode(true);
            GUIManager.self.ToggleWinMenu();
        } else {
            attemptList = new();
            ControlsManager.self.ExitCinematicMode(resetCamera);
            if (retryLevel) {
                RetryLevel();
            }
        }
    }
    
    private void PrintNoteList(List<NoteTrigger> list) {
        string str = "";
        foreach (var thing in list) {
            str += "(" + thing.note + ", " + thing.beatTriggered + ") ";
        }
        print (str);
    }
    private bool IsWin() {
        if (attemptList.Count < 1) {
            return false;
        }

        //PrintNoteList(solutionList);
        //PrintNoteList(attemptList);
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
    
    public void TriggerNote(Note note) {
        if (!attemptStarted) {
            return;
        }
        
        attemptList.Add(new NoteTrigger(note, songPosInBeats));
    }
    
    private void RetryLevel() {
        GUIManager.self.TogglePlayButtonImage(true);
        marble.GetComponent<PlayerMarble>().ResetSelf();
    }
}