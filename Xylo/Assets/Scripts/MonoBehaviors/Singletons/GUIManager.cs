using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GUIManager : MonoBehaviour
{
    public static GUIManager self;

	public GameObject loadingBannerPrefab, titleScreenCanvasPrefab, levelSelectCanvasPrefab;
	private GameObject titleScreenCanvas, levelSelectCanvas;

	public GameObject UICanvasPrefab, pauseMenuPrefab, pianoMenuPrefab, playButtonPrefab, winMenuCanvasPrefab, tutorialBoxPrefab;
	private GameObject UICanvas, pauseMenu, pianoMenu, playButton, winMenuCanvas, tutorialBox;
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

	public void InstantiateTitleUI(bool keepOpen) {
		titleScreenCanvas = Instantiate(titleScreenCanvasPrefab);
		levelSelectCanvas = Instantiate(levelSelectCanvasPrefab);

		if (keepOpen) {
			levelSelectCanvas.SetActive(false);
		} else {
			titleScreenCanvas.SetActive(false);
		}
	}
	public void InstantiateLevelUI(bool isTutorial) {
		UICanvas = Instantiate(UICanvasPrefab);
		winMenuCanvas = Instantiate(winMenuCanvasPrefab);
		winMenuCanvas.SetActive(false);
		pianoMenu = Instantiate(pianoMenuPrefab, UICanvas.transform);
		pianoMenuStartPos = pianoMenu.transform.position;
		pauseMenu = Instantiate(pauseMenuPrefab, UICanvas.transform);
		pauseMenu.SetActive(false);
		playButton = Instantiate(playButtonPrefab, UICanvas.transform);
		
		if (isTutorial) {
			tutorialBox = Instantiate(tutorialBoxPrefab, UICanvas.transform);
		}
	}

	public void ToggleTitleScreen() {
		titleScreenCanvas.SetActive(!titleScreenCanvas.activeSelf);
		levelSelectCanvas.SetActive(!levelSelectCanvas.activeSelf);
	}
	public void TogglePianoPosition() {
		if (pianoMenu.transform.position == pianoMenuStartPos) {
			pianoMenu.transform.Translate(0, 250, 0);
		} else {
			pianoMenu.transform.position = pianoMenuStartPos;
		}
    }
	public void TogglePiano() {
		pauseMenu.SetActive(!pauseMenu.activeSelf);
	}
	public void TogglePause() {
		ControlsManager.self.ToggleMenuActionMap();
		pauseMenu.SetActive(!pauseMenu.activeSelf);
	}
	public void ToggleWinMenu() {
		UICanvas.SetActive(false);
		winMenuCanvas.SetActive(!winMenuCanvas.activeSelf);
		ControlsManager.self.ToggleMenuActionMap();
	}

	public void TogglePlayButtonImage(bool toPlayButton) {
		if (toPlayButton) {
            playButton.GetComponent<Image>().sprite = playButtonImage;
        } else {
            playButton.GetComponent<Image>().sprite = retryButtonImage;
        }
	}

	public void LoadLeftToMiddle(float time) {
		GameObject loadingBanner = Instantiate(loadingBannerPrefab).transform.GetChild(0).gameObject;
		loadingBanner.transform.position = new Vector3(-2210f, loadingBanner.transform.position.y, loadingBanner.transform.position.z);
		LeanTween.moveLocalX(loadingBanner, 0f, time);
	}
	public void LoadMiddleToRight(float time) {
		GameObject loadingBanner = Instantiate(loadingBannerPrefab).transform.GetChild(0).gameObject;
		LeanTween.moveLocalX(loadingBanner, 2210f, time);
		Destroy(loadingBanner, time);
	}
}