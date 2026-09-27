using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    private bool speedPowerUpActive = false; 
    private bool freezePowerUpActive = false;
    private bool gunPowerUpActive = false;
    private bool missilePowerUpActive = false;
    private bool repairPowerUpActive = false;
    
    [Header("Camera Configuration")]
    public Camera playerCamera; 

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
                    activeType = "repair";
            }

            if (activeType == "speed" && speedPowerUpActive = false;)

        }
    }
}
