
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
    private Vector3 vel;
    public void RunMarble() {
        GetComponent<Rigidbody>().isKinematic = false;

        float T = .75f;                // airtime per bounce
        float g = -Physics.gravity.y;   // ~9.81
        float vY = g * T * 0.5f;        // vertical launch speed
        float vX = 2f / T;             // horizontal speed (negative to go left)

        Rigidbody rb = GetComponent<Rigidbody>();
        float mass = rb.mass;
        Vector3 impulse = mass * new Vector3(vX, vY, 0f);

        //rb.AddForce(impulse, ForceMode.Impulse);
        rb.velocity = impulse;
        vel = impulse;
        print(rb.velocity);
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

    float lastBouncedY = -99;

    public float CalculateBounciness(float T, float stepDown)
    {
        float g = -Physics.gravity.y;

        // Apex height for given airtime
        float h = g * T * T / 8f;

        // Safety: if stepDown is bigger than the apex, impossible
        if (stepDown >= h)
        {
            Debug.LogWarning("Step down is too large for this airtime — no valid bounciness!");
            return 0f;
        }

        // Formula: b^2 = 1 - stepDown/h
        float b = Mathf.Sqrt(1f - stepDown / h);
        return b;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Bounces")) return;

        
        var speed = currentVelocity.magnitude;
        var direction = Vector3.Reflect(currentVelocity.normalized, collision.contacts[0].normal);

        float bounciness = .75f;

        Vector3 newVelocity = new (direction.x * Mathf.Max(speed, 0f), direction.y * Mathf.Max(speed * bounciness, 0f), 0); 
        GetComponent<Rigidbody>().velocity = vel;
        print(newVelocity);
    }
}
