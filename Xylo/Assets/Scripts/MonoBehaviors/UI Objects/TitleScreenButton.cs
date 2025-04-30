using UnityEngine;

public class TitleScreenButton : MonoBehaviour
{
    public GameObject firstIsland;
    public void ButtonPressed() {
        //the first time the game loads, pressing this button creates a new save
        if (!SaveManager.GameDataExists()) {
            SaveManager.Save(new SaveProfile<GlobalSaveData>(new GlobalSaveData()));
            Debug.Log("New game created");
        }

        //every other time this button is pressed, the user must have come from the level select
        CameraManager.self.SwitchLookAtPosition(firstIsland.transform.position);
        transform.parent.gameObject.SetActive(false);
    }
}
