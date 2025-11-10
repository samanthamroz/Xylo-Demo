using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class OneWayGate : MonoBehaviour, IBlocksPassThrough {
    [SerializeField] private Vector3 _allowedDirection = Vector3.down;
    private Vector3 _axisOfRotation = Vector3.right;
    private Quaternion _originalRot, _rotatedRot;
    private Vector3 _originalPos, _rotatedPos;
    [SerializeField] bool startOpened = false;
    private bool isOpen, isTryingToClose;

    void Start() {
        float invertedDirection = _allowedDirection.y * -1;
        _originalRot = transform.localRotation;
        _rotatedRot = Quaternion.AngleAxis(_originalRot.x + 90, _axisOfRotation * invertedDirection) * _originalRot;
        
        _originalPos = transform.localPosition;

        float rpX = _originalPos.x;
        float rpY = _originalPos.y + ((transform.localScale.y * 0.5f) * invertedDirection);
        float rpZ = _originalPos.z + ((transform.localScale.y * 1.5f));

        _rotatedPos = new(rpX, rpY, rpZ);
    
        if (startOpened) {
            OpenGate();
        } else {
            CloseGate();
        }
    }

    void Update() {
        if (isOpen && isTryingToClose) {
            bool isSomethingInTheWay = Physics.CheckBox(_originalPos, 
                                                     transform.localScale * 0.5f, 
                                                     _originalRot);
            
            if (!isSomethingInTheWay) {
                CloseGate();
            }
        }
    }

    public bool CanPassThrough(Vector3 movementDirection)
    {
        if (!isOpen) {
            return true;
        }
        
        if (Physics.Raycast(transform.position, _allowedDirection * -1f, transform.localScale.y)) {
            return false;
        }

        Vector3 worldAllowedDir = transform.TransformDirection(_allowedDirection);
        float dot = Vector3.Dot(movementDirection.normalized, worldAllowedDir);
        return dot > 0;
    }

    private void OpenGate() {
        transform.SetLocalPositionAndRotation(_rotatedPos, _rotatedRot);
        isOpen = true;
    }

    private void CloseGate() {
        transform.SetLocalPositionAndRotation(_originalPos, _originalRot);
        isOpen = false;
        isTryingToClose = false;
    }

    void OnCollisionEnter(Collision other) {
        Debug.Log($"collision with {other.gameObject.name}");

        if (other.transform.IsChildOf(transform)) return; //if to this attached, disregard
        if (other.gameObject.layer == 5) return;
        
        if (!isOpen) {
            OpenGate();
        }
    }

    void OnCollisionExit(Collision other)
    {
        isTryingToClose = true;
    }
}