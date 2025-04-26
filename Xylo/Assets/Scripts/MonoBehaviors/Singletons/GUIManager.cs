using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GUIManager : MonoBehaviour
{
    public static GUIManager self;
	public GameObject UICanvasPrefab, pauseMenuPrefab, pianoMenuPrefab, playButtonPrefab;
	private GameObject UICanvas, pauseMenu, pianoMenu, playButton;
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
		pauseMenu.SetActive(!pauseMenu.activeSelf);
	}

	public void TogglePlay() {
		
	}

	public void TogglePlayButtonImage(bool toPlayButton) {
		if (toPlayButton) {
            playButton.GetComponent<Image>().sprite = playButtonImage;
        } else {
            playButton.GetComponent<Image>().sprite = retryButtonImage;
        }
	}
}