using UnityEngine;

public class OneWayGateZ : OneWayGate
{
    protected override void Start() {
        _allowedDirection = Vector3.down;

        _originalRot = _physicalObj.transform.localRotation;
        _rotatedRot = Quaternion.AngleAxis(90, Vector3.right) * _originalRot;
        
        _originalPos = _physicalObj.transform.localPosition;
        float halfScale = transform.localScale.y / 2f;
        _rotatedPos = new(_originalPos.x, _originalPos.y + transform.localScale.y * 3, _originalPos.z - halfScale);
    }
}