using UnityEngine;

// Uses the first power-up from the inventory.
// Only one timed effect or gun burst can run at once on a kart.
public class PowerUpManager : MonoBehaviour
{
    // Make an empty child called FirePoint and assign it here.
    public Transform firePoint;

    private KartController kart;
    private PowerUpDefinition activePowerUp;
    private float timeLeft;
    private float timeToNextShot;
    private int shotsLeft;

    private float normalSpeed;
    private float normalAcceleration;
    private KartController[] frozenKarts;

    private void Awake()
    {
        kart = GetComponent<KartController>();

        if (kart == null)
        {
            Debug.LogError(
                "PowerUpManager requires KartController " +
                "on the same GameObject.",
                gameObject
            );
        }
    }

    private void Update()
    {
        if (activePowerUp == null)
        {
            return;
        }

        // Gun and Missile use a shot counter, not an effect duration.
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
            return false;
        }

        if (powerUp.type == PowerUpType.None)
        {
            return false;
        }

        // Weapons need a projectile prefab and a place to fire from.
        if (powerUp.type == PowerUpType.Gun || powerUp.type == PowerUpType.Missile)
        {
            if (firePoint == null || powerUp.projectilePrefab == null)
            {
                Debug.LogWarning("Assign a FirePoint and projectile prefab.");
                return false;
            }

            if (powerUp.shotsPerUse <= 0)
            {
                return false;
            }
        }

        // Health Boost works immediately and uses the asset's heal amount.
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
            // Find the other karts and freeze them, not the user.
            frozenKarts = FindObjectsByType<KartController>(FindObjectsSortMode.None);

            for (int i = 0; i < frozenKarts.Length; i++)
            {
                if (frozenKarts[i] != kart)
                {
                    frozenKarts[i].SetFrozen(true);
                }
            }
        }

        if (powerUp.type == PowerUpType.Gun || powerUp.type == PowerUpType.Missile)
        {
            shotsLeft = powerUp.shotsPerUse;
            FireShot(); // Fire the first shot immediately.
        }

        // Shield and Reflection are checked by KartController when hit.
        return true;
    }

    private void FireShot()
    {
        // Create a projectile using the prefab assigned to this power-up.
        PowerUpProjectile bullet = Instantiate(
            activePowerUp.projectilePrefab,
            firePoint.position,
            firePoint.rotation);

        bullet.Launch(kart, activePowerUp);
        shotsLeft = shotsLeft - 1;
        timeToNextShot = activePowerUp.shotInterval;

        if (shotsLeft <= 0)
        {
            EndPowerUp();
        }
    }

    private void EndPowerUp()
    {
        if (activePowerUp == null)
        {
            return;
        }

        if (activePowerUp.type == PowerUpType.Speed)
        {
            kart.maxSpeed = normalSpeed;
            kart.acceleration = normalAcceleration;
        }

        if (activePowerUp.type == PowerUpType.Freeze && frozenKarts != null)
        {
            for (int i = 0; i < frozenKarts.Length; i++)
            {
                if (frozenKarts[i] != null && frozenKarts[i] != kart)
                {
                    frozenKarts[i].SetFrozen(false);
                }
            }
        }

        activePowerUp = null;
        frozenKarts = null;
        shotsLeft = 0;
    }

    private void OnDisable()
    {
        // Restore speed/unfreeze opponents if this component is disabled.
        EndPowerUp();
    }

    // The health and UI scripts only need these four methods.
    public bool IsShieldActive()
    {
        if (activePowerUp == null)
        {
            return false;
        }

        return activePowerUp.type == PowerUpType.Shield;
    }

    public float GetReflectionFraction()
    {
        if (activePowerUp == null)
        {
            return 0f;
        }

        if (activePowerUp.type == PowerUpType.Reflection)
        {
            return Mathf.Clamp01(activePowerUp.reflectionFraction);
        }

        return 0f;
    }

    public PowerUpDefinition GetActivePowerUp()
    {
        return activePowerUp;
    }

    public float GetTimeLeft()
    {
        return timeLeft;
    }

    public int GetShotsLeft()
    {
        return shotsLeft;
    }
}
