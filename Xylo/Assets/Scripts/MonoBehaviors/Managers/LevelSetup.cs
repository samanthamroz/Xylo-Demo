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
        
        if (firstTime) {
            GUIManager.self.InstantiateTitleUI(true);
            CameraManager.self.InstantiateTitleCamera();
            return;
        }

        GUIManager.self.InstantiateTitleUI(false);
        DoSceneStart();
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
        CameraManager.self.InstantiateCamera(LoadingManager.self.GetCurrentLevelNumber());
        GUIManager.self.LoadMiddleToRight(.25f);
    }
}