using UnityEngine;
using UnityEngine.InputSystem;

public class KartController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 20f;
    public float maxSpeed = 25f;
    public float turnSpeed = 90f;

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

    public float regenDelay;
    public float regenInterval;
    public float regenAmount;

    private float nextHealTime;
    private PowerUpManager powerUpManager;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("This kart needs a Rigidbody!");
            return;
        }

        rb.isKinematic = false;
        rb.useGravity = true;

        // Stop the kart from tipping forwards or sideways.
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;
    }

    public void RegenHealth()
    {
        health = Mathf.Min(health + 60f, maxHealth);
    }

    private void Update()
    {
        if (isFrozen)
        {
            forwardInput = 0f;
            steeringInput = 0f;
            return;
        }

        forwardInput = 0f;
        steeringInput = 0f;

        if (Keyboard.current != null)
        {
            // Forward.
            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed)
            {
                forwardInput = 1f;
            }
            // Reverse.
            else if (Keyboard.current.sKey.isPressed ||
                     Keyboard.current.downArrowKey.isPressed)
            {
                forwardInput = -1f;
            }

            // Turn left.
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                steeringInput = -1f;
            }
            // Turn right.
            else if (Keyboard.current.dKey.isPressed ||
                     Keyboard.current.rightArrowKey.isPressed)
            {
                steeringInput = 1f;
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        if (isFrozen)
        {
            // Keep the kart stopped while frozen.
            rb.linearVelocity = Vector3.zero;
            return;
        }

        MoveKart();
        SidewaysMovement();
        ApplyFriction();
        SteerKart();
        LimitSpeed();
    }

    private void MoveKart()
    {
        // Apply force in the direction the kart faces.
        Vector3 force =
            transform.forward *
            forwardInput *
            acceleration;

        rb.AddForce(force, ForceMode.Acceleration);
    }

    private void SteerKart()
    {
        // Do not steer if the kart is almost stopped.
        if (rb.linearVelocity.magnitude < 0.1f)
            return;

        float speedFactor =
            Mathf.Clamp01(
                rb.linearVelocity.magnitude / maxSpeed
            );

        float currentTurnSpeed = turnSpeed;

        // Space increases turning while drifting.
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

        transform.Rotate(0f, turnAmount, 0f);
    }

    private void ApplyFriction()
    {
        bool drifting =
            Keyboard.current != null &&
            Keyboard.current.spaceKey.isPressed;

        // Convert the velocity into the kart's local direction.
        Vector3 localVelocity =
            transform.InverseTransformDirection(
                rb.linearVelocity
            );

        float sideGrip =
            drifting ? driftSideGrip : normalFriction;

        // Reduce sideways sliding.
        localVelocity.x *=
            Mathf.Clamp01(
                1f - (sideGrip * Time.fixedDeltaTime)
            );

        // Convert the velocity back to world direction.
        rb.linearVelocity =
            transform.TransformDirection(localVelocity);
    }

    private void LimitSpeed()
    {
        // Keep the kart below its maximum speed.
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void SidewaysMovement()
    {
        if (steeringInput == 0f)
            return;

        Vector3 sidewaysForce =
            transform.right *
            steeringInput *
            acceleration;

        rb.AddForce(sidewaysForce, ForceMode.Acceleration);
    }

    // Called by the freeze power-up.
    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;

        if (isFrozen && rb != null)
            rb.linearVelocity = Vector3.zero;
    }

    private void Start()
    {
        powerUpManager = GetComponent<PowerUpManager>();
        nextHealTime = Time.time + regenDelay;
    }

    // LateUpdate keeps regeneration running even while the kart is frozen.
    private void LateUpdate()
    {
        if (health >= maxHealth)
        {
            return;
        }

        if (regenAmount <= 0f || regenInterval <= 0f)
        {
            return;
        }

        if (Time.time >= nextHealTime)
        {
            RegenHealth(regenAmount);
            nextHealTime = Time.time + regenInterval;
        }
    }

    public void RegenHealth(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
    }

    public void TakeDamage(float damage, KartController attacker)
    {
        if (damage <= 0f)
        {
            return;
        }

        if (powerUpManager != null && powerUpManager.IsShieldActive())
        {
            return;
        }

        if (powerUpManager != null && attacker != null && attacker != this)
        {
            float fraction = powerUpManager.GetReflectionFraction();
            float reflectedDamage = damage * fraction;
            damage = damage - reflectedDamage;

            if (reflectedDamage > 0f)
            {
                attacker.TakeReflectedDamage(reflectedDamage);
            }
        }

        LoseHealth(damage);
    }

    public void TakeReflectedDamage(float damage)
    {
        if (powerUpManager != null && powerUpManager.IsShieldActive())
        {
            return;
        }

        LoseHealth(damage);
    }

    private void LoseHealth(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        health = Mathf.Max(health - damage, 0f);
        nextHealTime = Time.time + regenDelay;
    }
}
