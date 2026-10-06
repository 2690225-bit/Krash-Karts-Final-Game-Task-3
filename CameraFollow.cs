using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Camera Position")]
    public Vector3 offset = new Vector3(0f, 5f, -8f);

    [Header("Camera Settings")]
    public float followSpeed = 8f;
    public float rotationSpeed = 8f;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Positions behind the kart
        Vector3 desiredPosition =
            target.position +
            target.TransformDirection(offset);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // Looks at the kart
        Vector3 lookPosition =
            target.position + Vector3.up * 1.5f;

        Quaternion desiredRotation =
            Quaternion.LookRotation(
                lookPosition - transform.position
            );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
