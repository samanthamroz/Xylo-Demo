using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager self;

    void Awake() {
		if (self == null) {
			self = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}

		SceneManager.sceneLoaded += LoadCurrentScene;
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

	public void LoadNewScene(string sceneName) {
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
		Debug.Log(scene.name);
        var saveData = SaveManager.Load<SceneSaveData>(scene.name).saveData;
    }
}