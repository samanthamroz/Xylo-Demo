using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager self;

    void Awake() {
		if (self == null) {
			self = this;
			SceneManager.sceneLoaded += LoadCurrentScene;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }
    public static void LoadCurrentScene(Scene scene, LoadSceneMode mode) {
        CameraManager.self.InstantiateCamera(scene.buildIndex);
        ControlsManager.self.InitializeActionMap(scene.buildIndex < 2);
        
        if (scene.buildIndex == 2) { //for tutorial only
            GUIManager.self.InstantiateLevelUI(true);
            AudioManager.self.LoadSounds(SceneManager.GetActiveScene().buildIndex);
        }

        if (scene.buildIndex > 2) {
			GUIManager.self.InstantiateLevelUI(false);
            AudioManager.self.LoadSounds(SceneManager.GetActiveScene().buildIndex);
		}

        try {
            var saveData = SaveManager.Load<SceneSaveData>(scene.name).saveData;
        } catch {}
    }

	public void LoadNewScene(string sceneName) {
		SaveCurrentScene();
		SceneManager.LoadScene(sceneName);
	}

    public static void SaveCurrentScene() {
        //create blank sceneSave
        var sceneSave = new SceneSaveData {
            scene = SceneManager.GetActiveScene(),
            objectPositions = new List<Vector3>(),
            objectRotations = new List<Quaternion>(),
            objectStates = new List<bool>()
        };
        
        //save to file
        var saveProfile  = new SaveProfile<SceneSaveData>(sceneSave, sceneSave.scene.name);
        SaveManager.Save(saveProfile);
    }
}