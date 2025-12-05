using UnityEngine;

public class LightingManager : MonoBehaviour
{
    [HideInInspector] public LightingManager self;
    [SerializeField] private GameObject[] lightSources;
    void Awake() {
		if (self == null) {
			self = this;
		}
	}
    void Start() {
        foreach (GameObject g in lightSources) {
            g.SetActive(false);
        }
        lightSources[LoadingManager.self.GetCurrentLevelNumber()].SetActive(true);
    }
}