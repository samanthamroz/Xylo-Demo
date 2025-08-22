
using UnityEngine;

public class PlayerMarble : InteractableObject
{
    public Vector3 resetPosition;
    public Vector3 currentVelocity;
    void Start()
    {
        resetPosition = transform.position;
        currentVelocity = Vector3.zero;
    }
    void Update()
    {
        currentVelocity = GetComponent<Rigidbody>().velocity;
    }
    
    public override void DoClick() {
        LevelManager.self.StartAttempt();
    }
    public void RunMarble() {
        GetComponent<Rigidbody>().isKinematic = false;

        float T = 0.75f;                // airtime per bounce
        float g = -Physics.gravity.y;   // ~9.81
        float vY = g * T * 0.5f;        // vertical launch speed
        float vX = 2f / T;             // horizontal speed (negative to go left)

        Rigidbody rb = GetComponent<Rigidbody>();
        float mass = rb.mass;
        Vector3 impulse = mass * new Vector3(vX, vY, 0f);

        rb.AddForce(impulse, ForceMode.Impulse);
    }
    
    public void ResetSelf() {
        GetComponent<Rigidbody>().isKinematic = true;
        LeanTween.move(gameObject, resetPosition, .5f).setEaseInOutSine();
    }

    public void ResetSelfToCurrentPosition() {
        resetPosition = transform.position;
        ResetSelf();
    }

    public void UpdateSavedVelocity() {
        currentVelocity = GetComponent<Rigidbody>().velocity;
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Bounces")) return;

        var speed = currentVelocity.magnitude;
        var direction = Vector3.Reflect(currentVelocity.normalized, collision.contacts[0].normal);
        float bounciness = .75f;

        Vector3 newVelocity = new (direction.x * Mathf.Max(speed, 0f), direction.y * Mathf.Max(speed * bounciness, 0f), 0); 

        GetComponent<Rigidbody>().velocity = newVelocity;
    }
}
