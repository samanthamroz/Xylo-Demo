using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public partial class CameraManager
{
    private class CinematicCameraManager {
        private Vector3[] sectionCinematicViewPoints { get { return self.cameraConfiguration.GetLevelCameraData(LoadingManager.self.GetCurrentLevelNumber()).sectionCinematicViewPoints; } }
        private Vector3[] sectionGameViewPoints { get { return self.cameraConfiguration.GetLevelCameraData(LoadingManager.self.GetCurrentLevelNumber()).sectionGameViewPoints; } }

        public CinematicCameraManager() {
            
        }

        public IEnumerator DoSectionView(int sectionNum) {
            self.cam.transform.GetPositionAndRotation(out Vector3 originalPosition, out Quaternion originalRotation);
            float time = .5f;

            LeanTween.moveLocal(self.cameraObject, sectionCinematicViewPoints[sectionNum], time).setEaseInOutSine();

            float elapsed = 0f;
            while (elapsed < time) {
                //what to update here
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / time);
                float easedT = LeanTween.easeInOutSine(0f, 1f, t);



                // Calculate desired rotation toward current lookHere position
                Vector3 direction = (self.currentlookAtObject.transform.position - self.cam.transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Smoothly interpolate rotation
                self.cam.transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, easedT);

                yield return null; // wait for next frame
            }

            while (self.currentlookAtObject != self.lookAtObject) {
                self.currentlookAtObject.transform.LookAt(self.cam.transform);
                self.cam.transform.LookAt(self.currentlookAtObject.transform);
                yield return null;
            }
        }
        public IEnumerator DoMoveToNextSection(int sectionNum) {
            self.cam.transform.GetPositionAndRotation(out Vector3 originalPosition, out Quaternion originalRotation);
            float time = 1f;

            LeanTween.moveLocal(self.lookAtObject, sectionGameViewPoints[sectionNum], time).setEaseInOutSine();
            LeanTween.moveLocal(self.cam.gameObject, self.GetNewCameraPosition(sectionGameViewPoints[sectionNum]), time).setEaseInOutSine();

            float elapsed = 0f;
            while (elapsed < time) {
                //what to update here
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / time);
                float easedT = LeanTween.easeInOutSine(0f, 1f, t);

                // Calculate desired rotation toward current lookHere position
                Vector3 direction = (self.currentlookAtObject.transform.position - self.cam.transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Smoothly interpolate rotation
                self.cam.transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, easedT);


                yield return null; // wait for next frame
            }
            self.lookAtPointResetPos = sectionGameViewPoints[sectionNum];
            self.SetCameraMode(CamMode.NORMAL);
            AudioManager.self.PlayMelodyForCurrentSection();
        }
    
        public IEnumerator DoCinematicLevelOne() {
            self.cam.transform.GetPositionAndRotation(out Vector3 _, out Quaternion originalRotation);

            var data = self.cameraConfiguration.GetLevelCameraData(LoadingManager.self.GetCurrentLevelNumber());

            float timeToStartingPos = data.timeToStartingPos;
            float timeToNextPos = 1f;
            float timeBetweenPositions = data.totalTimeOfSection / sectionCinematicViewPoints.Length;
            float timeToWait = timeBetweenPositions - timeToNextPos;

            //Move to starting position
            
            float sumX = 0, sumY = 0, sumZ = 0;
            foreach(Vector3 point in sectionCinematicViewPoints) {
                sumX += point.x;
                sumY += point.y;
                sumZ += point.z;
            }
            Vector3 startingPosition = new(sumX / sectionCinematicViewPoints.Length, sumY / sectionCinematicViewPoints.Length, sumZ / sectionCinematicViewPoints.Length);
            
            //LeanTween.moveLocal(self.lookAtObject, startingPosition, timeToStartingPos).setEaseInOutSine();
            LeanTween.moveLocal(self.cameraObject, startingPosition, timeToStartingPos).setEaseInOutSine();
            float elapsed = 0f;
            while (elapsed < timeToStartingPos) {
                //what to update here
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / timeToStartingPos);
                float easedT = LeanTween.easeInOutSine(0f, 1f, t);

                // Calculate desired rotation toward current lookHere position
                Vector3 direction = (self.currentlookAtObject.transform.position - self.cam.transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Smoothly interpolate rotation
                self.cam.transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, easedT);


                yield return null; // wait for next frame
            }
            
            elapsed = 0f;
            while (elapsed < timeBetweenPositions - timeToStartingPos + .5f) {
                //what to update here
                elapsed += Time.deltaTime;
                self.currentlookAtObject.transform.LookAt(self.cam.transform);
                self.cam.transform.LookAt(self.currentlookAtObject.transform);
                yield return null;
            }
            
            for (int i = 0; i < sectionCinematicViewPoints.Length; i++) {
                self.cam.transform.GetPositionAndRotation(out _, out originalRotation);

                Vector3 moveBy = (self.cam.transform.position - self.currentlookAtObject.transform.position) * .5f;
                Vector3 moveTo = self.cam.transform.position + moveBy;
                if (LoadingManager.self.GetCurrentLevelNumber() == 0) {
                    moveTo.z = startingPosition.z;
                } else {
                    moveTo.x = startingPosition.x;
                }

                LeanTween.moveLocal(self.cam.gameObject, moveTo, timeToNextPos).setEaseInOutSine();

                elapsed = 0f;
                while (elapsed < timeToNextPos) {
                    //what to update here
                    elapsed += Time.deltaTime;

                    float t = Mathf.Clamp01(elapsed / timeToNextPos);
                    float easedT = LeanTween.easeInOutSine(0f, 1f, t);

                    // Calculate desired rotation toward current lookHere position
                    Vector3 direction = (self.currentlookAtObject.transform.position - self.cam.transform.position).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    // Smoothly interpolate rotation
                    self.cam.transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, easedT);

                    yield return null; // wait for next frame
                }

                elapsed = 0f;
                while (elapsed < timeToWait) {
                    //what to update here
                    elapsed += Time.deltaTime;
                    self.currentlookAtObject.transform.LookAt(self.cam.transform);
                    self.cam.transform.LookAt(self.currentlookAtObject.transform);
                    yield return null;
                }
            }

            GUIManager.self.ActivateWinMenuUI();
            ControlsManager.self.ActivateMenuMap();
            LeanTween.moveLocal(self.cam.gameObject, startingPosition, 1f).setEaseInOutSine();
        }
    }
}