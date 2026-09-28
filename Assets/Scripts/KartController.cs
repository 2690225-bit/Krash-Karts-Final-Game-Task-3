using UnityEngine;
using UnityEngine.InputSystem;

public class KartController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 20f; // How quickly the kart accelerates
    public float maxSpeed = 25f; // Maximum speed
    public float turnSpeed = 90f; // Steering speed

    [Header("Health")]
    public float maxHealth = 100f; // Maximum health of the kart
    public float health = 100f; // Current health of the kart

    [Header("Friction")]
    public float normalFriction = 4f; // Normal sideways grip
    public float driftFriction = 1.5f; // Drift friction

    [Header("Drifting")]
    public float driftTurnMultiplier = 1.35f; // Extra steering while drifting
    public float driftSideGrip = 0.35f; // Allows more sideways sliding

    private Rigidbody rb;

    private float forwardInput;
    private float steeringInput;

    private bool isFrozen = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("PlayerKart needs a Rigidbody!");
            return;
        }

        rb.isKinematic = false;
        rb.useGravity = true;

        // Stops the kart from tipping over
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (isFrozen)
        {
            forwardInput = 0f;
            steeringInput = 0f;
            return;
        }

        forwardInput = 0f;
        steeringInput = 0f;

        // Reads WASD and arrow-key controls
        if (Keyboard.current != null)
        {
            // Forward
            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed)
            {
                forwardInput = 1f;
            }

            // Reverse
            else if (Keyboard.current.sKey.isPressed ||
                     Keyboard.current.downArrowKey.isPressed)
            {
                forwardInput = -1f;
            }

            // Left
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                steeringInput = -1f;
            }

            // Right
            else if (Keyboard.current.dKey.isPressed ||
                     Keyboard.current.rightArrowKey.isPressed)
            {
                steeringInput = 1f;
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (isFrozen)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }


        MoveKart();
        SidewaysMovement(); 
        ApplyFriction();
        SteerKart();
        LimitSpeed();
    }

    void MoveKart()
    {
        // Moves the kart in the direction it is facing
        Vector3 force =
            transform.forward *
            forwardInput *
            acceleration;

        rb.AddForce(force, ForceMode.Acceleration);
    }

    void SteerKart()
    {
        // Prevents steering while completely stopped
        if (rb.linearVelocity.magnitude < 0.1f)
            return;

        float speedFactor =
            Mathf.Clamp01(
                rb.linearVelocity.magnitude / maxSpeed
            );

        float currentTurnSpeed = turnSpeed;

        // Space activates drifting
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.isPressed)
        {
            currentTurnSpeed *= driftTurnMultiplier;
        }

        float turnAmount =
            steeringInput *
            currentTurnSpeed *
            speedFactor *
            Time.fixedDeltaTime;

        // Turns the kart left or right
        transform.Rotate(
            0f,
            turnAmount,
            0f
        );
    }

    void ApplyFriction()
    {
        bool drifting =
            Keyboard.current != null &&
            Keyboard.current.spaceKey.isPressed;

        // Converts velocity into the kart's local space
        Vector3 localVelocity =
            transform.InverseTransformDirection(
                rb.linearVelocity
            );

        float sideGrip =
            drifting ? driftSideGrip : normalFriction;

        // Reduces sideways sliding
        localVelocity.x *=
            Mathf.Clamp01(
                1f -
                (sideGrip * Time.fixedDeltaTime)
            );

        // Converts velocity back to world space
        rb.linearVelocity =
            transform.TransformDirection(
                localVelocity
            );
    }

    void LimitSpeed()
    {
        // Prevents the kart from exceeding maximum speed
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized *
                maxSpeed;
        }
    }
    void SidewaysMovement()
    {
        if (steeringInput == 0f)
            return;

        Vector3 sidewaysForce =
            transform.right *
            steeringInput *
            acceleration;

        rb.AddForce(sidewaysForce, ForceMode.Acceleration);
    }

    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;

        // Immediately stop the kart when frozen
        if (frozen && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    public void RegenHealth()
    {
        health = Mathf.Min(health + 60f, maxHealth);
    }
}