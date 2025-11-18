using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class NextLevelButton : MonoBehaviour {
    private Button thisButton;
    void Awake() {
        thisButton = GetComponent<Button>();
    }

    void OnEnable()
    {
        //Register Button Events
        thisButton.onClick.AddListener(() => Load());
    }

    private void Load() {
        LoadingManager.self.LoadNextLevel(true);
    }

    void OnDisable()
    {
        //Un-Register Button Events
        thisButton.onClick.RemoveAllListeners();
    }
}