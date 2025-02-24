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
    }

	void Start()
    {
        LoadCurrentScene();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) {
            LoadCurrentScene();
        }

        if (Input.GetKeyDown(KeyCode.V)) {
            SaveCurrentScene();
        }
    }

	public void LoadNewScene(Scene scene) {
		SceneManager.LoadScene(scene.name);
		LoadCurrentScene();
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

    public static void LoadCurrentScene() {
		Debug.Log(SceneManager.GetActiveScene().name);
        var saveData = SaveManager.Load<SceneSaveData>(SceneManager.GetActiveScene().name).saveData;
    }
}