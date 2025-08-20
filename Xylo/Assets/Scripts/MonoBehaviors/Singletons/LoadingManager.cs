using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager self;
    public static bool hasTitleLoaded = false;
    public GlobalSaveData saveData { get { return SaveManager.Load<GlobalSaveData>().saveData;} }
    public SceneSaveData sceneData;

    void Awake() {
		if (self == null) {
			self = this;
            if (!SaveManager.GameDataExists()) {
                GlobalSaveData newSave = new();
                SaveManager.Save(new SaveProfile<GlobalSaveData>(newSave));
            }
			SceneManager.sceneLoaded += LoadCurrentScene;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }
    public int GetCurrentLevelNumber()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }
    private void LoadCurrentScene(Scene scene, LoadSceneMode mode)
    {

        if (scene.buildIndex == 0)
        { //for title only 
            ControlsManager.self.InitializeActionMap("levelselect");
            if (!hasTitleLoaded)
            {
                GUIManager.self.InstantiateTitleUI(true);
                CameraManager.self.SetCameraMode(CamMode.TITLESCREEN);
                CameraManager.self.InstantiateTitleCamera();
                hasTitleLoaded = true;
                return;
            }
            else
            {
                GUIManager.self.InstantiateTitleUI(false);
            }
        }
        if (scene.buildIndex == 1)
        { //for tutorial only
            ControlsManager.self.InitializeActionMap("levelmenus");
            GUIManager.self.InstantiateLevelUI(true);
            AudioManager.self.LoadSounds(SceneManager.GetActiveScene().buildIndex);
        }
        if (scene.buildIndex > 1)
        {
            ControlsManager.self.InitializeActionMap("main");
            GUIManager.self.InstantiateLevelUI(false);
            AudioManager.self.LoadSounds(SceneManager.GetActiveScene().buildIndex);
        }
        CameraManager.self.SetCameraMode(CamMode.NORMAL);
        CameraManager.self.InstantiateCamera(scene.buildIndex);

        try
        {
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
        var saveProfile  = new SaveProfile<SceneSaveData>(sceneSave, sceneSave.scene.name);
        SaveManager.Save(saveProfile);
    }

    public void SetCurrentSectionCompleted(int sectionNum)
    {
        GlobalSaveData temp;
        try {
            temp = SaveManager.Load<GlobalSaveData>().saveData;
            temp.levelCompletionStatusList[GetCurrentLevelNumber() - 1] = true;
        } catch {
            temp = new GlobalSaveData {};
            temp.levelCompletionStatusList[GetCurrentLevelNumber() - 1] = true;
        }
        
        //set level complete if all sections are done
        bool levelComplete = true;
        for (int i = 0; i < temp.sectionCompletionStatusList[GetCurrentLevelNumber() - 1].Count; i++) {
            if (!temp.sectionCompletionStatusList[GetCurrentLevelNumber() - 1][i]) {
                levelComplete = false;
            }
        }
        if (levelComplete) {
            temp.levelCompletionStatusList[GetCurrentLevelNumber() - 1] = true;
        }

        SaveManager.Save(new SaveProfile<GlobalSaveData>(temp));
    }

    public bool IsLevelCompleted(int checkLevelNumber)
    {
        bool isCompleted = false;
        try
        {
            isCompleted = saveData.levelCompletionStatusList[checkLevelNumber];
        }
        catch (ArgumentOutOfRangeException)
        {
            print("Level status unknown");
        }

        return isCompleted;
    }
}