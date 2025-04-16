using UnityEngine;
using UnityEngine.SceneManagement;

public class GUIManager : MonoBehaviour
{
    public static GUIManager self;
	public GameObject UICanvasPrefab, cameraControllerPrefab, pauseMenuPrefab, pianoMenuPrefab;
	private GameObject UICanvas, pauseMenu, pianoMenu;

	void Awake() {
		if (self == null) {
			self = this;
			SceneManager.sceneLoaded += InstantiateLevelUI;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }

	void Start() {
		
	}

	private void InstantiateLevelUI(Scene scene, LoadSceneMode mode) {
		if (scene.buildIndex > 1) {
			UICanvas = Instantiate(UICanvasPrefab);
			pianoMenu = Instantiate(pianoMenuPrefab, UICanvas.transform);
			pianoMenu.SetActive(false);
			pauseMenu = Instantiate(pauseMenuPrefab, UICanvas.transform);
			pauseMenu.SetActive(false);
		}
	}

	public void TogglePiano() {
        pianoMenu.SetActive(!pianoMenu.activeSelf);
    }

	public void TogglePause() {
		pauseMenu.SetActive(!pauseMenu.activeSelf);
	}
}