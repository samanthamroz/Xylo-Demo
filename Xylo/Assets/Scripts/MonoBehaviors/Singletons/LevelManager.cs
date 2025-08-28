using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//inspo: https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity

class NoteTrigger
{
    public Note note { get; set; }
    public double beatTriggered { get; set; }
    public NoteTrigger(Note _note, double _beatTriggered)
    {
        note = _note;
        beatTriggered = _beatTriggered;
    }
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager self;

    private int levelNum { get { return LoadingManager.self.GetCurrentLevelNumber(); } }
    private int sectionNum = 0;

    public GameObject marblePrefab, deathPlanePrefab;
    private GameObject marble, deathPlane;
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
    private NoteTrigger[] currentSectionSolution { get { return solutions[levelNum - 1][sectionNum]; } }
    private List<NoteTrigger> attemptList;

    private Vector3[] marbleStartPositions = { new(.5f, 13f, -6) };
    private Vector2[][] deathPlaneCoords = {
        //Level 1
        new Vector2[] {new(0, 8), new(8, 8), new(16, 8), new(24, 8)}
    };
    private float forgivenessBetweenBeats = .2f;
    [HideInInspector] public bool attemptStarted;
    private bool attemptCountingStarted;

    [SerializeField] private bool DEBUG_AutoWin;

    void Awake()
    {
        self = this;
    }
    void Start()
    {
        attemptStarted = false;

        marble = Instantiate(marblePrefab, marbleStartPositions[levelNum - 1], Quaternion.identity);
        deathPlane = Instantiate(deathPlanePrefab, deathPlaneCoords[levelNum - 1][sectionNum], Quaternion.identity);
    }
    public void StartPlaying()
    {
        attemptStarted = true;
        attemptList = new List<NoteTrigger>();

        CameraManager.self.DoBeginAttempt(sectionNum, marble);

        marble.GetComponent<PlayerMarble>().RunMarble();
    }
    public void StartCountingForAttempt()
    {
        attemptCountingStarted = true;
    }
    public void EndAttempt(bool retrySection = true)
    {
        if (!attemptStarted)
        {
            if (retrySection)
            {
                marble.GetComponent<PlayerMarble>().ResetSelf();
            }
            return;
        }
        attemptStarted = false;
        attemptCountingStarted = false;

        bool hasWonSection = false;
        try
        {
            hasWonSection = CheckForSectionWin();
        }
        catch (NullReferenceException) { } //occurs when restart is triggered before first note block is triggered

        print(hasWonSection);
        if (!hasWonSection && !DEBUG_AutoWin)
        {
            attemptList = new();
            if (retrySection)
            {
                marble.GetComponent<PlayerMarble>().ResetSelf();
            }
            return;
        }
        LoadingManager.self.SetCurrentSectionCompleted(sectionNum);

        //Move to next section
        if (!LoadingManager.self.IsLevelCompleted())
        {
            sectionNum += 1;

            CameraManager.self.DoMoveToNextSection(sectionNum);

            LeanTween.moveLocal(deathPlane, deathPlaneCoords[levelNum - 1][sectionNum], .5f);

            marble.GetComponent<PlayerMarble>().SaveCurrentVelocity();
            marble.GetComponent<PlayerMarble>().ResetSelfToCurrentPosition();
            return;
        }

        //TODO: Level won stuff
        CameraManager.self.DoEndOfLevel();
    }

    private void PrintNoteList(List<NoteTrigger> list)
    {
        string str = "";
        foreach (var thing in list)
        {
            str += "(" + thing.note + ", " + thing.beatTriggered + ") ";
        }
        print(str);
    }
    private bool CheckForSectionWin()
    {
        if (attemptList.Count < 1)
        {
            return false;
        }

        PrintNoteList(currentSectionSolution.ToList());
        PrintNoteList(attemptList);
        if ((attemptList[0].note != currentSectionSolution[0].note) ||
            (attemptList.Count != currentSectionSolution.Length))
        {
            return false;
        }
        double distanceBetweenAttemptNotes, distanceBetweenSolutionNotes;

        for (int i = 1; i < attemptList.Count; i++)
        {
            if (attemptList[i].note != currentSectionSolution[i].note)
            {
                return false;
            }
            distanceBetweenAttemptNotes = attemptList[i].beatTriggered - attemptList[i - 1].beatTriggered;
            distanceBetweenSolutionNotes = currentSectionSolution[i].beatTriggered - currentSectionSolution[i - 1].beatTriggered;

            if (Math.Abs(distanceBetweenAttemptNotes - distanceBetweenSolutionNotes) >= forgivenessBetweenBeats)
            {
                return false;
            }
        }
        return true;
    }
    public void TriggerNote(Note note)
    {
        if (!attemptCountingStarted)
        {
            return;
        }

        attemptList.Add(new NoteTrigger(note, BeatManager.self.songPosInBeats));
    }
}