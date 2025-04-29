using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectIslandGrapics : MonoBehaviour
{
    public Sprite greyedImage, originalImage; 
    public int unlockWhenLevelCompleted;
    // Start is called before the first frame update
    void Start()
    {
        if (LoadingManager.self.IsLevelCompleted(unlockWhenLevelCompleted)) {
            GetComponent<Image>().sprite = originalImage;
        } else {
            GetComponent<Image>().sprite = greyedImage;
        }
    }
}
