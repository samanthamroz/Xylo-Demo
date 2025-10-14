using System.Collections.Generic;
using UnityEngine;
public abstract class OneWayGate : MonoBehaviour {
    protected Vector3 _allowedDirection;
    [SerializeField] protected GameObject _physicalObj;
    protected Quaternion _originalRot, _rotatedRot;
    protected Vector3 _originalPos, _rotatedPos;
    protected List<Collider> _objectsInside = new();

    protected abstract void Start();
    public bool CanPassThrough(Vector3 movementDirection)
    {
        if (_objectsInside.Count > 0) {
            return true;
        }
        
        if (Physics.Raycast(transform.position, _allowedDirection * -1f, _physicalObj.transform.localScale.y)) {
            return false;
        }

        Vector3 worldAllowedDir = transform.TransformDirection(_allowedDirection);
        float dot = Vector3.Dot(movementDirection.normalized, worldAllowedDir);
        return dot > 0;
    }

    void OnTriggerEnter(Collider other) {
        if (other.transform.IsChildOf(transform)) return;

        _objectsInside.Add(other);
        
        // Only rotate once when first object enters
        if (_objectsInside.Count == 1) {
           _physicalObj.transform.SetLocalPositionAndRotation(_rotatedPos, _rotatedRot);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform.IsChildOf(transform)) return;
        
        _objectsInside.Remove(other);
        
        if (_objectsInside.Count == 0) {
            _physicalObj.transform.SetLocalPositionAndRotation(_originalPos, _originalRot);
        }
    }
}