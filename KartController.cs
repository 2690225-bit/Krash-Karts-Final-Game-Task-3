
using UnityEngine;
using UnityEngine.InputSystem;

public class KartController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 20f;
    public float maxSpeed = 25f;
    public float turnSpeed = 65f;

    [Header("Health")]
    public float maxHealth = 100f;
    public float health = 100f;

    [Header("Friction")]
    public float normalFriction = 4f;
    public float driftFriction = 1.5f;

    [Header("Drifting")]
    public float driftTurnMultiplier = 1.35f;
    public float driftSideGrip = 0.35f;

    [Header("Passive Regeneration")]
    public float regenDelay = 30f;
    public float regenInterval = 3f;
    public float regenAmount = 5f;

    private Rigidbody rb;
    private Transform kartBody;
    private float forwardInput;
    private float steeringInput;
    private bool isFrozen = true;
    private float nextHealTime;
    private PowerUpManager powerUpManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            rb = GetComponentInParent<Rigidbody>();

        if (rb == null)
            rb = GetComponentInChildren<Rigidbody>(true);

        if (rb == null)
        {
            Debug.LogError(
                "KartController: Rigidbody missing from kart hierarchy.",
                gameObject
            );
            enabled = false;
            return;
        }

        kartBody = rb.transform;

        // Do not change Rigidbody constraints or gravity here.
        // This avoids changing the selection-panel kart physics.

        powerUpManager = GetComponent<PowerUpManager>();

        if (powerUpManager == null)
            powerUpManager = GetComponentInParent<PowerUpManager>();

        if (powerUpManager == null)
            powerUpManager = GetComponentInChildren<PowerUpManager>(true);
    }

    private void Start()
    {
        health = Mathf.Clamp(health, 0f, maxHealth);
        nextHealTime = Time.time + regenDelay;

        Debug.Log("KartController ready. Waiting for race countdown.");
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            Debug.LogWarning("NO KEYBOARD DETECTED");
            return;
        }

        if (isFrozen)
        {
            forwardInput = 0f;
            steeringInput = 0f;
            return;
        }

        forwardInput = 0f;
        steeringInput = 0f;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
            forwardInput = 1f;
        else if (Keyboard.current.sKey.isPressed ||
                 Keyboard.current.downArrowKey.isPressed)
            forwardInput = -1f;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
            steeringInput = -1f;
        else if (Keyboard.current.dKey.isPressed ||
                 Keyboard.current.rightArrowKey.isPressed)
            steeringInput = 1f;
    }

    private void FixedUpdate()
    {
        if (rb == null || kartBody == null || rb.isKinematic)
            return;

        if (isFrozen)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        // Accelerate and reverse.
        rb.AddForce(
            kartBody.forward * forwardInput * acceleration,
            ForceMode.Acceleration
        );

        if (forwardInput != 0f)
        {
            Debug.Log(
                "W INPUT DETECTED | Frozen: " + isFrozen +
                " | Kinematic: " + rb.isKinematic +
                " | Constraints: " + rb.constraints +
                " | Velocity: " + rb.linearVelocity
            );
        }

        // Reduce sideways sliding.
        Vector3 localVelocity =
            kartBody.InverseTransformDirection(rb.linearVelocity);

        bool drifting = Keyboard.current != null &&
                        Keyboard.current.spaceKey.isPressed;

        float grip = drifting ? driftSideGrip : normalFriction;

        localVelocity.x *= Mathf.Clamp01(
            1f - grip * Time.fixedDeltaTime
        );

        rb.linearVelocity =
            kartBody.TransformDirection(localVelocity);

        // Turn while moving.
        float speed = rb.linearVelocity.magnitude;

        if (speed > 0.2f && steeringInput != 0f)
        {
            float speedFactor = Mathf.Clamp01(speed / maxSpeed);
            float turnMultiplier =
                drifting ? driftTurnMultiplier : 1f;

            float turnAmount =
                steeringInput * turnSpeed * speedFactor *
                turnMultiplier * Time.fixedDeltaTime;

            kartBody.Rotate(0f, turnAmount, 0f);
        }

        // Limit maximum speed.
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized * maxSpeed;
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

        Debug.Log("Kart controls frozen: " + isFrozen);
    }

    public void RegenHealth()
    {
        health = Mathf.Min(health + 60f, maxHealth);
    }

    public void RegenHealth(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
    }

    private void LateUpdate()
    {
        if (health >= maxHealth ||
            regenAmount <= 0f ||
            regenInterval <= 0f)
            return;

        if (Time.time >= nextHealTime)
        {
            RegenHealth(regenAmount);
            nextHealTime = Time.time + regenInterval;
        }
    }

    public void TakeDamage(float damage, KartController attacker)
    {
        if (damage <= 0f)
            return;

        if (powerUpManager != null &&
            powerUpManager.IsShieldActive())
            return;

        if (powerUpManager != null &&
            attacker != null &&
            attacker != this)
        {
            float reflectedDamage =
                damage * powerUpManager.GetReflectionFraction();

            damage -= reflectedDamage;

            if (reflectedDamage > 0f)
                attacker.TakeReflectedDamage(reflectedDamage);
        }

        LoseHealth(damage);
    }

    public void TakeReflectedDamage(float damage)
    {
        if (powerUpManager != null &&
            powerUpManager.IsShieldActive())
            return;

        LoseHealth(damage);
    }

    private void LoseHealth(float damage)
    {
        if (damage <= 0f)
            return;

        health = Mathf.Max(health - damage, 0f);
        nextHealTime = Time.time + regenDelay;
    }
}
