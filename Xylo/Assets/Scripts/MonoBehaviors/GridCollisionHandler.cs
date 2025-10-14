using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GridCollisionHandler : MonoBehaviour
{
    public bool IsCollidingAtPosition(Vector3 currentPosition, Vector3 targetPosition) {
        Collider[] colliders = Physics.OverlapBox(targetPosition, GetComponent<Collider>().bounds.extents, Quaternion.identity);

        bool isColliding = false;
        foreach (Collider c in colliders) {
            if (!IsConnectedToSelf(c) && !c.isTrigger && !PassThrough(c, (currentPosition - targetPosition).normalized)) {
                isColliding = true;
                break;
            }
        }

        return isColliding;
    }

    private bool IsConnectedToSelf(Collider c) {
        return c.gameObject == gameObject || c.transform.IsChildOf(transform);
    }

    private bool PassThrough(Collider c, Vector3 direction) {
        if (c.gameObject.TryGetComponent<OneWayGate>(out OneWayGate g)) {
            return g.CanPassThrough(direction);
        }
        
        OneWayGate parentGate = c.GetComponentInParent<OneWayGate>();
        if (parentGate != null) {
            return parentGate.CanPassThrough(direction);
        }
        
        return false;
    }
}