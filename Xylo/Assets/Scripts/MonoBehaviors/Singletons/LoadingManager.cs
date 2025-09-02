using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class LoadingManager : MonoBehaviour {
    public static LoadingManager self;
    public static bool hasTitleLoaded = false;
    public GlobalSaveData saveData { get { return SaveManager.Load<GlobalSaveData>().saveData; } }
    public SceneSaveData sceneData;
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
    private void LoadCurrentScene(Scene scene, LoadSceneMode mode) {
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

        try {
            sceneData = SaveManager.Load<SceneSaveData>(scene.name).saveData;
        }
        catch { }

        GUIManager.self.LoadMiddleToRight(.25f);
    }
    public IEnumerator LoadNewScene(string sceneName) {
        SaveCurrentScene();
        float time = .25f;
        GUIManager.self.LoadLeftToMiddle(time);

        yield return new WaitForSeconds(time);

        SceneManager.LoadScene(sceneName);
        yield return null; //fixes coroutine running during scene load
    }
    public void ReloadCurrentScene() {
        StartCoroutine(LoadNewScene(SceneManager.GetActiveScene().name));
    }
    private void SaveCurrentScene() {
        //create blank sceneSave
        var sceneSave = new SceneSaveData {
            scene = SceneManager.GetActiveScene()
        };

        //save to file
        var saveProfile = new SaveProfile<SceneSaveData>(sceneSave, sceneSave.scene.name);
        SaveManager.Save(saveProfile);
    }

    public void SetCurrentSectionCompleted(int sectionNum) {
        GlobalSaveData temp;
        try {
            temp = SaveManager.Load<GlobalSaveData>().saveData;
            temp.sectionCompletionStatusList[GetCurrentLevelNumber()][sectionNum] = true;
        }
        catch {
            temp = new GlobalSaveData { };
            temp.sectionCompletionStatusList[GetCurrentLevelNumber()][sectionNum] = true;
        }

        //set level complete if all sections are done
        bool levelComplete = true;
        for (int i = 0; i < temp.sectionCompletionStatusList[GetCurrentLevelNumber()].Count; i++) {
            if (!temp.sectionCompletionStatusList[GetCurrentLevelNumber()][i]) {
                levelComplete = false;
            }
        }
        if (levelComplete) {
            temp.levelCompletionStatusList[GetCurrentLevelNumber()] = true;
        }

        SaveManager.Save(new SaveProfile<GlobalSaveData>(temp));
    }

    public bool IsLevelCompleted(int checkLevelNumber = 99) {
        if (checkLevelNumber == 99) {
            checkLevelNumber = GetCurrentLevelNumber();
        }
        //print($"{string.Join("", saveData.levelCompletionStatusList)}");
        bool isCompleted = false;
        try {
            isCompleted = saveData.levelCompletionStatusList[checkLevelNumber];
        }
        catch (IndexOutOfRangeException) {
            print("Level " + checkLevelNumber + "status unknown");
        }

        return isCompleted;
    }
}