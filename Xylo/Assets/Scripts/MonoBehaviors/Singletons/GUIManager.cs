using UnityEngine;
using UnityEngine.SceneManagement;

public class GUIManager : MonoBehaviour
{
    public static GUIManager self;
	public GameObject UICanvasPrefab, pauseMenuPrefab, pianoMenuPrefab;
	private GameObject UICanvas, pauseMenu, pianoMenu;
	private Vector3 pianoMenuStartPos;

	void Awake() {
		if (self == null) {
			self = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }

	public void InstantiateLevelUI() {
		UICanvas = Instantiate(UICanvasPrefab);
		pianoMenu = Instantiate(pianoMenuPrefab, UICanvas.transform);
		pianoMenuStartPos = pianoMenu.transform.position;
		pauseMenu = Instantiate(pauseMenuPrefab, UICanvas.transform);
		pauseMenu.SetActive(false);
	}

	public void TogglePiano() {
		if (pianoMenu.transform.position == pianoMenuStartPos) {
			pianoMenu.transform.Translate(0, 250, 0);
		} else {
			pianoMenu.transform.position = pianoMenuStartPos;
		}
    }

	public void TogglePause() {
		pauseMenu.SetActive(!pauseMenu.activeSelf);
	}
}