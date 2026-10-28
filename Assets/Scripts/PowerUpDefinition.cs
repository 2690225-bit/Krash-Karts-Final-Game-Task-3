using UnityEngine;

public enum PowerUpType
{
    None = 0,
    Speed = 1,
    Freeze = 2,
    Gun = 3,
    Missile = 4,
    Regeneration = 5,
    Shield = 6,
    Reflection = 7
}

// Adds an Editor menu for making power-up assets. Gameplay values stay in the Inspector.
[CreateAssetMenu(fileName = "NewPowerUp", menuName = "Krash Karts/Power Up")]
public class PowerUpDefinition : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public Color iconColor = Color.white;
    public PowerUpType type;

    public float duration;
    public float speedBonus;
    public float accelerationBonus;

    // Health Boost uses healthBonus.
    public float healthBonus;

    public float reflectionFraction;

    public PowerUpProjectile projectilePrefab;
    public float damage;
    public int shotsPerUse;
    public float shotInterval;
    public float projectileSpeed;
    public float projectileLifetime;
}
