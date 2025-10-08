using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class HandleGraphicsHolder : MonoBehaviour {
    [SerializeField] private Material regularMaterial, greyedMaterial;
    public void Initialize() {
        SetActive(false);
    }
    public void SetGrey(bool isGreyed) {
        if (isGreyed) {
            GetComponent<MeshRenderer>().material = greyedMaterial;
        } else {
            GetComponent<MeshRenderer>().material = regularMaterial;
        }
    }
    public void SetActive(bool isActive) {
        if (isActive) {
            GetComponent<MeshRenderer>().enabled = true;
        } else {
            GetComponent<MeshRenderer>().enabled = false;
        }
    }
}