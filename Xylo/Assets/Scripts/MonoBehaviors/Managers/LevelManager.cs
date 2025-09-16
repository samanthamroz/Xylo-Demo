using System;
using System.Collections.Generic;
using UnityEngine;

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

    public GameObject marblePrefab, deathPlaneObj;
    private GameObject marbleObject;
    
    private NoteTrigger[][][] solutions =
    {
        //Level 0
        new NoteTrigger[][] {
            //Individual Sections
            new NoteTrigger[] {new(Note.E, 1f), new(Note.B, 2f), new(Note.Gb, 3f), new(Note.B, 4f)},
            new NoteTrigger[] {new(Note.E, 1f), new(Note.A, 2f), new(Note.B, 3f), new(Note.B, 4f)},
            new NoteTrigger[] {new(Note.E, 1f), new(Note.B, 2f), new(Note.E, 3f), new(Note.Gb, 4f)},
            new NoteTrigger[] {new(Note.Ab, 1f), new(Note.B, 2f), new(Note.Eb, 3f), new(Note.Db, 4f)},
        }
    };
    private NoteTrigger[] currentSectionSolution { get { return solutions[levelNum][sectionNum]; } }
    private List<NoteTrigger> attemptList;

    [SerializeField] private Transform firstBlockPos;
    [SerializeField] private Vector3 setDirectionMoving = Vector3.right;
    [HideInInspector] public Vector3 directionMoving => setDirectionMoving;

    private Vector3 marbleStartPosition;
    private Vector3[][] deathPlaneCoords = {
        //Level 1
        new Vector3[] {new(0, 0, 0), new(0f, 0f, 17.2f), new(0, -1.29f, 32f), new(0, -6, 49)}
    };
    private float forgivenessBetweenBeats = .1f;
    private bool attemptCountingStarted;

    [SerializeField] private bool DEBUG_AutoWin;
    [SerializeField] public bool DEBUG_UseManualStart = false;
    [SerializeField] private Vector3 DEBUG_ManualPosition;
    public Vector3 DEBUG_ManualVelocity;
    
    [HideInInspector] public int sectionNum { get; private set; }
    [HideInInspector] public bool attemptStarted { get; private set; }

    private PlayerMarble marble => marbleObject.GetComponent<PlayerMarble>();
    private int levelNum => LoadingManager.self.GetCurrentLevelNumber();

    void Awake() {
        self = this;
    }
    void Start() {
        sectionNum = 0;
        attemptStarted = false;

        if (DEBUG_UseManualStart) {
            marbleObject = Instantiate(marblePrefab, DEBUG_ManualPosition, Quaternion.identity);
        } else {
            float horizontalDistanceToFirst = Mathf.Abs(BeatManager.self.xDistancePerBeat * BeatManager.self.beatsBetweenFirstTwoBeats);
            marbleStartPosition = new(firstBlockPos.position.x + horizontalDistanceToFirst * -directionMoving.x, 
                firstBlockPos.position.y + .3f, 
                firstBlockPos.position.z - 1);
            marbleObject = Instantiate(marblePrefab, marbleStartPosition, Quaternion.identity);
        }

        LoadingManager.self.SetMarbleStartForSection(0, VectorUtils.nullVector, marble.transform.position);
    }
    public void StartPlaying() {
        attemptStarted = true;
        attemptList = new List<NoteTrigger>();

        CameraManager.self.DoBeginAttempt(sectionNum, marbleObject);

        marble.RunMarble();
    }
    public void StartCountingForAttempt() {
        attemptCountingStarted = true;
    }
    public void EndAttempt(bool retrySection = true) {
        if (!attemptStarted) {
            if (retrySection) {
                marble.ResetSelf();
            }
            return;
        }
        attemptStarted = false;
        attemptCountingStarted = false;

        bool hasWonSection = false;
        try {
            hasWonSection = CheckForSectionWin();
        }
        catch (NullReferenceException) { } //occurs when restart is triggered before first note block is triggered

        if (!hasWonSection && !DEBUG_AutoWin) {
            attemptList = new();
            if (retrySection) {
                marble.ResetSelf();
            }
            CameraManager.self.DoMoveToNextSection(sectionNum);
            return;
        }

        LoadingManager.self.SetCurrentSectionCompleted(sectionNum);
        LoadingManager.self.SetMarbleStartForSection(sectionNum + 1, marble.GetComponent<Rigidbody>().velocity, marble.transform.position);
        //Move to next section
        if (!LoadingManager.self.IsLevelCompleted()) {
            GoToNextSection();
            return;
        }

        //TODO: Level won stuff
        marble.RunMarbleFromBeginning();
        CameraManager.self.DoEndOfLevel(marble.gameObject);
    }

    public void GoToNextSection() {
        //if (!LoadingManager.self.IsCurrentSectionCompleted(sectionNum)) return;

        sectionNum += 1;

        CameraManager.self.DoMoveToNextSection(sectionNum);

        LeanTween.moveLocal(deathPlaneObj, deathPlaneCoords[levelNum][sectionNum], .5f);

        VelocityPosition marbStart = LoadingManager.self.GetMarbleStartForSection(sectionNum);

        marble.PlaceMarbleForSectionStart(marbStart.velocity, marbStart.position);
    }

    public void GoToPreviousSection() {
        if (sectionNum == 0) return;

        sectionNum -= 1;

        CameraManager.self.DoMoveToNextSection(sectionNum);

        LeanTween.moveLocal(deathPlaneObj, deathPlaneCoords[levelNum][sectionNum], .5f);

        VelocityPosition marbStart = LoadingManager.self.GetMarbleStartForSection(sectionNum);
        marble.PlaceMarbleForSectionStart(marbStart.velocity, marbStart.position);
    }

    private void PrintNoteList(List<NoteTrigger> list) {
        string str = "| ";
        foreach (var thing in list) {
            string s = $"{thing.note:F}, {thing.beatTriggered:F} | ";
            str += s;
        }
        print(str);
    }
    private void PrintDistanceList(List<NoteTrigger> list) {
        List<double> distanceList = new();
        for (int i = 1; i < list.Count; i++) {
            distanceList.Add(list[i].beatTriggered - list[i - 1].beatTriggered);
        }

        string str = "| ";
        double distanceCovered = 0;
        for (int i = 0; i < list.Count - 1; i++) {
            string s = $"{list[i].note:F}, {distanceList[i]:F} | ";
            str += s;
            distanceCovered += distanceList[i];
            if (Mathf.Floor((float)distanceCovered) != 0 && Mathf.Round((float)distanceCovered) % 4 == 0) {
                str += "\n|";
                distanceCovered = 0;
            }
        }
        print(str);
    }
    
    private bool CheckForSectionWin() {
        if (attemptList.Count < 1) {
            return false;
        }

        //PrintNoteList(currentSectionSolution.ToList());
        PrintNoteList(attemptList);
        PrintDistanceList(attemptList);
        if ((attemptList[0].note != currentSectionSolution[0].note) ||
            (attemptList.Count != currentSectionSolution.Length)) {
            return false;
        }
        double distanceBetweenAttemptNotes, distanceBetweenSolutionNotes;

        for (int i = 1; i < attemptList.Count; i++) {
            if (attemptList[i].note != currentSectionSolution[i].note) {
                return false;
            }
            distanceBetweenAttemptNotes = attemptList[i].beatTriggered - attemptList[i - 1].beatTriggered;
            distanceBetweenSolutionNotes = currentSectionSolution[i].beatTriggered - currentSectionSolution[i - 1].beatTriggered;

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