using UnityEngine;

public class OneWayGateY : OneWayGate
{
    protected override void Start() {
        _allowedDirection = Vector3.down;

        _originalRot = _physicalObj.transform.localRotation;
        _rotatedRot = Quaternion.AngleAxis(90, Vector3.right) * _originalRot;
        
        _originalPos = _physicalObj.transform.localPosition;
        float halfScale = _physicalObj.transform.lossyScale.y / 2f;
        _rotatedPos = new(_originalPos.x, _originalPos.y + halfScale, _originalPos.z + transform.localScale.y + halfScale);
    }
}