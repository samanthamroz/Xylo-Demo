using System.Collections.Generic;
using UnityEngine;
public class OneWayGate : MonoBehaviour, IBlocksPassThrough {
    [SerializeField] private Vector2 _allowedDirection;
    private Quaternion _originalRot, _rotatedRot;
    private Vector3 _originalPos, _rotatedPos;
    [SerializeField] bool startOpened = false;
    private bool isOpen, isTryingToClose;

    void Start() {
        float invertedYDirection = _allowedDirection.y * -1f;
        float invertedXDirection = _allowedDirection.x * -1f;

        _originalRot = transform.localRotation;
        _originalPos = transform.localPosition;

        float halfScale = transform.localScale.y * 0.5f;
        
        if (_allowedDirection.x != 0) {
            Vector3 _axisOfRotation = _originalRot * new Vector3(_allowedDirection.x, 0, 0) * invertedXDirection;
            _rotatedRot = Quaternion.AngleAxis(_originalRot.y - 90f, _axisOfRotation) * _originalRot;

            float rpoX = halfScale * invertedXDirection;
            float rpoY = 0f;
            float rpoZ = halfScale * 3f;

            Quaternion roY = Quaternion.Euler(0f, _originalRot.eulerAngles.y, 0f);
            Vector3 rpOffset = roY * new Vector3(rpoX, rpoY, rpoZ);

            _rotatedPos = _originalPos + rpOffset;
        }
        
        if (_allowedDirection.y != 0) {
            Vector3 _axisOfRotation = _originalRot * new Vector3(_allowedDirection.y, 0, 0) * invertedYDirection;
            _rotatedRot = Quaternion.AngleAxis(_originalRot.x - 90f, _axisOfRotation) * _originalRot;

            float rpoX = 0f;
            float rpoY = halfScale * invertedYDirection;
            float rpoZ = halfScale * 3f;

            Quaternion roY = Quaternion.Euler(0f, _originalRot.eulerAngles.y, 0f);
            Vector3 rpOffset = roY * new Vector3(rpoX, rpoY, rpoZ);

            _rotatedPos = _originalPos + rpOffset;
        }
    
        if (startOpened) {
            OpenGate();
        } else {
            CloseGate();
        }
    }

    void Update() {
        Vector3 raycastDirection = _allowedDirection * -1f;
        Debug.DrawRay(transform.position, raycastDirection * 1f, Color.red);

        if (isOpen && isTryingToClose) {
            // Check in world space with a slightly smaller box to avoid edge cases
            Vector3 worldPos = transform.parent != null ? 
                transform.parent.TransformPoint(_originalPos) : _originalPos;
            Quaternion worldRot = transform.parent != null ? 
                transform.parent.rotation * _originalRot : _originalRot;
            
            Collider[] overlaps = Physics.OverlapBox(worldPos, 
                                                     transform.localScale * 0.45f, 
                                                     worldRot);
            
            bool isSomethingInTheWay = false;
            foreach (Collider col in overlaps) {
                // Ignore self and children
                if (col.transform == transform || col.transform.IsChildOf(transform)) continue;
                // Ignore triggers
                if (col.isTrigger) continue;
                
                isSomethingInTheWay = true;
                break;
            }
            
            if (!isSomethingInTheWay) {
                CloseGate();
            }
        }
    }

    public bool CanPassThrough(Vector3 movementDirection)
    {
        if (isOpen) {
            return true;
        }
        
        Vector3 raycastDirection = _allowedDirection * -1f;
        if (Physics.Raycast(transform.position, raycastDirection, 1f)) {
            return false;
        }

        //Vector3 worldAllowedDir = transform.TransformDirection(_allowedDirection).normalized;
        float dot = Vector3.Dot(movementDirection.normalized, _allowedDirection);
        bool movingInAllowedDirection = dot > 0f;

        return movingInAllowedDirection;
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

    void OnTriggerEnter(Collider other) {
        if (other.transform.IsChildOf(transform)) return; //if to this attached, disregard
        if (other.gameObject.layer == 5) return;
        
        if (!isOpen) {
            OpenGate();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform.IsChildOf(transform)) return;
        if (other.gameObject.layer == 5) return;

        isTryingToClose = true;
    }
}