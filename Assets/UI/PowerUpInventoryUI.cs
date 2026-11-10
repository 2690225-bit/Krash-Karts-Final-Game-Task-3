using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Add this script to an empty GameObject, then assign the player's inventory.
// For a player spawned during Play, enter its existing unique tag instead.
// This script creates the Canvas HUD when you press Play.
[RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
[DisallowMultipleComponent]
public class PowerUpInventoryUI : MonoBehaviour
{
    public PowerUpInventory inventory;

    // Optional: only needed when the player is created after the scene starts.
    public string playerTag = "";

    private KartController kart;
    private PowerUpManager manager;
    private Font font;

    private Image healthFill;
    private Text healthText;
    private Image[] slotIcons = new Image[3];
    private Text[] slotNames = new Text[3];
    private Image[] slotBackgrounds = new Image[3];
    private Text useHint;
    private Text activeEffectText;

    // Only the appearance of the UI is hardcoded here.
    private Color panelColor = new Color(0.08f, 0.11f, 0.16f, 0.95f);
    private Color emptySlotColor = new Color(0.16f, 0.20f, 0.27f, 1f);
    private Color nextSlotColor = new Color(0.12f, 0.38f, 0.31f, 1f);

    private void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Always create the HUD, even if the player has not appeared yet.
        CreateHealthBar();
        CreateInventory();
        ShowWaitingUI();
    }

    private void LateUpdate()
    {
        if (TryConnectToPlayer() == false)
        {
            ShowWaitingUI();
            return;
        }

        UpdateHealthBar();
        UpdateInventory();
        UpdateMessages();
    }

    private bool TryConnectToPlayer()
    {
        // An inventory assigned in the Inspector always takes priority.
        // Only search while no inventory is connected.
        if (inventory == null && string.IsNullOrEmpty(playerTag) == false)
        {
            GameObject player = GameObject.FindWithTag(playerTag);

            if (player != null)
            {
                inventory = player.GetComponent<PowerUpInventory>();
            }
        }

        if (inventory == null)
        {
            kart = null;
            manager = null;
            return false;
        }

        // Refresh these references if the inventory changes or the kart respawns.
        if (kart == null || manager == null ||
            kart.gameObject != inventory.gameObject ||
            manager.gameObject != inventory.gameObject)
        {
            kart = inventory.GetComponent<KartController>();
            manager = inventory.GetComponent<PowerUpManager>();
        }

        return kart != null && manager != null;
    }

    private void ShowWaitingUI()
    {
        healthFill.rectTransform.anchorMax = new Vector2(0f, 1f);
        healthText.text = "Waiting for player";

        for (int i = 0; i < 3; i++)
        {
            slotNames[i].text = "EMPTY";
            slotIcons[i].enabled = false;
            slotBackgrounds[i].color = emptySlotColor;
        }

        if (inventory != null)
        {
            activeEffectText.text = "Check the player's components";
            useHint.text = "The inventory needs KartController and PowerUpManager.";
        }
        else if (string.IsNullOrEmpty(playerTag))
        {
            activeEffectText.text = "Player inventory is not connected";
            useHint.text = "Assign Inventory, or enter the player's existing tag.";
        }
        else
        {
            activeEffectText.text = "Waiting for the player to appear";
            useHint.text = "Looking for an inventory on the object tagged " + playerTag;
        }
    }

    private void CreateHealthBar()
    {
        Image panel = CreateImage("Health Panel", transform,
            new Vector2(24f, -24f), new Vector2(300f, 72f), panelColor);

        CreateText("Health Label", panel.transform, "HEALTH",
            new Vector2(12f, -8f), new Vector2(276f, 22f), 18);

        Image background = CreateImage("Health Background", panel.transform,
            new Vector2(12f, -34f), new Vector2(276f, 26f), emptySlotColor);

        healthFill = CreateImage("Health Fill", background.transform,
            Vector2.zero, Vector2.zero, new Color(0.2f, 0.75f, 0.35f, 1f));

        // Stretch the fill to its parent. Later, anchorMax.x controls its width.
        healthFill.rectTransform.anchorMin = Vector2.zero;
        healthFill.rectTransform.anchorMax = Vector2.one;
        healthFill.rectTransform.offsetMin = Vector2.zero;
        healthFill.rectTransform.offsetMax = Vector2.zero;

        healthText = CreateText("Health Number", background.transform, "",
            Vector2.zero, new Vector2(276f, 26f), 18);
        healthText.alignment = TextAnchor.MiddleCenter;
    }

    private void CreateInventory()
    {
        Image panel = CreateImage("Inventory Panel", transform,
            new Vector2(0f, 24f), new Vector2(510f, 210f), panelColor);

        // Place the inventory at the bottom centre of the screen.
        RectTransform rect = panel.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 24f);

        CreateText("Inventory Label", panel.transform, "POWER-UPS",
            new Vector2(12f, -8f), new Vector2(486f, 22f), 18);

        for (int i = 0; i < 3; i++)
        {
            slotBackgrounds[i] = CreateImage("Slot " + (i + 1), panel.transform,
                new Vector2(12f + i * 162f, -38f), new Vector2(150f, 100f), emptySlotColor);

            Transform slot = slotBackgrounds[i].transform;
            string label = "SLOT " + (i + 1);

            if (i == 0)
            {
                label = "NEXT";
            }

            Text slotLabel = CreateText("Slot Label", slot, label,
                new Vector2(5f, -4f), new Vector2(140f, 20f), 14);
            slotLabel.alignment = TextAnchor.MiddleCenter;

            slotIcons[i] = CreateImage("Icon", slot,
                new Vector2(55f, -26f), new Vector2(40f, 40f), Color.white);
            slotIcons[i].preserveAspect = true;
            slotIcons[i].enabled = false;

            slotNames[i] = CreateText("Name", slot, "EMPTY",
                new Vector2(5f, -70f), new Vector2(140f, 26f), 14);
            slotNames[i].alignment = TextAnchor.MiddleCenter;
        }

        activeEffectText = CreateText("Active Effect", panel.transform, "",
            new Vector2(12f, -146f), new Vector2(486f, 22f), 15);
        activeEffectText.alignment = TextAnchor.MiddleCenter;

        useHint = CreateText("Use Hint", panel.transform, "",
            new Vector2(12f, -176f), new Vector2(486f, 22f), 15);
        useHint.alignment = TextAnchor.MiddleCenter;
    }

    private void UpdateHealthBar()
    {
        float fraction = 0f;

        if (kart.maxHealth > 0f)
        {
            fraction = Mathf.Clamp01(kart.health / kart.maxHealth);
        }

        healthFill.rectTransform.anchorMax = new Vector2(fraction, 1f);
        healthText.text = Mathf.CeilToInt(kart.health) + " / " + Mathf.CeilToInt(kart.maxHealth);
    }

    private void UpdateInventory()
    {
        for (int i = 0; i < 3; i++)
        {
            PowerUpDefinition powerUp = inventory.GetPowerUpAt(i);
            slotBackgrounds[i].color = emptySlotColor;

            if (powerUp == null)
            {
                slotNames[i].text = "EMPTY";
                slotIcons[i].enabled = false;
            }
            else
            {
                slotNames[i].text = powerUp.displayName;
                slotIcons[i].sprite = powerUp.icon;
                slotIcons[i].color = powerUp.iconColor;
                slotIcons[i].enabled = powerUp.icon != null;

                if (i == 0)
                {
                    slotBackgrounds[i].color = nextSlotColor;
                }
            }
        }
    }

    private void UpdateMessages()
    {
        PowerUpDefinition active = manager.GetActivePowerUp();

        if (active == null)
        {
            activeEffectText.text = "";

            if (inventory.useKey == Key.None)
            {
                useHint.text = "Choose a Use Key on the player's inventory.";
            }
            else
            {
                useHint.text = "Press " + inventory.useKey + " to use the first power-up";
            }
        }
        else if (active.type == PowerUpType.Gun || active.type == PowerUpType.Missile)
        {
            activeEffectText.text = active.displayName + " - Shots left: " + manager.GetShotsLeft();
            useHint.text = "Wait until shooting finishes";
        }
        else
        {
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, manager.GetTimeLeft()));
            activeEffectText.text = active.displayName + " - " + seconds + "s left";
            useHint.text = "Wait for the current power-up to finish";
        }
    }

    // Both Images and Text need a RectTransform to set their size and position.
    private GameObject CreateUIObject(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        return obj;
    }

    private Image CreateImage(string name, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject obj = CreateUIObject(name, parent, position, size);
        Image image = obj.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private Text CreateText(string name, Transform parent, string words, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject obj = CreateUIObject(name, parent, position, size);
        Text text = obj.AddComponent<Text>();
        text.font = font;
        text.text = words;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }
}
