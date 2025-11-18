using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ReloadButton : MonoBehaviour {
    private Button thisButton;
    void Awake() {
        thisButton = GetComponent<Button>();
    }

    void OnEnable()
    {
        //Register Button Events
        thisButton.onClick.AddListener(() => Reload());
    }

    private void Reload() {
        LoadingManager.self.ReloadCurrentLevel(true);
    }

    void OnDisable()
    {
        //Un-Register Button Events
        thisButton.onClick.RemoveAllListeners();
    }
}