using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Xylo/Level Configuration")]
public class LevelConfiguration : ScriptableObject {
    [System.Serializable]
    public class LevelData {
        [Header("Movement")]
        public Vector3 marbleDirection = Vector3.right;
        
        [Header("Starting Position")]
        public Vector3 firstBlockPosition;
        
        [Header("Death Plane Positions")]
        public Vector3[] deathPlaneCoords;

        [Header("Metronome Values")]
        public int Bpm;
        public int timeSigHigh;
        public int timeSigLow;

        [Header("Beat Values")]
        public float xDistancePerBeat;
        public float beatsBetweenFirstTwoBeats;

        [Header("Solution")]
        public NoteTriggerArray[] sectionSolutions;
    }
    
    public LevelData[] levels;
    
    public LevelData GetLevelData(int levelIndex) {
        if (levelIndex < 0 || levelIndex >= levels.Length) {
            Debug.LogError($"Level index {levelIndex} out of range");
            return null;
        }
        return levels[levelIndex];
    }
}