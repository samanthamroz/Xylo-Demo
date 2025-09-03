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
            SceneManager.sceneLoaded += LoadCurrentScene;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
    public int GetCurrentLevelNumber() {
        return SceneManager.GetActiveScene().buildIndex - 1;
    }

    // Loading Data functions
    // These read the data from disk and save to useable variable
    // LoadCurrentScene is triggered on Awake
    private void LoadCurrentScene(Scene scene, LoadSceneMode mode) {
        if (currentSceneData == null) {
            Debug.Log("Creating new scene save");
            SaveManager.Save(
                new SaveProfile<SceneSaveData>(new() { scene = SceneManager.GetActiveScene() },
                SceneManager.GetActiveScene().name));
        }

        if (scene.buildIndex == 0) { //for title only 
            ControlsManager.self.InitializeActionMap("levelselect");
            if (!hasTitleLoaded) {
                GUIManager.self.InstantiateTitleUI(true);
                CameraManager.self.InstantiateTitleCamera();
                hasTitleLoaded = true;
                return;
            }
            else {
                GUIManager.self.InstantiateTitleUI(false);
            }
        }
        if (scene.buildIndex == 1) { //for tutorial only
            ControlsManager.self.InitializeActionMap("levelmenus");
            GUIManager.self.InstantiateLevelUI(true);
        }
        if (scene.buildIndex > 1) {
            ControlsManager.self.InitializeActionMap("main");
            GUIManager.self.InstantiateLevelUI(false);
        }
        CameraManager.self.InstantiateCamera(scene.buildIndex);

        GUIManager.self.LoadMiddleToRight(.25f);
    }
    private void RefreshPointerToSceneData() {
        currentSceneData = SaveManager.Load<SceneSaveData>(SceneManager.GetActiveScene().name).saveData;
    }
    private void RefreshPointerToGlobalData() {
        globalData = SaveManager.Load<GlobalSaveData>().saveData;
    }

    // Loading Scene Functions
    //
    // These take us from the current scene to a different one
    public IEnumerator LoadNewScene(string sceneName) {
        SaveGlobal();
        float time = .25f;
        GUIManager.self.LoadLeftToMiddle(time);

        yield return new WaitForSeconds(time);

        SceneManager.LoadScene(sceneName);
        yield return null; //fixes coroutine running during scene load
    }
    public void ReloadCurrentScene() {
        StartCoroutine(LoadNewScene(SceneManager.GetActiveScene().name));
    }

    // Saving Functions
    // 
    // These write save data to disk as it currently is
    private void SaveCurrentScene() {
        //save to file
        var saveProfile = new SaveProfile<SceneSaveData>(currentSceneData, currentSceneData.scene.name);
        SaveManager.Save(saveProfile);
        RefreshPointerToSceneData();
    }
    private void SaveGlobal() {
        SaveCurrentScene();
        SaveManager.Save(new SaveProfile<GlobalSaveData>(globalData));
        RefreshPointerToGlobalData();
    }

    // Writing Functions
    //
    // These change values in the save data and then write it to disk
    public void SetCurrentSectionCompleted(int sectionNum) {
        //increment number of sections completed
        currentSceneData.numSectionsComplete++;

        SaveCurrentScene();

        //set level complete if all sections are done
        if (currentSceneData.numSectionsComplete == globalData.numSectionsInLevel[GetCurrentLevelNumber()]) {
            globalData.levelCompletionStatusList[GetCurrentLevelNumber()] = true;
        }

        SaveGlobal();
    }
    public void SetMarbleStartForSection(int sectionNum, Vector3 velocity, Vector3 position) {
        currentSceneData.numSectionsComplete++;

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
    public bool IsLevelCompleted(int checkLevelNumber = 99) {
        if (checkLevelNumber == 99) {
            checkLevelNumber = GetCurrentLevelNumber();
        }
        //print($"{string.Join("", saveData.levelCompletionStatusList)}");
        bool isCompleted = false;
        try {
            isCompleted = globalData.levelCompletionStatusList[checkLevelNumber];
        }
        catch (IndexOutOfRangeException) {
            print("Level " + checkLevelNumber + "status unknown");
        }

        return isCompleted;
    }

    public VelocityPosition GetMarbleStartForSection(int sectionNum) {
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