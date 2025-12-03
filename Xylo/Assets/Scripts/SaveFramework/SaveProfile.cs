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
    // World → Level → Section structure
    // World 0: { Level 0: 4 sections, Level 1: 3 sections, Level 2: 5 sections }
    // World 1: { Level 0: 6 sections, Level 1: 4 sections }
    // World 2: { Level 0: 3 sections, Level 1: 7 sections, Level 2: 4 sections }
    public readonly int[][] sectionsPerLevel = new int[][] {
        new int[] { 4, 13 }//,  // World 0
        //new int[] { -1, -1 },     // World 1
        //new int[] { -1, -1, -1 }   // World 2
    };

    // Tracks completion status for each section
    // completedSections[worldNum][levelNum] = hashset of completed section numbers
    public Dictionary<int, Dictionary<int, HashSet<int>>> completedSections = new();

    // Tracks which levels are fully completed (all sections done)
    // completedLevels[worldNum] = hashset of completed level numbers
    public Dictionary<int, HashSet<int>> completedLevels = new();

    public GlobalSaveData() {
        // Initialize the nested dictionaries and hashsets
        for (int w = 0; w < sectionsPerLevel.Length; w++) {
            completedSections[w] = new Dictionary<int, HashSet<int>>();
            completedLevels[w] = new HashSet<int>();
            
            for (int l = 0; l < sectionsPerLevel[w].Length; l++) {
                completedSections[w][l] = new HashSet<int>();
            }
        }
    }

    // Helper methods for easier access
    public bool IsSectionCompleted(int worldNum, int levelNum, int sectionNum) {
        return completedSections.ContainsKey(worldNum) &&
               completedSections[worldNum].ContainsKey(levelNum) &&
               completedSections[worldNum][levelNum].Contains(sectionNum);
    }

    public bool IsLevelCompleted(int worldNum, int levelNum) {
        return completedLevels.ContainsKey(worldNum) &&
               completedLevels[worldNum].Contains(levelNum);
    }

    public void SetSectionCompleted(int worldNum, int levelNum, int sectionNum) {
        if (!completedSections.ContainsKey(worldNum)) {
            completedSections[worldNum] = new Dictionary<int, HashSet<int>>();
        }
        if (!completedSections[worldNum].ContainsKey(levelNum)) {
            completedSections[worldNum][levelNum] = new HashSet<int>();
        }
        
        completedSections[worldNum][levelNum].Add(sectionNum);
        
        // Check if all sections in this level are now completed
        if (completedSections[worldNum][levelNum].Count == sectionsPerLevel[worldNum][levelNum]) {
            SetLevelCompleted(worldNum, levelNum);
        }
    }

    public void SetLevelCompleted(int worldNum, int levelNum) {
        if (!completedLevels.ContainsKey(worldNum)) {
            completedLevels[worldNum] = new HashSet<int>();
        }
        completedLevels[worldNum].Add(levelNum);
    }

    public int GetNumCompletedSections(int worldNum, int levelNum) {
        if (!completedSections.ContainsKey(worldNum) || 
            !completedSections[worldNum].ContainsKey(levelNum)) {
            return 0;
        }
        return completedSections[worldNum][levelNum].Count;
    }

    public int GetTotalSectionsInLevel(int worldNum, int levelNum) {
        if (worldNum >= sectionsPerLevel.Length || 
            levelNum >= sectionsPerLevel[worldNum].Length) {
            return 0;
        }
        return sectionsPerLevel[worldNum][levelNum];
    }
}

public record SceneSaveData : SaveProfileData {
    public Scene scene;
    public int numSectionsComplete = 0;
    public Dictionary<int, Vector3> sectionStartMarbleVelocities = new();
    public Dictionary<int, Vector3> sectionStartMarblePositions = new();
}