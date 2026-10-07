using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    private bool speedPowerUpActive = false;
    private bool freezePowerUpActive = false;
    private bool gunPowerUpActive = false;
    private bool missilePowerUpActive = false;
    private bool regenerationPowerUpActive = false;

    private float speedPowerUpTimer = 0f;
    private float freezePowerUpTimer = 0f;
    private float gunPowerUpTimer = 0f;
    private float missilePowerUpTimer = 0f;

    [Header("Camera Configuration")]
    public Camera playerCamera;

    private KartController kart;

    // These allow future gun/missile scripts to check
    // whether this kart currently has those power-ups.
    public bool HasGunPowerUp => gunPowerUpActive;
    public bool HasMissilePowerUp => missilePowerUpActive;

    void Start()
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

    void OnTriggerEnter(Collider col)
    {
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

    void Update()
    {
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

                freezePowerUpActive = false;
            }
        }

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
