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

    private float normalSpeed;
    private float normalAcceleration;
    private KartController[] frozenKarts;
    [Header("Camera Configuration")]
    public Camera playerCamera;

    private KartController kart;

    // These allow future gun/missile scripts to check
    // whether this kart currently has those power-ups.
    public bool HasGunPowerUp => gunPowerUpActive;
    public bool HasMissilePowerUp => missilePowerUpActive;

    private void Start()
    {
        playerKart = GetComponent<KartController>();
    }

    private void Update()
    {
        if (activePowerUp == null)
        {
            return;
        }

        if (activePowerUp.type == PowerUpType.Gun ||
            activePowerUp.type == PowerUpType.Missile)
        {
            timeToNextShot = timeToNextShot - Time.deltaTime;

            if (timeToNextShot <= 0f)
            {
                FireShot();
            }
        }
        else
        {
            timeLeft = timeLeft - Time.deltaTime;

            if (timeLeft <= 0f)
            {
                EndPowerUp();
            }
        }
    }

    public bool UsePowerUp(PowerUpDefinition powerUp)
    {
        if (activePowerUp != null || kart == null || powerUp == null)
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

        if (powerUp.type == PowerUpType.Regeneration)
        {
            kart.RegenHealth(powerUp.healthBonus);
            return true;
        }

        activePowerUp = powerUp;
        timeLeft = powerUp.duration;

        if (powerUp.type == PowerUpType.Speed)
        {
            // Save old values so they can be restored later.
            normalSpeed = kart.maxSpeed;
            normalAcceleration = kart.acceleration;
            kart.maxSpeed = kart.maxSpeed + powerUp.speedBonus;
            kart.acceleration = kart.acceleration + powerUp.accelerationBonus;
        }

        if (powerUp.type == PowerUpType.Freeze)
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
