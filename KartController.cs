using UnityEngine;
using UnityEngine.InputSystem;

public class KartController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 20f;
    public float maxSpeed = 25f;
    public float turnSpeed = 90f;
    public float reverseSpeed = 10f;

    [Header("Health")]
    public float maxHealth = 100f;
    public float health = 100f;

    [Header("Friction")]
    public float normalFriction = 4f;
    public float driftFriction = 1.5f;

    [Header("Drifting")]
    public float driftTurnMultiplier = 1.35f;
    public float driftSideGrip = 0.35f;

    private Rigidbody rb;

    private float forwardInput;
    private float steeringInput;

    private bool isFrozen = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("KartController needs a Rigidbody!");
            return;
        }

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (isFrozen)
        {
            forwardInput = 0f;
            steeringInput = 0f;
            return;
        }

        
        // FORWARD / REVERSE
             forwardInput = 0f;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
        {
            forwardInput = 1f;
        }
        else if (Keyboard.current.sKey.isPressed ||
                 Keyboard.current.downArrowKey.isPressed)
        {
            forwardInput = -1f;
        }

        // STEERING
        steeringInput = 0f;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            steeringInput = -1f;
        }
        else if (Keyboard.current.dKey.isPressed ||
                 Keyboard.current.rightArrowKey.isPressed)
        {
            steeringInput = 1f;
        }
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (isFrozen)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        MoveKart();
        ApplyFriction();
        SteerKart();
        LimitSpeed();
    }

    void MoveKart()
    {
        float currentAcceleration = acceleration;

        if (forwardInput < 0f)
        {
            currentAcceleration = reverseSpeed;
        }

        Vector3 force =
            transform.forward *
            forwardInput *
            currentAcceleration;

        rb.AddForce(force, ForceMode.Acceleration);
    }

    void SteerKart()
    {
        if (Mathf.Abs(steeringInput) < 0.01f)
            return;

        float speed = rb.linearVelocity.magnitude;

        if (speed < 0.5f)
            return;

        float speedFactor =
            Mathf.Clamp01(speed / maxSpeed);

        float currentTurnSpeed = turnSpeed;

        // Hold SPACE to drift
        bool drifting =
            Keyboard.current != null &&
            Keyboard.current.spaceKey.isPressed;

        if (drifting)
        {
            currentTurnSpeed *= driftTurnMultiplier;
        }

        float turnAmount =
            steeringInput *
            currentTurnSpeed *
            speedFactor *
            Time.fixedDeltaTime;

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

        Vector3 localVelocity =
            transform.InverseTransformDirection(
                rb.linearVelocity
            );

        float grip =
            drifting
                ? driftSideGrip
                : normalFriction;

        // Reduce sideways sliding
        localVelocity.x *=
            Mathf.Clamp01(
                1f -
                grip * Time.fixedDeltaTime
            );

        rb.linearVelocity =
            transform.TransformDirection(
                localVelocity
            );
    }

    void LimitSpeed()
    {
        float currentMaxSpeed = maxSpeed;

        if (forwardInput < 0f)
        {
            currentMaxSpeed = reverseSpeed;
        }

        if (rb.linearVelocity.magnitude > currentMaxSpeed)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized *
                currentMaxSpeed;
        }
    }

    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;

        if (frozen && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void SpawnAt(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogError("Spawn point has not been assigned!");
            return;
        }

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = spawnPoint.position;
        rb.rotation = spawnPoint.rotation;

        health = maxHealth;
    }

    public void RegenHealth()
    {
        health = Mathf.Min(
            health + 60f,
            maxHealth
        );
    }
}
