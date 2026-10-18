using UnityEngine;
[System.Serializable]
public class PowerUpDefinitionFallback { public string powerUpName; public string type; }

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

    [Header("Player Tracking")]
    [Tooltip("You can leave this blank! It will automatically look for the KartController in your scene.")]
    public KartController kart;

    // These allow future gun/missile scripts to check
    // whether this kart currently has those power-ups.
    public bool HasGunPowerUp => gunPowerUpActive;
    public bool HasMissilePowerUp => missilePowerUpActive;

    void Start()
    {
        if (kart == null)
        {
            kart = GetComponent<KartController>();
        }

        if (kart == null)
        {
            kart = Object.FindFirstObjectByType<KartController>();
        }

        if (kart == null)
        {
            Debug.LogError(
                "PowerUpManager: Could not locate a KartController component on " +
                gameObject.name + " or anywhere else inside the open SceneHierarchy layout!"
            );
        }
        else
        {
            Debug.Log("PowerUpManager: Successfully locked onto target Kart: " + kart.gameObject.name);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        // Only reacts to power-up objects.
        if (!col.CompareTag("PickUp"))
            return;

        PowerUpProperties pickup = col.GetComponent<PowerUpProperties>();

        if (pickup == null)
        {
            Debug.LogError(
                "PickUp object " + col.gameObject.name + " is missing PowerUpProperties component!"
            );
            return;
        }

        string activeType = "";

        var pickupTypeField = pickup.GetType().GetField("Type") ?? pickup.GetType().GetField("type") ?? pickup.GetType().GetField("powerUpType");
        var pickupPropertyField = pickup.GetType().GetProperty("Type") ?? pickup.GetType().GetProperty("type") ?? pickup.GetType().GetProperty("powerUpType");

        if (pickupTypeField != null)
        {
            activeType = pickupTypeField.GetValue(pickup)?.ToString();
        }
        else if (pickupPropertyField != null)
        {
            activeType = pickupPropertyField.GetValue(pickup, null)?.ToString();
        }

        if (string.IsNullOrEmpty(activeType))
        {
            Debug.LogError("PowerUpManager: Could not find a 'Type' variable inside your teammate's PowerUpProperties script!");
            return;
        }

        // Passes the collected power-up directly into the activation loop
        ActivatePowerUp(activeType.ToLower());

        // Disables the pickup item box after collection.
        col.gameObject.SetActive(false);
    }

    public void UsePowerUp(PowerUpDefinition itemDefinition)
    {
        if (itemDefinition == null) return;

        string typeName = "";
        var nameField = itemDefinition.GetType().GetField("powerUpName") ?? itemDefinition.GetType().GetField("typeName") ?? itemDefinition.GetType().GetField("type");
        var nameProperty = itemDefinition.GetType().GetProperty("powerUpName") ?? itemDefinition.GetType().GetProperty("typeName") ?? itemDefinition.GetType().GetProperty("type");

        if (nameField != null) typeName = nameField.GetValue(itemDefinition)?.ToString();
        else if (nameProperty != null) typeName = nameProperty.GetValue(itemDefinition, null)?.ToString();

        if (!string.IsNullOrEmpty(typeName))
        {
            ActivatePowerUp(typeName.ToLower());
        }
    }

    public void UsePowerUp(string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return;
        ActivatePowerUp(typeName.ToLower());
    }

    private void ActivatePowerUp(string activeType)
    {
        // Random power-ups selection handling
        if (activeType == "random")
        {
            int roll = Random.Range(0, 5);
            if (roll == 0) activeType = "speed";
            else if (roll == 1) activeType = "freeze";
            else if (roll == 2) activeType = "gun";
            else if (roll == 3) activeType = "missile";
            else activeType = "regeneration";
        }

        // SPEED POWER UP
        if (activeType == "speed" && !speedPowerUpActive)
        {
            if (kart == null) return;

            kart.maxSpeed += 10f;
            kart.acceleration += 10f;

            speedPowerUpActive = true;
            speedPowerUpTimer = 8f;
            Debug.Log("Speed boost activated!");
        }

        // FREEZE POWER UP
        else if (activeType == "freeze" && !freezePowerUpActive)
        {
            if (kart == null) return;

            KartController[] allKarts = FindObjectsByType<KartController>(FindObjectsSortMode.None);

            foreach (KartController otherKart in allKarts)
            {
                if (otherKart != kart)
                {
                    otherKart.SetFrozen(true);
                }
            }

            freezePowerUpActive = true;
            freezePowerUpTimer = 3f;
            Debug.Log("All rival karts frozen!");
        }

        // GUN POWER UP
        else if (activeType == "gun" && !gunPowerUpActive)
        {
            gunPowerUpActive = true;
            gunPowerUpTimer = 5f;
            Debug.Log("Gun weapons system online!");
        }

        // MISSILE POWER UP
        else if (activeType == "missile" && !missilePowerUpActive)
        {
            missilePowerUpActive = true;
            missilePowerUpTimer = 5f;
            Debug.Log("Tracking missile loaded!");
        }

        // REGENERATION POWER UP
        else if (activeType == "regeneration" && !regenerationPowerUpActive)
        {
            if (kart == null) return;

            kart.RegenHealth();
            Debug.Log("Health regenerated!");
        }
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
                KartController[] allKarts = FindObjectsByType<KartController>(FindObjectsSortMode.None);

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
    }
}
