using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Autosave : MonoBehaviour
{
    void Start()
    {
        LoadRoom();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) {
            LoadRoom();
        }

        if (Input.GetKeyDown(KeyCode.V)) {
            SaveRoom();
        }
    }

    public static void SaveRoom() {
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

    public static void LoadRoom() {
        var saveData = SaveManager.Load<SceneSaveData>(SceneManager.GetActiveScene().name).saveData;
    }
}