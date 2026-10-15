
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    private KartController playerKart;

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

    private void Awake()
    {
        playerKart = GetComponent<KartController>();
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
        }
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
    }
}
