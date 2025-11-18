using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LoadSceneButton : MonoBehaviour {
    private Button thisButton;
    [SerializeField] string sceneNameToLoad;
    [SerializeField] int levelNumberToLoad = -1;
    void Awake() {
        thisButton = GetComponent<Button>();
    }

    void OnEnable()
    {
        //Register Button Events
        thisButton.onClick.AddListener(() => Load(sceneNameToLoad, levelNumberToLoad));
    }

    public void Load(string sceneName, int levelNum) {
        ControlsManager.self.PauseGameTime(false);
        if (levelNum != -1) {
            StartCoroutine(LoadingManager.self.LoadNewScene(sceneName, levelNum));
        } else {
            StartCoroutine(LoadingManager.self.LoadNewScene(sceneName));
        }
    }

    void OnDisable()
    {
        //Un-Register Button Events
        thisButton.onClick.RemoveAllListeners();
    }
}