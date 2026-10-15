
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PowerUpInventory : MonoBehaviour
{
    [Header("Inventory Settings")]

    // Set the number of inventory slots in the Inspector.
    [SerializeField, Min(1)]
    private int slotCount = 3;

    // Stores the power-ups currently held by the player.
    private PowerUpDefinition[] powerUps;

    // The selected slot: 0 = first, 1 = second, 2 = third.
    public int selectedSlot = 0;

    // Handles the effects of power-ups.
    private PowerUpManager powerUpManager;

    // Notifies the UI when the inventory changes.
    public event Action OnInventoryChanged;

    // Reports the actual number of inventory slots.
    public int SlotCount =>
        powerUps != null
            ? powerUps.Length
            : Mathf.Max(1, slotCount);

    private void Awake()
    {
        // Create the inventory using the chosen slot count.
        powerUps = new PowerUpDefinition[
            Mathf.Max(1, slotCount)
        ];

        // Find the PowerUpManager on the same GameObject.
        powerUpManager = GetComponent<PowerUpManager>();
    }

    private void Update()
    {
        // Check that the keyboard is available.
        if (Keyboard.current == null)
        {
            return;
        }

        // Select a slot using the number keys.
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SelectSlot(0);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SelectSlot(1);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            SelectSlot(2);
        }

        // Press E to use the selected power-up.
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            UseSelectedPowerUp();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Find the pickup script on the collider or its parent.
        PowerUpProperties pickup =
            other.GetComponentInParent<PowerUpProperties>();

        if (pickup == null)
        {
            return;
        }

        // Make sure the pickup or its parent has the PickUp tag.
        if (!other.CompareTag("PickUp") &&
            !pickup.CompareTag("PickUp"))
        {
            return;
        }

        // Get the power-up's ScriptableObject asset.
        PowerUpDefinition definition =
            pickup.definition;

        if (definition == null)
        {
            Debug.LogWarning(
                "This pickup has no Power-Up Definition assigned!",
                pickup.gameObject
            );

            return;
        }

        // Add the power-up if there is an empty slot.
        if (AddPowerUp(definition))
        {
            // Hide the pickup after collecting it.
            pickup.gameObject.SetActive(false);
        }
    }

    public PowerUpDefinition GetPowerUpAt(int slot)
    {
        // Prevent invalid slot numbers.
        if (slot < 0 || slot >= powerUps.Length)
        {
            return null;
        }

        return powerUps[slot];
    }

    public void SelectSlot(int slot)
    {
        // Ignore invalid slot numbers.
        if (slot < 0 || slot >= powerUps.Length)
        {
            return;
        }

        // Remember which slot is selected.
        selectedSlot = slot;

        // Tell the UI to update its highlight and details.
        OnInventoryChanged?.Invoke();
    }

    public bool AddPowerUp(PowerUpDefinition definition)
    {
        if (definition == null)
        {
            return false;
        }

        // Find the first empty slot.
        for (int i = 0; i < powerUps.Length; i++)
        {
            if (powerUps[i] == null)
            {
                powerUps[i] = definition;

                // Tell the UI that a power-up was collected.
                OnInventoryChanged?.Invoke();

                return true;
            }
        }

        // Do not collect the item if every slot is full.
        Debug.Log("Power-up inventory is full!");

        return false;
    }

    public void UseSelectedPowerUp()
    {
        // Check that the selected slot is valid.
        if (selectedSlot < 0 ||
            selectedSlot >= powerUps.Length)
        {
            return;
        }

        // Get the power-up in the selected slot.
        PowerUpDefinition definition =
            powerUps[selectedSlot];

        // Do nothing if the slot is empty.
        if (definition == null)
        {
            return;
        }

        // Make sure the player has a PowerUpManager.
        if (powerUpManager == null)
        {
            Debug.LogError(
                "PowerUpManager is missing from the player!"
            );

            return;
        }

        // Activate the power-up.
        // Remove it from inventory only if activation succeeds.
        if (powerUpManager.UsePowerUp(definition))
        {
            powerUps[selectedSlot] = null;

            // Tell the UI to show the now-empty slot.
            OnInventoryChanged?.Invoke();
        }
    }
}
