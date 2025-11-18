using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class LoadingManager : MonoBehaviour {
    public static LoadingManager self;
    private static bool hasTitleLoaded = false;
    private GlobalSaveData globalData;
    private SceneSaveData currentSceneData;
    [SerializeField] private bool DEBUG_AlwaysResetData = false;
    [SerializeField] private int DEBUG_OverrideCurrentLevelNum = -1;
    private int currentLevelNumber = 0;
    private const int NUM_LEVELS_IN_WORLD_ONE = 2;
    private const int NUM_LEVELS_IN_WORLD_TWO = 0;
    private const int NUM_LEVELS_IN_WORLD_THREE = 0;

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
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
    
    // Loading Data functions
    // These read the data from disk and save to useable variable
    // LoadCurrentScene is triggered on Awake
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

        if (scene.buildIndex == 0) { //for title only 
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
        //save to file
        var saveProfile = new SaveProfile<SceneSaveData>(currentSceneData, currentSceneData.scene.name);
        SaveManager.Save(saveProfile);
    }
    private void SaveGlobal() {
        SaveCurrentScene();
        SaveManager.Save(new SaveProfile<GlobalSaveData>(globalData));
    }

    // Writing Functions
    //
    // These change values in the save data and then write it to disk
    public void SetCurrentSectionCompleted(int sectionNum) {
        //increment number of sections completed
        if (sectionNum == currentSceneData.numSectionsComplete) {
            currentSceneData.numSectionsComplete++;
            globalData.SetSectionCompleted(GetCurrentWorldNumber(), currentLevelNumber, sectionNum);
        }

        SaveCurrentScene();

        //set level complete if all sections are done
        if (globalData.GetTotalSectionsInLevel(GetCurrentWorldNumber(), currentLevelNumber) == globalData.GetNumCompletedSections(GetCurrentWorldNumber(), currentLevelNumber)) {
            globalData.SetLevelCompleted(GetCurrentWorldNumber(), currentLevelNumber);
        }

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
            isCompleted = globalData.IsLevelCompleted(GetCurrentWorldNumber(), currentLevelNumber);
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
}