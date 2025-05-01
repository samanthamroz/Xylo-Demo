using UnityEngine;

public class TitleScreenButton : MonoBehaviour
{
    public GameObject firstIsland;
    public void ButtonPressed() {
        //every other time this button is pressed, the user must have come from the level select
        CameraManager.self.SwitchLevelSelectIsland("level0");
        transform.parent.gameObject.SetActive(false);
    }
}
