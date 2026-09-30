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

    private KartController playerKart;

    void Start()
    {
        playerKart = GetComponent<KartController>();
    }

    void OnTriggerEnter(Collider col)
    {
        // Only react to objects tagged as "PickUp"
        if (col.CompareTag("PickUp"))
        {
            // Get the pickup's properties so we can find out its type and duration
            PickUpProperties pickupHit = col.GetComponent<PickUpProperties>();

            string activeType = pickupHit.type;
            
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

            if (activeType == "speed" && speedPowerUpActive == false)
            {
                playerKart.maxSpeed += 10f;
                playerKart.acceleration += 10f;

                speedPowerUpActive = true;

                // Set the timer for the speed power-up to 8 seconds
                speedPowerUpTimer = 8f;
            }

            else if (activeType == "freeze" && freezePowerUpActive == false)
            {
                // Find every kart in the scene by looking for objects of type KartController and not sorting them
                KartController[] allKarts = FindObjectsByType<KartController>(FindObjectsSortMode.None);

                // Freeze every kart except the player
                foreach (KartController kart in allKarts)
                {
                    if (kart != playerKart)
                    {
                        kart.SetFrozen(true);
                    }
                }

                freezePowerUpActive = true;

                // Freeze lasts 3 seconds
                freezePowerUpTimer = 3f;
            }

            else if (activeType == "gun" && gunPowerUpActive == false)
            {
                gunPowerUpActive = true;

                // Gun lasts 5 seconds
                gunPowerUpTimer = 5f;
            }

            else if (activeType == "missile" && missilePowerUpActive == false)
            {
                missilePowerUpActive = true;

                // Missile lasts 5 seconds
                missilePowerUpTimer = 5f;
            }

            else if (activeType == "regeneration" && regenerationPowerUpActive == false)
            {
                regenerationPowerUpActive = true;

                playerKart.RegenHealth();
            }
        }
    }

    void Update()
    {
        if (speedPowerUpActive)
        {
            speedPowerUpTimer -= Time.deltaTime;

            if (speedPowerUpTimer <= 0f)
            {
                playerKart.maxSpeed -= 10f;
                playerKart.acceleration -= 10f;
                speedPowerUpActive = false;
            }
        }
        
        if (freezePowerUpActive)
        {
            freezePowerUpTimer -= Time.deltaTime;

            if (freezePowerUpTimer <= 0f)
            {
                // Find all karts again 
                KartController[] allKarts = FindObjectsByType<KartController>(FindObjectsSortMode.None);

                // Unfreeze every kart except the player
                foreach (KartController kart in allKarts)
                {
                    if (kart != playerKart)
                    {
                        kart.SetFrozen(false);
                    }
                }

                freezePowerUpActive = false;
            }
        }
    }
}
