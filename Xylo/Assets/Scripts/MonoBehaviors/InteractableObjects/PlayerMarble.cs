using UnityEngine;

public class PlayerMarble : InteractableObject
{
    public Vector3 resetPosition;
    public Vector3 currentVelocity;
    private float currentVX; // track horizontal speed
    void Start()
    {
        resetPosition = transform.position;
        currentVelocity = Vector3.zero;
    }
    public override void DoClick() {
        LevelManager.self.StartAttempt();
    }

    public void RunMarble() {
        GetComponent<Rigidbody>().isKinematic = false;

        float T = 0.75f;                // airtime per bounce
        float g = -Physics.gravity.y;   // ~9.81
        float vY = g * T * 0.5f;        // vertical launch speed
        currentVX = 2f / T;             // horizontal speed (negative to go left)

        Rigidbody rb = GetComponent<Rigidbody>();
        float mass = rb.mass;
        Vector3 impulse = mass * new Vector3(currentVX, vY, 0f);

        rb.AddForce(impulse, ForceMode.Impulse); 

        Debug.Log(GetComponent<Rigidbody>().velocity);
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

    void OnCollisionEnter(Collision other)
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        float T = 0.75f;
        float g = -Physics.gravity.y;
        float vY = g * T * 0.5f;    // up speed

        // Get the collision normal
        Vector3 normal = other.contacts[0].normal;

        // Reflect the horizontal velocity component along the collision normal
        Vector3 horizontalVelocity = new Vector3(currentVX, 0f, 0f);
        Vector3 reflected = Vector3.Reflect(horizontalVelocity, normal);

        // Use the reflected X for horizontal speed
        currentVX = reflected.x;

        float bounceLoss = .75f;
        if (Mathf.Abs(normal.y) > 0.5f) 
        {
            vY *= bounceLoss;
        } 
        else 
        {
            // keep falling velocity when hitting mostly vertical walls
            vY = rb.velocity.y;
        }

        rb.velocity = new Vector3(currentVX, vY, 0);

        Debug.Log(rb.velocity);
    }
}
