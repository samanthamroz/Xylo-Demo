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

public record GlobalSaveData : SaveProfileData
{
    private const int numLevels = 1;
    public bool[] levelCompletionStatusList = new bool[numLevels];
    public List<bool>[] sectionCompletionStatusList = new List<bool>[numLevels];
    public GlobalSaveData() //constructor
    {
        for (int i = 0; i < numLevels; i++)
        {
            sectionCompletionStatusList[i] = new List<bool>();
        }
    }
}

public record SceneSaveData : SaveProfileData {
    public Scene scene;
}