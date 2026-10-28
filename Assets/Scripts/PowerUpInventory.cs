using UnityEngine;
using UnityEngine.InputSystem;

// The first power-up collected is always the first one used.
public class PowerUpInventory : MonoBehaviour
{
    // Choose the activation key in the Inspector (for example E).
    public Key useKey;

    // Exactly three slots, as requested. Empty slots contain null.
    private PowerUpDefinition[] slots = new PowerUpDefinition[3];
    private PowerUpManager manager;

    private void Start()
    {
        manager = GetComponent<PowerUpManager>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (useKey == Key.None)
        {
            return;
        }

        if (Keyboard.current[useKey].wasPressedThisFrame)
        {
            UseFirst();
        }
    }

    // The UI reads slots 0, 1 and 2 using this method.
    public PowerUpDefinition GetPowerUpAt(int number)
    {
        return slots[number];
    }

    public bool TryAdd(PowerUpDefinition powerUp)
    {
        if (powerUp == null)
        {
            return false;
        }

        // Find the first empty slot.
        for (int i = 0; i < 3; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = powerUp;
                return true;
            }
        }

        // All three slots are full.
        return false;
    }

    public void UseFirst()
    {
        if (slots[0] == null)
        {
            return;
        }

        if (manager == null)
        {
            return;
        }

        // Do not remove the item if it could not be activated.
        if (manager.UsePowerUp(slots[0]) == false)
        {
            return;
        }

        // Move the remaining power-ups one place to the left.
        slots[0] = slots[1];
        slots[1] = slots[2];
        slots[2] = null;
    }
}
