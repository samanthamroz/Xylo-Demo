using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEditor;

public class LoadingManager : MonoBehaviour {
    public static LoadingManager self;
    private bool hasTitleLoaded = false;
    private GlobalSaveData globalData;
    private SceneSaveData currentSceneData;
    private int currentLevelNumber = 0;
    private const int NUM_LEVELS_IN_WORLD_ONE = 2;

    [SerializeField] private bool DEBUG_AlwaysResetData = false;
    [SerializeField] private int DEBUG_OverrideCurrentLevelNum = -1;

    public int GetCurrentWorldNumber() {
        return SceneManager.GetActiveScene().buildIndex - 1;
    }
    public int GetCurrentLevelNumber() {
        return currentLevelNumber;
    }

    void Awake() {
        if (self == null) {
            self = this;
            if (DEBUG_AlwaysResetData) {
                SaveManager.DeleteAll();
            }
            if (!SaveManager.GameDataExists()) {
                GlobalSaveData newSave = new();
                SaveManager.Save(new SaveProfile<GlobalSaveData>(newSave));
            }
            SceneManager.sceneLoaded += LoadCurrentLevel;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
    
    // Loading Data functions
    // These read the data from disk and save to useable variable
    // LoadCurrentLevel is triggered on Awake
    private void LoadCurrentLevel(Scene scene, LoadSceneMode mode) {
        RefreshPointerToGlobalData();

        try {
            RefreshPointerToSceneData();
        } catch {
            SaveManager.Save(
                new SaveProfile<SceneSaveData>(new() { scene = SceneManager.GetActiveScene() },
                SceneManager.GetActiveScene().name));
            RefreshPointerToSceneData();
        }

        if (DEBUG_OverrideCurrentLevelNum > -1) {
            currentLevelNumber = DEBUG_OverrideCurrentLevelNum;
            DEBUG_OverrideCurrentLevelNum = -1; //reset so the next load doesn't go back to this level
        }

        if (scene.name == "LevelSelect") { //for title only 
            if (!hasTitleLoaded) {
                LevelSetup.SetupTitle(true);
                hasTitleLoaded = true;
            } else {
                LevelSetup.SetupTitle(false);
            }
            return;
        }

        if (GetCurrentWorldNumber() == 0 && GetCurrentLevelNumber() == 0) { //for tutorial only
            LevelSetup.SetupTutorial();
            return;
        }

        LevelSetup.SetupLevel(GetCurrentWorldNumber(), currentLevelNumber);
    }
    private void OnSceneUnloaded(Scene scene) {
        SetInteractablePositions();
        SaveCurrentScene();
        SaveGlobal();
    }
    
    private void RefreshPointerToSceneData() {
        currentSceneData = SaveManager.Load<SceneSaveData>(SceneManager.GetActiveScene().name).saveData;
        if (currentSceneData == null) {
            Debug.Log("Creating new scene save");
            SaveManager.Save(
                new SaveProfile<SceneSaveData>(new() { scene = SceneManager.GetActiveScene() },
                SceneManager.GetActiveScene().name));
            RefreshPointerToSceneData();
        }
        //Debug.Log($"Loaded scene data with {currentSceneData.sectionStartMarbleVelocities.Count} velocity entries");
    }
    private void RefreshPointerToGlobalData() {
        globalData = SaveManager.Load<GlobalSaveData>().saveData;
        if (globalData == null) {
            GlobalSaveData newSave = new();
            SaveManager.Save(new SaveProfile<GlobalSaveData>(newSave));
            RefreshPointerToGlobalData();
        }
    }

    // Loading Scene Functions
    //
    // These take us from the current scene to a different one
    public IEnumerator LoadNewScene(string sceneName, int levelNum) {
        SaveGlobal();
        float time = .25f;
        GUIManager.self.LoadLeftToMiddle(time);

        yield return new WaitForSeconds(time);
        
        currentLevelNumber = levelNum;
        SceneManager.LoadScene(sceneName);
        yield return null; //fixes coroutine running during scene load
    }
    public IEnumerator LoadNewScene(string sceneName) {
        SaveGlobal();

        float time = .25f;
        GUIManager.self.LoadLeftToMiddle(time);

        yield return new WaitForSeconds(time);
        
        SceneManager.LoadScene(sceneName);
        yield return null; //fixes coroutine running during scene load
    }
    public void ReloadCurrentLevel(bool forceUnpause = false) {
        if (forceUnpause) {
            Time.timeScale = 1f;
        }

        StartCoroutine(LoadNewScene(SceneManager.GetActiveScene().name, currentLevelNumber));
    }
    public void LoadNextLevel(bool forceUnpause = false) {
        if (forceUnpause) {
            Time.timeScale = 1f;
        }

        if (currentLevelNumber + 1 < NUM_LEVELS_IN_WORLD_ONE) {
            StartCoroutine(LoadNewScene(SceneManager.GetActiveScene().name, currentLevelNumber + 1));
            return;
        }
        
        StartCoroutine(LoadNewScene("LevelSelect"));
    }

    // Saving Functions
    // 
    // These write save data to disk as it currently is
    private void SaveCurrentScene() {
        var saveProfile = new SaveProfile<SceneSaveData>(currentSceneData, currentSceneData.scene.name);
        SaveManager.Save(saveProfile);
    }
    private void SaveGlobal() {
        SaveManager.Save(new SaveProfile<GlobalSaveData>(globalData));
    }

    // Writing Functions
    //
    // These change values in the save data and then write it to disk
    public void SetSectionCompleted(int sectionNum, bool isCompleted = true) {
        //increment number of sections completed
        if (sectionNum == currentSceneData.numSectionsComplete) {
            currentSceneData.numSectionsComplete++;
            globalData.SetSectionCompleted(GetCurrentWorldNumber(), currentLevelNumber, sectionNum, isCompleted);
        }

        SaveCurrentScene();

        //set level complete if all sections are done
        if (globalData.GetTotalSectionsInLevel(GetCurrentWorldNumber(), currentLevelNumber) == globalData.GetNumCompletedSections(GetCurrentWorldNumber(), currentLevelNumber)) {
            globalData.SetLevelCompleted(GetCurrentWorldNumber(), currentLevelNumber);
        }

        SaveGlobal();
    }
    public void SetCurrentLevelCompleted() {
        globalData.SetLevelCompleted(GetCurrentWorldNumber(), currentLevelNumber);
        SaveGlobal();
    }
    public void SetMarbleStartForSection(int sectionNum, Vector3 velocity, Vector3 position) {
        if (currentSceneData.sectionStartMarbleVelocities.ContainsKey(sectionNum)) {
            currentSceneData.sectionStartMarbleVelocities[sectionNum] = velocity;
        }
        else {
            currentSceneData.sectionStartMarbleVelocities.Add(sectionNum, velocity);
        }

        if (currentSceneData.sectionStartMarblePositions.ContainsKey(sectionNum)) {
            currentSceneData.sectionStartMarblePositions[sectionNum] = position;
        }
        else {
            currentSceneData.sectionStartMarblePositions.Add(sectionNum, position);
        }

        SaveCurrentScene();
    }
    public void SetInteractablePositions() {
        var foundInteractables = FindObjectsByType<DraggableInteractable>(FindObjectsSortMode.None);
        foreach (var found in foundInteractables) {
            string foundId = ((DraggableInteractable)found).uniqueId;
            if (currentSceneData.interactablePositions.ContainsKey(foundId)) {
                currentSceneData.interactablePositions[foundId] = found.transform.position;
            } else {
                currentSceneData.interactablePositions.Add(foundId, found.transform.position);
            }
        }
    }

    // References for other managers
    //
    // These are easily accessbile values for other managers to use
    public bool IsCurrentSectionCompleted(int checkSectionNum) {
        bool isCompleted = false;
        try {
            isCompleted = currentSceneData.numSectionsComplete > checkSectionNum;
        }
        catch (IndexOutOfRangeException) {
            print("Level " + checkSectionNum + "status unknown");
        }

        return isCompleted;
    }
    public bool IsLevelCompleted(int checkWorldNumber = -1, int checkLevelNumber = -1) {
        if (checkLevelNumber == -1) {
            checkLevelNumber = GetCurrentLevelNumber();
        }
        if (checkWorldNumber == -1) {
            checkWorldNumber = GetCurrentWorldNumber();
        }
        //print($"{string.Join("", saveData.levelCompletionStatusList)}");
        bool isCompleted = false;
        try {
            isCompleted = globalData.IsLevelCompleted(checkWorldNumber, checkLevelNumber);
        }
        catch (IndexOutOfRangeException) {
            print("Level " + checkLevelNumber + " of world " + checkWorldNumber + " status unknown");
        }

        return isCompleted;
    }

    public VelocityPosition GetMarbleStartForSection(int sectionNum) {
        //Debug.Log($"currentSceneData is null: {currentSceneData == null}");
        //Debug.Log($"Velocity dictionary is null: {currentSceneData.sectionStartMarbleVelocities == null}");

        VelocityPosition returnVal = new() {
            velocity = VectorUtils.nullVector,
            position = VectorUtils.nullVector
        };

        try {
            returnVal.velocity = currentSceneData.sectionStartMarbleVelocities[sectionNum];
            returnVal.position = currentSceneData.sectionStartMarblePositions[sectionNum];
        }
        catch {
            print("Could not find marble data for section " + sectionNum);
        }

        return returnVal;
    }

    public Vector3 GetPositionForInteractable(string id) {
        Vector3 returnVal = VectorUtils.nullVector;
        try {
            returnVal = currentSceneData.interactablePositions[id];
        } catch {
            print("not found");
        }

        return returnVal;
    }
}