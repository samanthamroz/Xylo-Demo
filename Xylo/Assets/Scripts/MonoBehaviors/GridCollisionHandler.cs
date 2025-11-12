using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GridCollisionHandler : MonoBehaviour
{
    public bool IsCollidingAtPosition(Vector3 currentPosition, Vector3 targetPosition) {
        Collider[] colliders = Physics.OverlapBox(targetPosition, GetComponent<Collider>().bounds.extents, Quaternion.identity);

        bool isColliding = false;
        foreach (Collider c in colliders) {
            if (IsConnectedToSelf(c)) {
                continue;
            }

            // For triggers, only check CanPassThrough
            if (c.isTrigger) {
                if (!CanPassThrough(c, (currentPosition - targetPosition).normalized)) {
                    isColliding = true;
                    break;
                }
                continue;
            }

            // For non-triggers, check CanPassThrough, otherwise it's a collision
            if (!CanPassThrough(c, (currentPosition - targetPosition).normalized)) {
                isColliding = true;
                break;
            }
        }

        return isColliding;
    }

    private bool IsConnectedToSelf(Collider c) {
        return c.gameObject == gameObject || c.transform.IsChildOf(transform);
    }

    private bool CanPassThrough(Collider c, Vector3 direction) {
        if (c.gameObject.TryGetComponent(out IBlocksPassThrough b)) {
            return b.CanPassThrough(direction);
        }
        
        // If it's a trigger without IBlocksPassThrough, allow pass through
        // If it's a solid collider without IBlocksPassThrough, block it
        return c.isTrigger;
    }
}