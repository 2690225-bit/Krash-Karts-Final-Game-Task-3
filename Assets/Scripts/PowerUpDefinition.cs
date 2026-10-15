
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewPowerUp",
    menuName = "Krash Karts/Power-Up Definition"
)]
public class PowerUpDefinition : ScriptableObject
{
    [Header("Identification")]
    public string id;

    [Header("Display")]
    public string displayName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public Color iconColor = Color.white;

    [Header("Effect Settings")]
    public float duration = 5f;
    public float speedBonus = 10f;
    public float accelerationBonus = 10f;
    public float healthBonus = 60f;
}
