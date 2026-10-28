
using UnityEngine;

public class PowerUpProperties : MonoBehaviour
{
    [Header("Power-Up Settings")]
    public PowerUpDefinition definition;

    [Header("Collection Settings")]
    public bool disappearOnCollect = true;

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        // Prevent the same power-up being collected twice.
        if (collected)
            return;

        // Find the inventory on the kart or its parent.
        PowerUpInventory inventory =
            other.GetComponentInParent<PowerUpInventory>();

        // Also check the collider's children if needed.
        if (inventory == null)
        {
            inventory = other.GetComponentInChildren<PowerUpInventory>();
        }

        // This collision wasn't with a kart that has an inventory.
        if (inventory == null)
            return;

        // Don't collect a power-up if its definition is missing.
        if (definition == null)
        {
            Debug.LogWarning(
                "PowerUpProperties: No PowerUpDefinition assigned!",
                gameObject
            );
            return;
        }

        // Add the power-up to the first available inventory slot.
        bool added = inventory.TryAdd(definition);

        if (!added)
        {
            Debug.Log("Power-up inventory is full!");
            return;
        }

        // Mark it collected and make the box disappear.
        collected = true;

        Debug.Log(
            "Collected " + definition.name +
            " into " + inventory.gameObject.name + "'s inventory."
        );

        if (disappearOnCollect)
        {
            gameObject.SetActive(false);
        }
    }
}
