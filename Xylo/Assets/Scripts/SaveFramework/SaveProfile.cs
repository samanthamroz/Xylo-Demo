using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SaveProfile<T> where T : SaveProfileData {
    public string name;
    public T saveData;

    private SaveProfile() { }

    public SaveProfile(T saveData, string name = "global") {
        this.name = name;
        this.saveData = saveData;
    }
}

public abstract record SaveProfileData { }

public record GlobalSaveData : SaveProfileData {
    private const int numLevels = 1;
    public readonly int[] numSectionsInLevel = { 4 };
    public bool[] levelCompletionStatusList = new bool[numLevels];
}

public record SceneSaveData : SaveProfileData {
    public Scene scene;
    public int numSectionsComplete = 0;
    public Dictionary<int, Vector3> sectionStartMarbleVelocities = new();
    public Dictionary<int, Vector3> sectionStartMarblePositions = new();
}