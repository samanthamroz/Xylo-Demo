using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//inspo: https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity

public class LevelManager : MonoBehaviour {
    public static LevelManager self;
    [SerializeField] private LevelConfiguration levelConfig;

    //Level config data
    [HideInInspector] public Vector3 directionMoving;
    private NoteTriggerArray[] solutions;


    public GameObject marblePrefab;
    private GameObject marbleObject;
    
    private NoteTriggerArray currentSectionSolution { get { return solutions[sectionNum]; } }
    private List<NoteTrigger> attemptList;

    private Vector3 marbleStartPosition;
    private float forgivenessBetweenBeats = .15f;
    private bool attemptCountingStarted, fullLevelCountingStarted;

    [SerializeField] private bool DEBUG_AutoWin;
    [SerializeField] public bool DEBUG_UseManualStart, DEBUG_FreeAdvance;
    [SerializeField] private Vector3 DEBUG_ManualPosition;
    [SerializeField] public Vector3 DEBUG_ManualVelocity;
    
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
        
        // Load level-specific configuration
        var currentLevelData = levelConfig.GetLevelData(levelNum);
        if (currentLevelData == null) {
            Debug.LogError("Failed to load level configuration!");
            return;
        }
        
        directionMoving = currentLevelData.marbleDirection;
        solutions = currentLevelData.sectionSolutions;
        
        if (DEBUG_UseManualStart) {
            marbleObject = Instantiate(marblePrefab, DEBUG_ManualPosition, Quaternion.identity);
        } else {
            float horizontalDistanceToFirst = Mathf.Abs(BeatManager.self.xDistancePerBeat * BeatManager.self.beatsBetweenFirstTwoBeats);
            
            Vector3 offset = -directionMoving * horizontalDistanceToFirst;
            marbleStartPosition = new Vector3(
                currentLevelData.firstBlockPosition.x + offset.x, 
                currentLevelData.firstBlockPosition.y + .3f, 
                currentLevelData.firstBlockPosition.z + offset.z
            );
            
            marbleObject = Instantiate(marblePrefab, marbleStartPosition, Quaternion.identity);
        }

        CollisionManager.self.TurnOffCollisionForPuzzle(0);
        CloudsManager.self.MoveCloudsForSection(0);
        LoadingManager.self.SetMarbleStartForSection(0, VectorUtils.nullVector, marble.transform.position);
        if (levelNum != 0) AudioManager.self.PlayMelodyForCurrentSection();
        SnapDraggables();
    }

    public void StartPlaying() {
        attemptStarted = true;
        attemptList = new List<NoteTrigger>();

        CameraManager.self.DoBeginAttempt(sectionNum, marbleObject);
        SnapDraggables();
        marble.RunMarble();
    }
    private void SnapDraggables() {
        var draggables = FindObjectsByType<DraggableInteractable>(FindObjectsSortMode.None);
        foreach (DraggableInteractable d in draggables) {
            d.transform.localPosition = VectorUtils.GetSnapToGridVector(Vector3.zero, d.transform.localPosition);
        }
    }
    public void StartCountingForAttempt() {
        attemptCountingStarted = true;
    }
    public void EndAttempt(bool retrySection = true, bool autoWin = false, bool autoLose = false) {
        if (!attemptStarted) {
            if (retrySection) {
                marble.ResetSelf(sectionNum == 0);
            }
            return;
        }
        attemptStarted = false;
        attemptCountingStarted = false;

        if (autoLose) {
            return;
        }

        bool hasWonSection = false;
        if (!autoWin && !DEBUG_AutoWin) {
            try {
                hasWonSection = CheckForSectionWin();
            }
            catch (NullReferenceException) { } //occurs when restart is triggered before first note block is triggered
        }

        if (!autoWin && !hasWonSection && !DEBUG_AutoWin) {
            attemptList = new();
            if (retrySection) {
                marble.ResetSelf(sectionNum == 0);
            }
            CameraManager.self.DoMoveToNextSection(sectionNum);
            return;
        }

        LoadingManager.self.SetSectionCompleted(sectionNum);
        LoadingManager.self.SetMarbleStartForSection(sectionNum + 1, marble.GetComponent<Rigidbody>().velocity, marble.transform.position);
        //Move to next section
        if (!LoadingManager.self.IsLevelCompleted()) {
            GoToNextSection();
            return;
        }

        //TODO: Level won stuff
        if (levelNum == 0) {
            CollisionManager.self.TurnOffAllCollision();
            marble.RunMarbleFromBeginning();
            CameraManager.self.DoEndOfLevel(marble.gameObject);
            attemptStarted = true;
            attemptList = new List<NoteTrigger>();
            attemptCountingStarted = true;
            fullLevelCountingStarted = true;
            ControlsManager.self.ActivateMenuMap();
        } else {
            GUIManager.self.ActivateWinMenuUI();
            ControlsManager.self.ActivateMenuMap();
        }
        
    }

    private IEnumerator DelayedAutoStart() {
        yield return new WaitForSeconds(1.5f); // Wait for camera transition and marble reset
        StartPlaying();
    }

    public void GoToSection(int sectionGoTo) {
        CameraManager.self.StopAllCoroutines();

        sectionNum = sectionGoTo;
        
        CameraManager.self.DoMoveToNextSection(sectionNum);
        CloudsManager.self.MoveCloudsForSection(sectionNum);
        CollisionManager.self.TurnOffCollisionForPuzzle(sectionNum);

        VelocityPosition marbStart = LoadingManager.self.GetMarbleStartForSection(sectionNum);
        marble.PlaceMarbleForSectionStart(marbStart.velocity, marbStart.position);

        if (sectionNum == 6) {
            StartCoroutine(DelayedAutoStart());
        }
    }

    public void GoToNextSection() {
        if (!LoadingManager.self.IsCurrentSectionCompleted(sectionNum) && !DEBUG_FreeAdvance) return;

        sectionNum += 1;
        
        CameraManager.self.DoMoveToNextSection(sectionNum);
        CloudsManager.self.MoveCloudsForSection(sectionNum);
        CollisionManager.self.TurnOffCollisionForPuzzle(sectionNum);

        VelocityPosition marbStart = LoadingManager.self.GetMarbleStartForSection(sectionNum);
        marble.PlaceMarbleForSectionStart(marbStart.velocity, marbStart.position);

        if (sectionNum == 6) {
            StartCoroutine(DelayedAutoStart());
        }
    }
    

    public void GoToPreviousSection() {
        if (sectionNum == 0) return;

        sectionNum -= 1;

        CameraManager.self.DoMoveToNextSection(sectionNum);
        CloudsManager.self.MoveCloudsForSection(sectionNum, true);
        CollisionManager.self.TurnOffCollisionForPuzzle(sectionNum);

        VelocityPosition marbStart = LoadingManager.self.GetMarbleStartForSection(sectionNum);
        marble.PlaceMarbleForSectionStart(marbStart.velocity, marbStart.position);

        if (sectionNum == 6) {
            StartCoroutine(DelayedAutoStart());
        }
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

        if (currentSectionSolution.Length == 0) return true;

        //PrintNoteList(currentSectionSolution.ToList());
        //PrintNoteList(attemptList);
        //PrintDistanceList(attemptList);
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

    private int CheckForSectionInaccuracies() {
        int notesChecked = 0;
        int sectionChecking = 0;

        while (notesChecked < attemptList.Count && sectionChecking < solutions.Length) {
            NoteTriggerArray arrayChecking = solutions[sectionChecking];
            int localIndex = 0;

            // Check all notes in current section
            while (localIndex < arrayChecking.Length && notesChecked < attemptList.Count) {
                int attemptIndex = notesChecked;
                
                // Check note match
                if (attemptList[attemptIndex].note != arrayChecking[localIndex].note) {
                    return sectionChecking;
                }

                // Check timing (skip for first note of section)
                if (localIndex > 0) {
                    double distanceBetweenAttemptNotes = 
                        attemptList[attemptIndex].beatTriggered - attemptList[attemptIndex - 1].beatTriggered;
                    double distanceBetweenSolutionNotes = 
                        arrayChecking[localIndex].beatTriggered - arrayChecking[localIndex - 1].beatTriggered;

                    if (Math.Abs(distanceBetweenAttemptNotes - distanceBetweenSolutionNotes) >= forgivenessBetweenBeats) {
                        return sectionChecking;
                    }
                }

                localIndex++;
                notesChecked++;
            }

            sectionChecking++;
        }

        return -1; // No inaccuracies
    }


    public void TriggerNote(Note note) {
        if (!attemptCountingStarted) {
            return;
        }

        attemptList.Add(new NoteTrigger(note, BeatManager.self.songPosInBeats));

        if (fullLevelCountingStarted) {
            if (DEBUG_AutoWin) return;

            int wrongSection = CheckForSectionInaccuracies();

            if (wrongSection != -1) {
                fullLevelCountingStarted = false;
                EndAttempt(true, false, true);
                LoadingManager.self.SetSectionCompleted(wrongSection, false);
                GoToSection(wrongSection);
            }
        }
    }
}