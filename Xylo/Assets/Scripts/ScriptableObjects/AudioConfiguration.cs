using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Xylo/Audio Configuration")]
public class AudioConfiguration : ScriptableObject {
    [System.Serializable]
    public class AudioData {
        [Header("Snippets")]
        public AudioClipList sectionClips;
    }
    
    public AudioData[] levels;
    
    public AudioData GetLevelAudioData(int levelIndex) {
        if (levelIndex < 0 || levelIndex >= levels.Length) {
            Debug.LogError($"Level index {levelIndex} out of range");
            return null;
        }
        return levels[levelIndex];
    }
}