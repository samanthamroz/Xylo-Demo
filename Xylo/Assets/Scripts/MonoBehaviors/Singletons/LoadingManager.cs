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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) {
            LoadCurrentScene(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        if (Input.GetKeyDown(KeyCode.V)) {
            SaveCurrentScene();
        }
    }

    void OnRestart(InputValue value) {
        if (value.Get<float>() == 1) {
            LoadNewScene(SceneManager.GetActiveScene().name);
        }
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

    public static void LoadCurrentScene(Scene scene, LoadSceneMode mode) {
        try {
            var saveData = SaveManager.Load<SceneSaveData>(scene.name).saveData;
        } catch {}
    }
}