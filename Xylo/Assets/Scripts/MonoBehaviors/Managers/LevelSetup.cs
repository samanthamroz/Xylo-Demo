using System.Collections;
using UnityEditor;
using UnityEngine;

public class LevelSetup : MonoBehaviour
{
    public LevelSetup self;

    void Awake() {
        if (self == null) {
            self = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    public static void SetupTitle(bool firstTime) {
        ControlsManager.self.InitializeActionMap("levelselect");
        CameraManager.self.InstantiateTitleCamera();

        if (firstTime) {
            GUIManager.self.InstantiateTitleUI(true);
            return;
        }

        GUIManager.self.InstantiateTitleUI(false);
        CameraManager.self.SwitchTitleScreenPosition("level0");
        GUIManager.self.LoadMiddleToRight(.25f);
    }

    public static void SetupTutorial() {
        ControlsManager.self.InitializeActionMap("levelmenus");
        GUIManager.self.InstantiateLevelUI(true);

        DoSceneStart();
    }

    public static void SetupLevel(int worldNum, int levelNum) {
        ControlsManager.self.InitializeActionMap("main");
        GUIManager.self.InstantiateLevelUI(false);

        DoSceneStart();
    }

    private static void DoSceneStart() {
        CameraManager.self.InstantiateCamera();
        GUIManager.self.LoadMiddleToRight(.25f);
        
        // Start coroutine to restore positions after Awake
        LoadingManager.self.StartCoroutine(RestoreInteractablePositions());
    }

    private static IEnumerator RestoreInteractablePositions() {
        // Wait one frame to ensure all Awake() methods have run
        yield return null;
        
        var foundInteractables = FindObjectsByType<DraggableInteractable>(FindObjectsSortMode.None);
        foreach (var found in foundInteractables) {
            int foundId = found.uniqueId;
            Vector3 newPos = LoadingManager.self.GetPositionForInteractable(foundId);
            if (newPos != VectorUtils.nullVector) {
                found.transform.position = newPos;
            }
        }
    }
}