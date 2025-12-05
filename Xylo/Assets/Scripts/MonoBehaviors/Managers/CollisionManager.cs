using System;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour {
    [System.Serializable]
    public class LevelCollisionData {
        public string sectionName;
        public GameObject[] objectsToDisable;
    }
    [System.Serializable]
    public class WorldCollisionData {
        public string levelName;
        public LevelCollisionData[] levelCollisionDatas;
    }

    [SerializeField] private WorldCollisionData[] worldCollisionDatas;
    private LevelCollisionData[] sections { get { return worldCollisionDatas[LoadingManager.self.GetCurrentLevelNumber()].levelCollisionDatas; } }
    public static CollisionManager self;

    void Awake() {
        if (self == null) {
            self = this;
        }
    }

    private void TurnOnAllCollisions() {
        foreach (var section in sections) {
            foreach (GameObject g in section.objectsToDisable) {
                if (g != null) g.SetActive(true);
            }
        }
    }

    public void TurnOffCollisionForPuzzle(int sectionNum) {
        if (sectionNum < 0 || sectionNum >= sections.Length) {
            Debug.LogWarning($"Section {sectionNum} out of range");
            return;
        }

        TurnOnAllCollisions();
        foreach (GameObject g in sections[sectionNum].objectsToDisable) {
            if (g != null) g.SetActive(false);
        }
    }

    public void TurnOffAllCollision() {
        foreach (var section in sections) {
            foreach (GameObject g in section.objectsToDisable) {
                if (g != null) g.SetActive(false);
            }
        }
    }
}