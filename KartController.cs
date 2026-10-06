using UnityEngine;
using UnityEngine.InputSystem;

public class KartController : MonoBehaviour
{
    public float acceleration = 30f;
    public float maxSpeed = 40f;
    public float turnSpeed = 90f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float forwardInput = 0f;
        float steeringInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.isPressed)
                forwardInput = 1f;

            if (Keyboard.current.downArrowKey.isPressed)
                forwardInput = -1f;

            if (Keyboard.current.leftArrowKey.isPressed)
                steeringInput = -1f;

            if (Keyboard.current.rightArrowKey.isPressed)
                steeringInput = 1f;
        }

        Vector3 force =
            transform.forward *
            forwardInput *
            acceleration;

        rb.AddForce(force, ForceMode.Acceleration);

        Vector3 horizontalVelocity =
            new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity =
                horizontalVelocity.normalized * maxSpeed;

            rb.linearVelocity = new Vector3(
                horizontalVelocity.x,
                rb.linearVelocity.y,
                horizontalVelocity.z
            );
        }

        if (horizontalVelocity.magnitude > 0.5f)
        {
            float turnAmount =
                steeringInput *
                turnSpeed *
                Time.fixedDeltaTime;

            transform.Rotate(0f, turnAmount, 0f);
        }
    }
}