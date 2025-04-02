using UnityEngine;
using UnityEngine.InputSystem;

public class MouseManager : MonoBehaviour
{
    public static MouseManager self;
    public Vector3 mousePosition;

    void Awake() {
		if (self == null) {
			self = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }

    void OnMouseMove(InputValue value) {
        mousePosition = value.Get<Vector2>();
    }
}