using UnityEngine;
public class PowerUpProperties : MonoBehaviour
{
    public PowerUpDefinition definition;

    private void OnTriggerEnter(Collider other)
    {
        // Find the inventory on the kart that entered the trigger.
        PowerUpInventory inventory = other.GetComponentInParent<PowerUpInventory>();

        if (inventory == null)
        {
            return;
        }

        // Only remove the pickup if there was room in the inventory.
        if (inventory.TryAdd(definition))
        {
            gameObject.SetActive(false);
        }
    }
}
