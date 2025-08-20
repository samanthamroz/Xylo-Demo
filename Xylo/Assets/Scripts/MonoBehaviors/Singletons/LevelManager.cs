using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

    private int currentSection = 0;

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
    private float songBpm, forgivenessBetweenBeats;
    private float secPerBeat, songPosInSec, songPosInBeats, dspSongTime;
    [HideInInspector] public bool attemptStarted;
    private bool attemptCountingStarted;

    void Awake() {
        self = this;
    }
    void Start() {
        attemptStarted = false;

        marble = Instantiate(marblePrefab, marbleStartPos, Quaternion.identity);

        secPerBeat = 60f / songBpm;
    }

    private double pauseStartDspTime = 0f, totalPausedTime = 0f;

    void Update()
    {
        double currentDspTime = AudioSettings.dspTime;
        if (!ControlsManager.self.isGamePaused)
        {
            if (pauseStartDspTime != 0) {
                totalPausedTime += currentDspTime - pauseStartDspTime;
            }
            pauseStartDspTime = 0;
            
            songPosInSec = (float)(currentDspTime - dspSongTime - totalPausedTime);
            songPosInBeats = songPosInSec / secPerBeat;
        } else {
            if (pauseStartDspTime == 0) {
                pauseStartDspTime = currentDspTime;
            }
        }
    }

    public void StartAttempt()
    {
        attemptStarted = true;
        attemptList = new List<NoteTrigger>();

        GUIManager.self.TogglePlayButtonImage(false);
        CameraManager.self.SwitchLookAtObject(marble, false);
        CameraManager.self.SetCameraMode(CamMode.CINEMATIC);

        //for when StartAttempt() isn't called by the marble itself
        marble.GetComponent<Rigidbody>().isKinematic = false;
    }
    public void StartCountingForAttempt() {
        //Record the time when the music starts
        dspSongTime = (float)AudioSettings.dspTime;
        attemptCountingStarted = true;
    }
    public void EndAttempt(bool retrySection = true) {
        if (!attemptStarted) {
            if (retrySection) {
                RetrySection();
            }
            return;
        }
        attemptStarted = false;
        attemptCountingStarted = false;

        bool hasWonSection = false;
        try {
            hasWonSection = CheckForSectionWin();
        } catch (NullReferenceException) {} //occurs when restart is triggered before first note block is triggered

        if (!hasWonSection) {
            attemptList = new();
            if (retrySection) {
                RetrySection();
            }
            return;
        }

        //TODO: Cinematic for end of level
        LoadingManager.self.SetCurrentSectionCompleted(0); //TODO: Fix
        ControlsManager.self.ActivateMainMap();
        CameraManager.self.SetCameraMode(CamMode.NORMAL);
        GUIManager.self.ActivateWinMenuUI();
    }
    
    private void PrintNoteList(List<NoteTrigger> list) {
        string str = "";
        foreach (var thing in list) {
            str += "(" + thing.note + ", " + thing.beatTriggered + ") ";
        }
        print (str);
    }
    private bool CheckForSectionWin() {
        if (attemptList.Count < 1) {
            return false;
        }

        //PrintNoteList(solutionList);
        //PrintNoteList(attemptList);
        if ((attemptList[0].note != solutionList[0].note) ||
            (attemptList.Count != solutionList.Count)) {
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
    private void RetrySection() {
        ControlsManager.self.ActivateMainMap();
        CameraManager.self.SetCameraMode(CamMode.NORMAL);
        GUIManager.self.TogglePlayButtonImage(true);
        marble.GetComponent<PlayerMarble>().ResetSelf();
    }
    public void TriggerNote(Note note) {
        if (!attemptCountingStarted) {
            return;
        }
        
        attemptList.Add(new NoteTrigger(note, songPosInBeats));
    }  
}