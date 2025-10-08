using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OneWayBoxCollider : MonoBehaviour
{
    [Tooltip("The direction that the other object should be coming from for entry.")]
    [SerializeField] private Vector3 entryDirection = Vector3.up;
    [SerializeField] private bool localDirection = false;
    [SerializeField] private Vector3 triggerScale = Vector3.one * 1.25f;
    [SerializeField] private float penetrationDepthThreshold = 0.2f;

    private new BoxCollider collider = null;
    private BoxCollider collisionCheckTrigger = null;

    // The entry direction, calculated accordingly based on whether it is a local direction or not. 
    // This is pretty much what I've done in the video when copy-pasting the local direction check, but written as a property
    public Vector3 PassthroughDirection => localDirection ? transform.TransformDirection(entryDirection.normalized) : entryDirection.normalized;

    private void Awake()
    {
        collider = GetComponent<BoxCollider>();

        // Adding the BoxCollider and making sure that its sizes match the ones
        // of the OG collider.
        collisionCheckTrigger = gameObject.AddComponent<BoxCollider>();
        collisionCheckTrigger.size = new Vector3(
            collider.size.x * triggerScale.x,
            collider.size.y * triggerScale.y,
            collider.size.z * triggerScale.z
        );
        collisionCheckTrigger.center = collider.center;
        collisionCheckTrigger.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        TryIgnoreCollision(other);
    }

    public void TryIgnoreCollision(Collider other)
    {
        // Simulate a collision between our trigger and the intruder to check
        // the direction that the latter is coming from. The method returns true
        // if any collision has been detected.
        if (Physics.ComputePenetration(
            collisionCheckTrigger, collisionCheckTrigger.bounds.center, transform.rotation,
            other, other.bounds.center, other.transform.rotation,
            out Vector3 collisionDirection, out float penetrationDepth))
        {
            float dot = Vector3.Dot(PassthroughDirection, collisionDirection);

            // Opposite direction; passing not allowed.
            if (dot < 0)
            {
                // Activate collison only once the intruder is close enough to the trigger border, to avoid teleportation
                if(penetrationDepth < penetrationDepthThreshold) 
                {
                    // Making sure that the two object are NOT ignoring each other.
                    Physics.IgnoreCollision(collider, other, false);
                }
            }
            else
            {
                // Making the colliders ignore each other, and thus allow passage
                // from one way.
                Physics.IgnoreCollision(collider, other, true);
            }
        }
    }
}