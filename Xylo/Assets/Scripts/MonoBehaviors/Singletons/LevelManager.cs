using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//inspo: https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity

class NoteTrigger {
    public Note note { get; set; }
    public double beatTriggered { get; set; }
    public NoteTrigger(Note _note, double _beatTriggered) {
        note = _note;
        beatTriggered = _beatTriggered;
    }
}

public class LevelManager : MonoBehaviour {
    public static LevelManager self;

    private int levelNum { get { return LoadingManager.self.GetCurrentLevelNumber(); } }
    private int currentSection = 0;

    public GameObject marblePrefab, deathPlanePrefab;
    private GameObject marble, deathPlane;
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

    private Vector3[] marbleStartPositions = { new(0f, 13f, -6) };
    private int[][] deathPlaneYLevels = {
        //Level 1
        new int[] {-3, -12, -25, -36}
    };
    private float forgivenessBetweenBeats;
    [HideInInspector] public bool attemptStarted;
    private bool attemptCountingStarted;

    [SerializeField] private bool DEBUG_AutoWin;

    void Awake() {
        self = this;
    }
    void Start() {
        attemptStarted = false;

        marble = Instantiate(marblePrefab, marbleStartPositions[levelNum - 1], Quaternion.identity);
        deathPlane = Instantiate(deathPlanePrefab, new(0, deathPlaneYLevels[levelNum - 1][currentSection], 0), Quaternion.identity);
    }

    private double pauseStartDspTime = 0f, totalPausedTime = 0f;
    private float lastQuantizedBeat = -.1f;
    public float currentBeat;
    private float subdivision = 4f;
    public void StartPlaying() {
        attemptStarted = true;
        attemptList = new List<NoteTrigger>();

        CameraManager.self.DoBeginAttempt(currentSection, marble);

        marble.GetComponent<PlayerMarble>().RunMarble();
    }
    public void StartCountingForAttempt() {
        attemptCountingStarted = true;
    }
    public void EndAttempt(bool retrySection = true) {
        if (!attemptStarted) {
            if (retrySection) {
                marble.GetComponent<PlayerMarble>().ResetSelf();
            }
            return;
        }
        attemptStarted = false;
        attemptCountingStarted = false;

        if (!DEBUG_AutoWin) {
            bool hasWonSection = false;
            try {
                hasWonSection = CheckForSectionWin();
            }
            catch (NullReferenceException) { } //occurs when restart is triggered before first note block is triggered

            if (!hasWonSection) {
                attemptList = new();
                if (retrySection) {
                    marble.GetComponent<PlayerMarble>().ResetSelf();
                }
                return;
            }
        }
        LoadingManager.self.SetCurrentSectionCompleted(currentSection);

        //Move to next section
        if (!LoadingManager.self.IsLevelCompleted()) {
            currentSection += 1;

            CameraManager.self.DoMoveToNextSection(currentSection);

            LeanTween.moveLocalY(deathPlane, deathPlaneYLevels[levelNum - 1][currentSection], .5f);

            //marble.GetComponent<PlayerMarble>().UpdateSavedVelocity();
            marble.GetComponent<PlayerMarble>().ResetSelfToCurrentPosition();
            return;
        }

        //TODO: Level won stuff
        /*
        ControlsManager.self.ActivateMainMap();
        CameraManager.self.SetCameraMode(CamMode.NORMAL); */
        //GUIManager.self.ActivateWinMenuUI();
    }

    private void PrintNoteList(List<NoteTrigger> list) {
        string str = "";
        foreach (var thing in list) {
            str += "(" + thing.note + ", " + thing.beatTriggered + ") ";
        }
        print(str);
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
        double distanceBetweenAttemptNotes, distanceBetweenSolutionNotes;

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
        if (!attemptCountingStarted) {
            return;
        }

        attemptList.Add(new NoteTrigger(note, BeatManager.self.songPosInBeats));
    }
}