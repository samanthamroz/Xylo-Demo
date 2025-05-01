using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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
                GlobalSaveData newSave = new()
                {
                    levelCompletionStatusList = new List<bool>()
                };
                SaveManager.Save(new SaveProfile<GlobalSaveData>(newSave));
            }
			SceneManager.sceneLoaded += LoadCurrentScene;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }
    private void LoadCurrentScene(Scene scene, LoadSceneMode mode) {
        ControlsManager.self.InitializeActionMap(scene.buildIndex == 0);
        
        if (scene.buildIndex == 0) { //for title only 
            if (!hasTitleLoaded) {
                print("test");
                GUIManager.self.InstantiateTitleUI();
                CameraManager.self.InstantiateTitleCamera();
                hasTitleLoaded = true;
                return;
            } else {
                
            }
        }
        if (scene.buildIndex == 1) { //for tutorial only
            GUIManager.self.InstantiateLevelUI(true);
            AudioManager.self.LoadSounds(SceneManager.GetActiveScene().buildIndex);
        }
        if (scene.buildIndex > 1) {
			GUIManager.self.InstantiateLevelUI(false);
            AudioManager.self.LoadSounds(SceneManager.GetActiveScene().buildIndex);
		}
        CameraManager.self.InstantiateCamera(scene.buildIndex);

        try {
            sceneData = SaveManager.Load<SceneSaveData>(scene.name).saveData;
        } catch { }
    }
	public void LoadNewScene(string sceneName) {
		SaveCurrentScene();
		SceneManager.LoadScene(sceneName);
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
    
    public void SetLevelCompleted(int levelNumber) {
        GlobalSaveData temp;
        try {
            temp = SaveManager.Load<GlobalSaveData>().saveData;
            temp.levelCompletionStatusList.Insert(levelNumber, true);
        } catch {
            temp = new GlobalSaveData {
                levelCompletionStatusList = new()
            };
            temp.levelCompletionStatusList.Insert(levelNumber, true);
        }
        SaveManager.Save(new SaveProfile<GlobalSaveData>(temp));
    }

    public bool IsLevelCompleted(int checkLevelNumber) {
        bool isCompleted = false;
        try {
            isCompleted = saveData.levelCompletionStatusList[checkLevelNumber];
        } catch (ArgumentOutOfRangeException) { 
            print("Level status unknown"); 
        }
        
        return isCompleted;
    }
}