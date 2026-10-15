
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    private KartController playerKart;
    private bool speedPowerUpActive = false;
    private bool freezePowerUpActive = false;
    private bool gunPowerUpActive = false;
    private bool missilePowerUpActive = false;
    private bool regenerationPowerUpActive = false;

    private float speedTimer;
    private float freezeTimer;
    private float gunTimer;
    private float missileTimer;

    private bool speedActive;
    private bool freezeActive;
    private bool gunActive;
    private bool missileActive;

    private float originalMaxSpeed;
    private float originalAcceleration;

    private KartController[] frozenKarts;
    [Header("Camera Configuration")]
    public Camera playerCamera;

    private KartController kart;

    // These allow future gun/missile scripts to check
    // whether this kart currently has those power-ups.
    public bool HasGunPowerUp => gunPowerUpActive;
    public bool HasMissilePowerUp => missilePowerUpActive;

    private void Awake()
    {
        kart = GetComponent<KartController>();

        if (kart == null)
        {
            Debug.LogError(
                "PowerUpManager needs a KartController on " +
                gameObject.name
            );
        }
    }

    private void Update()
    {
        // End the speed boost when its timer runs out.
        if (speedActive)
        {
            speedTimer -= Time.deltaTime;

            if (speedTimer <= 0f)
            {
                if (playerKart != null)
                {
                    playerKart.maxSpeed = originalMaxSpeed;
                    playerKart.acceleration = originalAcceleration;
                }

                speedActive = false;
            }
        }

        // Unfreeze other karts when the timer runs out.
        if (freezeActive)
        {
            freezeTimer -= Time.deltaTime;

            if (freezeTimer <= 0f)
            {
                if (frozenKarts != null)
                {
                    foreach (KartController kart in frozenKarts)
                    {
                        if (kart != null && kart != playerKart)
                            kart.SetFrozen(false);
                    }
        // Only reacts to power-up objects.
        if (!col.CompareTag("PickUp"))
            return;

        PowerupProperties pickup =
            col.GetComponent<PowerupProperties>();

        if (pickup == null)
        {
            Debug.LogError(
                "PickUp object " +
                col.gameObject.name +
                " is missing PowerupProperties!"
            );

            return;
        }

        // Gets the power-up type.
        string activeType = pickup.type.ToLower();

        // Random power-ups
        if (activeType == "random")
        {
            int roll = Random.Range(0, 5);

            if (roll == 0)
                activeType = "speed";
            else if (roll == 1)
                activeType = "freeze";
            else if (roll == 2)
                activeType = "gun";
            else if (roll == 3)
                activeType = "missile";
            else
                activeType = "regeneration";
        }

        // SPEED

        if (activeType == "speed" &&
            !speedPowerUpActive)
        {
            if (kart == null)
                return;

            kart.maxSpeed += 10f;
            kart.acceleration += 10f;

            speedPowerUpActive = true;

            speedPowerUpTimer = 8f;
        }

        // FREEZE

        else if (activeType == "freeze" &&
                 !freezePowerUpActive)
        {
            if (kart == null)
                return;

            KartController[] allKarts =
                FindObjectsByType<KartController>(
                    FindObjectsSortMode.None
                );

            // Freezes every other kart.
            foreach (KartController otherKart in allKarts)
            {
                if (otherKart != kart)
                {
                    otherKart.SetFrozen(true);
                }
            }

                freezeActive = false;
            }
        }

        // End the gun timer.
        if (gunActive)
        {
            gunTimer -= Time.deltaTime;

            if (gunTimer <= 0f)
                gunActive = false;
        }

        // End the missile timer.
        if (missileActive)
        {
            missileTimer -= Time.deltaTime;

            if (missileTimer <= 0f)
                missileActive = false;
        }
    }

    // Accepts the power-up asset instead of a string.
    public bool UsePowerUp(PowerUpDefinition definition)
    {
        if (playerKart == null)
        {
            Debug.LogError(
                "PowerUpManager needs a KartController on the same GameObject!"
            );

            return false;
        }

        if (definition == null)
            return false;

        // Read the power-up ID from its asset.
        string type = definition.id.ToLowerInvariant();

        switch (type)
        {
            case "speed":
                ActivateSpeed(definition);
                return true;

            case "freeze":
                ActivateFreeze(definition);
                return true;

            case "gun":
                gunActive = true;
                gunTimer = definition.duration;

                Debug.Log("Gun power-up activated.");
                return true;

            case "missile":
                missileActive = true;
                missileTimer = definition.duration;

                Debug.Log("Missile power-up activated.");
                return true;

            case "regeneration":
                playerKart.RegenHealth();
                return true;

            default:
                Debug.LogWarning("Unknown power-up ID: " + type);
                return false;
            freezePowerUpActive = true;

            freezePowerUpTimer = 3f;
        }

        // GUN

        else if (activeType == "gun" &&
                 !gunPowerUpActive)
        {
            gunPowerUpActive = true;

            gunPowerUpTimer = 5f;
        }

        // MISSILE

        else if (activeType == "missile" &&
                 !missilePowerUpActive)
        {
            missilePowerUpActive = true;

            // Keep your friend's 5 second duration.
            missilePowerUpTimer = 5f;
        }

        // REGENERATION

        else if (activeType == "regeneration" &&
                 !regenerationPowerUpActive)
        {
            if (kart == null)
                return;

            regenerationPowerUpActive = true;

            kart.RegenHealth();
        }

        // Disables the pickup after collection.
        // This prevents the same kart from repeatedly
        // triggering the same pickup while sitting inside it.
        col.gameObject.SetActive(false);
    }

    private void ActivateSpeed(PowerUpDefinition definition)
    {
        // Save the original stats only when the boost first starts.
        if (!speedActive)
        {
            originalMaxSpeed = playerKart.maxSpeed;
            originalAcceleration = playerKart.acceleration;
        }

        // Apply the configured bonuses without stacking them.
        playerKart.maxSpeed =
            originalMaxSpeed + definition.speedBonus;

        playerKart.acceleration =
            originalAcceleration + definition.accelerationBonus;
        // SPEED TIMER

        if (speedPowerUpActive)
        {
            speedPowerUpTimer -= Time.deltaTime;

            if (speedPowerUpTimer <= 0f)
            {
                if (kart != null)
                {
                    kart.maxSpeed -= 10f;
                    kart.acceleration -= 10f;
                }

                speedPowerUpActive = false;
            }
        }

        // FREEZE TIMER

        if (freezePowerUpActive)
        {
            freezePowerUpTimer -= Time.deltaTime;

            if (freezePowerUpTimer <= 0f)
            {
                KartController[] allKarts =
                    FindObjectsByType<KartController>(
                        FindObjectsSortMode.None
                    );

                // Unfreeze every other kart.
                foreach (KartController otherKart in allKarts)
                {
                    if (otherKart != kart)
                    {
                        otherKart.SetFrozen(false);
                    }
                }

        speedActive = true;
        speedTimer = definition.duration;
    }

    private void ActivateFreeze(PowerUpDefinition definition)
    {
        // Find every kart in the scene.
        frozenKarts = FindObjectsByType<KartController>(
            FindObjectsSortMode.None
        );

        // Freeze every kart except the player using this power-up.
        foreach (KartController kart in frozenKarts)
        {
            if (kart != null && kart != playerKart)
                kart.SetFrozen(true);
        }

        freezeActive = true;
        freezeTimer = definition.duration;
        // GUN TIMER

        if (gunPowerUpActive)
        {
            gunPowerUpTimer -= Time.deltaTime;

            if (gunPowerUpTimer <= 0f)
            {
                gunPowerUpActive = false;
            }
        }

        // MISSILE TIMER

        if (missilePowerUpActive)
        {
            missilePowerUpTimer -= Time.deltaTime;

            if (missilePowerUpTimer <= 0f)
            {
                missilePowerUpActive = false;
            }
        }

        // REGENERATION
        // Regeneration is currently instant,
        // so there is no timer to process.
    }
}
