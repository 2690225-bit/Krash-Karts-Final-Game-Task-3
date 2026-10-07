
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    private bool speedPowerUpActive;
    private bool freezePowerUpActive;
    private bool gunPowerUpActive;
    private bool missilePowerUpActive;
    private bool regenerationPowerUpActive;

    private float speedPowerUpTimer;
    private float freezePowerUpTimer;
    private float gunPowerUpTimer;
    private float missilePowerUpTimer;

    [Header("Camera Configuration")]
    public Camera playerCamera;

    private KartController playerKart;

    private float originalMaxSpeed;
    private float originalAcceleration;

    void Awake()
    {
        playerKart = GetComponent<KartController>();

        if (playerKart == null)
        {
            Debug.LogError(
                "PowerUpManager needs a KartController " +
                "on the same GameObject!",
                this
            );
            return;
        }

        originalMaxSpeed = playerKart.maxSpeed;
        originalAcceleration = playerKart.acceleration;
    }

    void OnTriggerEnter(Collider col)
    {
        if (!col.CompareTag("PickUp"))
            return;

        PowerupProperties pickup =
            col.GetComponentInParent<PowerupProperties>();

        if (pickup == null)
        {
            Debug.LogError(
                "Pickup is tagged PickUp but has no " +
                "PowerupProperties component!",
                col.gameObject
            );
            return;
        }

        if (playerKart == null)
            return;

        // Read the type before removing the pickup.
        string activeType = pickup.type.ToLowerInvariant();

        if (activeType == "random")
        {
            string[] types =
            {
                "speed",
                "freeze",
                "gun",
                "missile",
                "regeneration"
            };

            activeType = types[Random.Range(0, types.Length)];
        }

        switch (activeType)
        {
            case "speed":
                if (!speedPowerUpActive)
                {
                    playerKart.maxSpeed =
                        originalMaxSpeed + 10f;

                    playerKart.acceleration =
                        originalAcceleration + 10f;

                    speedPowerUpActive = true;
                    speedPowerUpTimer = 8f;
                }
                break;

            case "freeze":
                if (!freezePowerUpActive)
                {
                    KartController[] allKarts =
                        FindObjectsByType<KartController>(
                            FindObjectsSortMode.None
                        );

                    foreach (KartController kart in allKarts)
                    {
                        if (kart != playerKart)
                            kart.SetFrozen(true);
                    }

                    freezePowerUpActive = true;
                    freezePowerUpTimer = 3f;
                }
                break;

            case "gun":
                gunPowerUpActive = true;
                gunPowerUpTimer = 5f;
                break;

            case "missile":
                missilePowerUpActive = true;
                missilePowerUpTimer = 5f;
                break;

            case "regeneration":
                if (!regenerationPowerUpActive)
                {
                    playerKart.RegenHealth();
                    regenerationPowerUpActive = true;
                }
                break;

            default:
                Debug.LogWarning(
                    "Unknown power-up type: " + activeType
                );
                return;
        }

        // Remove the pickup after successfully collecting it.
        Destroy(pickup.gameObject);
    }

    void Update()
    {
        if (speedPowerUpActive)
        {
            speedPowerUpTimer -= Time.deltaTime;

            if (speedPowerUpTimer <= 0f)
            {
                playerKart.maxSpeed = originalMaxSpeed;
                playerKart.acceleration = originalAcceleration;

                speedPowerUpActive = false;
            }
        }

        if (freezePowerUpActive)
        {
            freezePowerUpTimer -= Time.deltaTime;

            if (freezePowerUpTimer <= 0f)
            {
                KartController[] allKarts =
                    FindObjectsByType<KartController>(
                        FindObjectsSortMode.None
                    );

                foreach (KartController kart in allKarts)
                {
                    if (kart != playerKart)
                        kart.SetFrozen(false);
                }

                freezePowerUpActive = false;
            }
        }

        if (gunPowerUpActive)
        {
            gunPowerUpTimer -= Time.deltaTime;

            if (gunPowerUpTimer <= 0f)
                gunPowerUpActive = false;
        }

        if (missilePowerUpActive)
        {
            missilePowerUpTimer -= Time.deltaTime;

            if (missilePowerUpTimer <= 0f)
                missilePowerUpActive = false;
        }
    }
}
