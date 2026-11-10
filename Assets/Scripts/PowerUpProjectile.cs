using UnityEngine;

public class PowerUpProjectile : MonoBehaviour
{
    private Rigidbody rb;
    private KartController shooter;
    private float damage;
    private float speed;
    private bool fired;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(KartController owner, PowerUpDefinition powerUp)
    {
        shooter = owner;
        damage = powerUp.damage;
        speed = powerUp.projectileSpeed;
        fired = true;

        Destroy(gameObject, powerUp.projectileLifetime);
    }

    private void FixedUpdate()
    {
        if (fired == false || rb == null)
        {
            return;
        }

        // Move straight in the direction it was facing when fired.
        Vector3 movement = transform.forward * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (fired == false)
        {
            return;
        }

        if (other.isTrigger)
        {
            return;
        }

        KartController target = other.GetComponentInParent<KartController>();

        // The shooter must not hit their own kart.
        if (target == shooter)
        {
            return;
        }

        if (target != null)
        {
            target.TakeDamage(damage, shooter);
        }

        // Destroy after hitting either a kart or a solid wall.
        fired = false;
        Destroy(gameObject);
    }
}
