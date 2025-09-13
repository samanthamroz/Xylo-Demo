using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GUIManager : MonoBehaviour {
	public static GUIManager self;

	[SerializeField] private GameObject loadingBannerPrefab, titleScreenCanvasPrefab, levelSelectCanvasPrefab;
	private GameObject titleScreenCanvas, levelSelectCanvas;

	[SerializeField] private GameObject UICanvasPrefab, pauseMenuPrefab, pianoMenuPrefab, playButtonPrefab, winMenuCanvasPrefab, tutorialBoxPrefab, cinematicBarsPrefab;
	private GameObject UICanvas, pauseMenu, pianoMenu, playButton, winMenuCanvas, tutorialBox, cinematicBarsCanvas;
	
	private Vector3 pianoMenuStartPos;
	[SerializeField] private Sprite playButtonImage, retryButtonImage;

	void Awake() {
		if (self == null) {
			self = this;
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
		}
	}

	public void InstantiateTitleUI(bool keepOpen) {
		titleScreenCanvas = Instantiate(titleScreenCanvasPrefab);
		levelSelectCanvas = Instantiate(levelSelectCanvasPrefab);

		if (keepOpen) {
			levelSelectCanvas.SetActive(false);
		}
		else {
			titleScreenCanvas.SetActive(false);
		}
	}
	public void InstantiateLevelUI(bool isTutorial) {
		UICanvas = Instantiate(UICanvasPrefab);

		pianoMenu = Instantiate(pianoMenuPrefab, UICanvas.transform);
		ActivatePiano(false, 0f);

		playButton = Instantiate(playButtonPrefab, UICanvas.transform);

		if (isTutorial) {
			for (int i = 0; i < UICanvas.transform.childCount; i++) {
				UICanvas.transform.GetChild(i).gameObject.SetActive(false);
			}
			tutorialBox = Instantiate(tutorialBoxPrefab, UICanvas.transform);
		}

		//pause menu has to be instantiated last so that it covers the other buttons when paused
		pauseMenu = Instantiate(pauseMenuPrefab, UICanvas.transform);
		pauseMenu.SetActive(false);

		winMenuCanvas = Instantiate(winMenuCanvasPrefab);
		winMenuCanvas.SetActive(false);

		cinematicBarsCanvas = Instantiate(cinematicBarsPrefab);
		ActivateCinematicBars(false, 0f);
	}

	public void ToggleTitleScreen() {
		titleScreenCanvas.SetActive(!titleScreenCanvas.activeSelf);
		levelSelectCanvas.SetActive(!levelSelectCanvas.activeSelf);
	}
	public void TogglePianoPosition() {
		ActivatePiano(pianoMenu.transform.localPosition.y == (-60f + -(Screen.height / 2)), .1f);
	}
	public void ActivatePiano(bool turnOn, float animationTime) {
		if (turnOn) {
			LeanTween.moveLocalY(pianoMenu, 200f + -(Screen.height / 2), animationTime).setEaseInOutSine();
		}
		else {
			LeanTween.moveLocalY(pianoMenu, -60f + -(Screen.height / 2), animationTime).setEaseInOutSine();
		}
	}
	private void ActivateCinematicBars(bool turnOn, float animationTime) {
		GameObject top = cinematicBarsCanvas.transform.GetChild(0).gameObject;
		GameObject bottom = cinematicBarsCanvas.transform.GetChild(1).gameObject;

		if (turnOn) {
			LeanTween.moveLocalY(top, Screen.height / 2 - 75, animationTime).setEaseInOutSine().setIgnoreTimeScale(true); ;
			LeanTween.moveLocalY(bottom, -(Screen.height / 2) + 75, animationTime).setEaseInOutSine().setIgnoreTimeScale(true); ;
			LeanTween.moveLocalY(playButton, playButton.transform.localPosition.y + 150f, animationTime).setEaseInOutSine().setIgnoreTimeScale(true); ;
		}
		else {
			LeanTween.moveLocalY(top, Screen.height / 2 + 75, animationTime).setEaseInOutSine().setIgnoreTimeScale(true); ;
			LeanTween.moveLocalY(bottom, -(Screen.height / 2) - 75, animationTime).setEaseInOutSine().setIgnoreTimeScale(true); ;
			LeanTween.moveLocalY(playButton, playButton.transform.localPosition.y - 150f, animationTime).setEaseInOutSine().setIgnoreTimeScale(true); ;
		}
	}
	private void SetPianoActive(bool isActive) {
		pianoMenu.SetActive(isActive);
	}
	public void TogglePause() {
		pauseMenu.SetActive(!pauseMenu.activeSelf);
		if (pauseMenu.activeSelf) {
			ControlsManager.self.ActivateMenuMap();
		}
		else {
			ControlsManager.self.RevertToLastMap();
		}

		if (CameraManager.self.isCinematicCamera) {
			//ControlsManager.self.ActivateCinematicMap();
			ActivateCinematicBars(!pauseMenu.activeSelf, .25f);
		}

		ControlsManager.self.PauseGameTime(pauseMenu.activeSelf);
	}
	public void ActivateLevelUI() {
		GameObject curr;
		Vector3 endingPosition;
		for (int i = 0; i < UICanvas.transform.childCount; i++) {
			curr = UICanvas.transform.GetChild(i).gameObject;
			curr.SetActive(true);
			endingPosition = curr.transform.position;
			if (curr == pauseMenu) {
				curr.SetActive(false);
			}
			else if (curr == pianoMenu) {
				curr.transform.Translate(0, -250f, 0);
				LeanTween.move(curr, endingPosition, .25f);
			}
			else if (curr == playButton) {
				curr.transform.Translate(0, -200f, 0);
				LeanTween.move(curr, endingPosition, .25f);
			}
			else { //pause button
				curr.transform.Translate(0, 150f, 0);
				LeanTween.move(curr, endingPosition, .25f);
			}
		}

		CameraManager.self.ManualZoom(-5f);
	}

	public IEnumerator ActivateCinematicUI(float animationTime = .5f) {
		ActivatePiano(false, animationTime);
		ActivateCinematicBars(true, animationTime);
		yield return new WaitForSeconds(animationTime);
		SetPianoActive(false);
	}
	public IEnumerator DeactivateCinematicUI(float animationTime = .5f) {
		SetPianoActive(true);
		ActivateCinematicBars(false, animationTime);
		yield return new WaitForSeconds(animationTime);
	}
	public void ActivateWinMenuUI() {
		UICanvas.SetActive(false);
		winMenuCanvas.SetActive(true);
		ControlsManager.self.ActivateMenuMap();
	}

	public void TogglePlayButtonImage(bool toPlayButton) {
		if (toPlayButton) {
			playButton.GetComponent<Image>().sprite = playButtonImage;
		}
		else {
			playButton.GetComponent<Image>().sprite = retryButtonImage;
		}
	}

	public void LoadLeftToMiddle(float time) {
		GameObject loadingBanner = Instantiate(loadingBannerPrefab).transform.GetChild(0).gameObject;
		loadingBanner.transform.position = new Vector3(-2210f, loadingBanner.transform.position.y, loadingBanner.transform.position.z);
		LeanTween.moveLocalX(loadingBanner, 0f, time).setEaseInOutSine();
	}
	public void LoadMiddleToRight(float time) {
		GameObject loadingBanner = Instantiate(loadingBannerPrefab).transform.GetChild(0).gameObject;
		LeanTween.moveLocalX(loadingBanner, 2210f, time).setEaseInOutSine();
		Destroy(loadingBanner, time);
	}
}