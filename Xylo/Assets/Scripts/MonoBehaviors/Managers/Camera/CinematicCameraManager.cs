using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public partial class CameraManager
{
    private class CinematicCameraManager {
        private readonly Vector3[] sectionCinematicViewPoints;
        private readonly Vector3[] sectionGameViewPoints;

        public CinematicCameraManager(Vector3[] sectionCinematicViewPoints, Vector3[] sectionGameViewPoints) {
            this.sectionCinematicViewPoints = sectionCinematicViewPoints;
            this.sectionGameViewPoints = sectionGameViewPoints;
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
            self.cam.transform.GetPositionAndRotation(out Vector3 originalPosition, out Quaternion originalRotation);
            float timeToStartingPos = .5f;
            float timeToNextPos = 1f;
            float timeBetweenPositions = 3f;
            float timeToWait = timeBetweenPositions - timeToNextPos;

            //Move to starting position
            Vector3 startingPosition = new(0, 15, -15);
            
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

            Vector3[] moveToPositions = { new(26, 14, -15), new(14, 13, -15), new(40, 12, -15) };
            
            foreach (Vector3 position in moveToPositions) {
                self.cam.transform.GetPositionAndRotation(out originalPosition, out originalRotation);

                LeanTween.moveLocal(self.cam.gameObject, position, timeToNextPos).setEaseInOutSine();
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
            self.lookAtObject.transform.position = new Vector3(17.5f, 18, -40);
            LeanTween.moveLocal(self.cam.gameObject, new Vector3(17.5f, 18, -50), 1f).setEaseInOutSine();
            self.SetCameraMode(CamMode.NORMAL);
        }
    }
}