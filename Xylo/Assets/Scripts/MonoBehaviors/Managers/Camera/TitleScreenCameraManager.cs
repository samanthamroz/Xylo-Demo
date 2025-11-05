using UnityEngine;
using System.Collections.Generic;

public partial class CameraManager {
    private class TitleScreenCameraManager {
        private readonly Dictionary<string, Vector3> levelSelectCameraPositions = new() {
            {"title", new Vector3(.03f, 50f, 11.3f)},
            {"credits", new Vector3(32.1f, -3.75f, -44.7f)},
            {"level0", new Vector3(.03f, -0.25f, 11.3f)}
        };
        public void ReturnToTitle(float time = 0f) {
            self.lookAtPointResetPos = levelSelectCameraPositions["title"];
            self.cameraHeight = 0f;
            self.startingZoom = 25f;
            self.currentZoom = self.startingZoom;
            self.zoomGoal = self.currentZoom;

            self.StartCoroutine(self.PlaceCamera(time, true));
        }

        public void MoveToIsland(string key) {
            self.currentlookAtObject.transform.position = levelSelectCameraPositions[key];
            self.currentlookAtObject.transform.eulerAngles = new Vector3(0, 0, 0);
            self.StartCoroutine(self.PlaceCamera(1f));
        }
    }
}