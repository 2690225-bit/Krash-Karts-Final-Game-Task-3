
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PowerUpInventoryUI : MonoBehaviour
{
    [Header("Inventory")]
    public PowerUpInventory inventory;

    [Header("Empty Slot Text")]
    [SerializeField] private string emptySlotLabel = "EMPTY";
    [SerializeField] private string emptySelectionName = "EMPTY";

    [SerializeField]
    private string emptySelectionDescription =
        "Select a slot containing a power-up.";

    // Stores the three slot elements.
    private readonly List<VisualElement> slots =
        new List<VisualElement>();

    // Stores the icon inside each slot.
    private readonly List<VisualElement> icons =
        new List<VisualElement>();

    // Stores the name label inside each slot.
    private readonly List<Label> slotNames =
        new List<Label>();

    // Elements in the large selected-power-up panel.
    private VisualElement selectedIcon;
    private VisualElement selectedIconCircle;
    private Label selectedName;
    private Label selectedDescription;

    private void Start()
    {
        // Find the UIDocument attached to this GameObject.
        UIDocument document = GetComponent<UIDocument>();

        if (document == null)
        {
            Debug.LogError(
                "PowerUpInventoryUI requires a UIDocument on the same GameObject."
            );

            enabled = false;
            return;
        }

        // Get the root of the UI Toolkit document.
        VisualElement root = document.rootVisualElement;

        // Find every element with the inventory-slot class.
        slots.Clear();
        icons.Clear();
        slotNames.Clear();

        slots.AddRange(
            root.Query<VisualElement>(
                className: "inventory-slot"
            ).ToList()
        );

        // Find the icon and name inside each slot.
        foreach (VisualElement slot in slots)
        {
            VisualElement icon =
                slot.Q<VisualElement>(
                    className: "power-up-icon"
                );

            Label label =
                slot.Q<Label>(
                    className: "slot-name"
                );

            if (icon == null || label == null)
            {
                Debug.LogError(
                    "Every inventory slot needs a power-up-icon and slot-name class."
                );

                enabled = false;
                return;
            }

            icons.Add(icon);
            slotNames.Add(label);
        }

        // Find the elements in the large selected-power-up panel.
        selectedIcon =
            root.Q<VisualElement>("SelectedIcon");

        selectedIconCircle =
            root.Q<VisualElement>("SelectedIconCircle");

        selectedName =
            root.Q<Label>("SelectedName");

        selectedDescription =
            root.Q<Label>("SelectedDescription");

        if (selectedIcon == null ||
            selectedIconCircle == null ||
            selectedName == null ||
            selectedDescription == null)
        {
            Debug.LogError(
                "A selected-power-up panel element is missing. Check the names in UI Builder."
            );

            enabled = false;
            return;
        }

        // Find the inventory automatically if it wasn't assigned.
        if (inventory == null)
        {
            inventory =
                FindFirstObjectByType<PowerUpInventory>();
        }

        if (inventory == null)
        {
            Debug.LogError(
                "PowerUpInventory was not found in the scene."
            );

            enabled = false;
            return;
        }

        // Make sure the UI and inventory have the same number of slots.
        if (slots.Count != inventory.SlotCount)
        {
            Debug.LogError(
                "The number of UI slots does not match the inventory slot count."
            );

            enabled = false;
            return;
        }

        // Refresh the UI whenever the inventory changes.
        inventory.OnInventoryChanged += RefreshUI;

        // Display the initial inventory.
        RefreshUI();
    }

    private void OnDestroy()
    {
        // Stop listening when this UI is destroyed.
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshUI;
        }
    }

    private void RefreshUI()
    {
        if (inventory == null)
        {
            return;
        }

        // Update every inventory slot.
        for (int i = 0; i < slots.Count; i++)
        {
            PowerUpDefinition definition =
                inventory.GetPowerUpAt(i);

            bool isSelected =
                inventory.selectedSlot == i;

            bool isEmpty = definition == null;

            // Highlight the selected slot using USS.
            slots[i].EnableInClassList(
                "selected-slot",
                isSelected
            );

            // Apply the empty-icon styling when necessary.
            icons[i].EnableInClassList(
                "empty-power-up",
                isEmpty
            );

            if (isEmpty)
            {
                slotNames[i].text = emptySlotLabel;

                // Remove any icon from the empty slot.
                icons[i].style.backgroundImage =
                    StyleKeyword.None;

                // Let the USS empty-icon colour take effect.
                icons[i].style.backgroundColor =
                    StyleKeyword.Null;
            }
            else
            {
                // Read the name and colour from the asset.
                slotNames[i].text =
                    definition.displayName;

                icons[i].style.backgroundColor =
                    definition.iconColor;

                // Read the image from the asset.
                SetIconImage(
                    icons[i],
                    definition.icon
                );
            }
        }

        // Get the power-up in the currently selected slot.
        PowerUpDefinition selectedDefinition =
            inventory.GetPowerUpAt(
                inventory.selectedSlot
            );

        if (selectedDefinition == null)
        {
            // Display the empty-selection information.
            selectedName.text = emptySelectionName;

            selectedDescription.text =
                emptySelectionDescription;

            selectedIcon.EnableInClassList(
                "empty-power-up",
                true
            );

            selectedIcon.style.backgroundImage =
                StyleKeyword.None;

            selectedIcon.style.backgroundColor =
                StyleKeyword.Null;
        }
        else
        {
            // Display information from the selected asset.
            selectedName.text =
                selectedDefinition.displayName;

            selectedDescription.text =
                selectedDefinition.description;

            selectedIcon.EnableInClassList(
                "empty-power-up",
                false
            );

            selectedIcon.style.backgroundColor =
                selectedDefinition.iconColor;

            SetIconImage(
                selectedIcon,
                selectedDefinition.icon
            );
        }
    }

    private void SetIconImage(
        VisualElement element,
        Sprite icon
    )
    {
        if (icon != null)
        {
            // Display the Sprite assigned to the power-up asset.
            element.style.backgroundImage =
                new StyleBackground(icon);
        }
        else
        {
            // Display no image if the asset has no Sprite.
            element.style.backgroundImage =
                StyleKeyword.None;
        }
    }
}
