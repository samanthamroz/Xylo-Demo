using UnityEngine;

[CreateAssetMenu(fileName = "CameraConfig", menuName = "Xylo/Camera Configuration")]
public class CameraConfiguration : ScriptableObject {
    [System.Serializable]
    public class CameraData {
        [Header("Instantiation")]
        public Vector3 startingPoint;
        public float startingHeight;
        public float startingZoom;
        public float zoomRange;
        public Vector3 startingRotation;

        [Header("Camera Positions")]
        public Vector3[] sectionCinematicViewPoints;
        public Vector3[] sectionGameViewPoints;

        [Header("Cinematic Stuff")]
        public float timeToStartingPos = 1f;
        public float totalTimeOfSection = 12f;
    }
    
    public CameraData[] levels;
    
    public CameraData GetLevelCameraData(int levelIndex) {
        if (levelIndex < 0 || levelIndex >= levels.Length) {
            Debug.LogError($"Level index {levelIndex} out of range");
            return null;
        }
        return levels[levelIndex];
    }
}