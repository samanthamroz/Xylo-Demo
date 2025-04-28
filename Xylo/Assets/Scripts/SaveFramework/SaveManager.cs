using System.IO;
using System;
using Newtonsoft.Json;
using UnityEngine;

public static class SaveManager {
    private static readonly string saveFolder = Application.persistentDataPath + "/GameData";

    public static void Save<T>(SaveProfile<T> save) where T : SaveProfileData {
        if (File.Exists($"{saveFolder}/{save.name}")) {
            File.Delete($"{saveFolder}/{save.name}");
            //Debug.Log($"Successfully overwrote {saveFolder}/{save.name}");
        }

        var jsonString = JsonConvert.SerializeObject(save, Formatting.Indented, new JsonSerializerSettings{ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
        
        if (!Directory.Exists(saveFolder)) {
            Directory.CreateDirectory(saveFolder); //creates /GameData directory
        }

        File.WriteAllText($"{saveFolder}/{save.name}", jsonString);
    }

    public static SaveProfile<T> Load<T>(string profileName) where T : SceneSaveData {
        if (!File.Exists($"{saveFolder}/{profileName}")) {
            throw new Exception($"Save profile {profileName} not found");
        }

        var fileContents = File.ReadAllText($"{saveFolder}/{profileName}");
        //Debug.Log($"Successfully loaded {saveFolder}/{profileName}");

        return JsonConvert.DeserializeObject<SaveProfile<T>>(fileContents);
    }

    public static SaveProfile<T> Load<T>() where T : GlobalSaveData {
        if (!File.Exists($"{saveFolder}/global")) {
            Save(new SaveProfile<GlobalSaveData>(new GlobalSaveData())); //make a new global save
            if (!File.Exists($"{saveFolder}/global")) {
                throw new Exception($"Save profile still not found");
            }
        }

        var fileContents = File.ReadAllText($"{saveFolder}/global");
        //Debug.Log($"Successfully loaded {saveFolder}/global");

        return JsonConvert.DeserializeObject<SaveProfile<T>>(fileContents);
    }

    public static void Delete(string profileName) {
        if (!File.Exists($"{saveFolder}/{profileName}")) {
            throw new Exception($"Save profile {profileName} not found");
        }

        //Debug.Log($"Successfully deleted {saveFolder}/{profileName}");
        File.Delete($"{saveFolder}/{profileName}");
    }

    public static bool GameDataExists() {
        return Directory.Exists($"{saveFolder}");
    }

    public static void DeleteAll() {
        if (Directory.Exists($"{saveFolder}")) {
            Directory.Delete($"{saveFolder}", true);
        }

        Save(new SaveProfile<GlobalSaveData>(new GlobalSaveData()));
        Debug.Log("Data deleted successfully");
    }
}