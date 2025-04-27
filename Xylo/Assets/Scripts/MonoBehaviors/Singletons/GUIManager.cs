using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GUIManager : MonoBehaviour
{
    public static GUIManager self;
	public GameObject UICanvasPrefab, pauseMenuPrefab, pianoMenuPrefab, playButtonPrefab, winMenuPrefab;
	private GameObject UICanvas, pauseMenu, pianoMenu, playButton, winMenu;
	private Vector3 pianoMenuStartPos;
	public Sprite playButtonImage, retryButtonImage;

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
		winMenu = Instantiate(winMenuPrefab, UICanvas.transform);
		winMenu.SetActive(false);
		pianoMenu = Instantiate(pianoMenuPrefab, UICanvas.transform);
		pianoMenuStartPos = pianoMenu.transform.position;
		pauseMenu = Instantiate(pauseMenuPrefab, UICanvas.transform);
		pauseMenu.SetActive(false);
		playButton = Instantiate(playButtonPrefab, UICanvas.transform);
	}

	public void TogglePiano() {
		if (pianoMenu.transform.position == pianoMenuStartPos) {
			pianoMenu.transform.Translate(0, 250, 0);
		} else {
			pianoMenu.transform.position = pianoMenuStartPos;
		}
    }

	public void TogglePause() {
		ControlsManager.self.ToggleMenuActionMap();
		pauseMenu.SetActive(!pauseMenu.activeSelf);
	}

	public void ToggleWinMenu() {
		ControlsManager.self.ToggleMenuActionMap();
		winMenu.SetActive(!winMenu.activeSelf);
	}

	public void TogglePlayButtonImage(bool toPlayButton) {
		if (toPlayButton) {
            playButton.GetComponent<Image>().sprite = playButtonImage;
        } else {
            playButton.GetComponent<Image>().sprite = retryButtonImage;
        }
	}
}