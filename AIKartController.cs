using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class AIKartController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SplineContainer roadSpline;
    [SerializeField] private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float turnSpeed = 65f;

    [Header("Track Following")]
    [SerializeField] private float lookAhead = 0.025f;
    [SerializeField] private float steeringSensitivity = 45f;

    [Header("Grip")]
    [SerializeField] private float sideGrip = 4f;

    private bool _raceStarted;
    private Transform _body;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb == null)
            rb = GetComponentInParent<Rigidbody>();

        if (rb == null)
            rb = GetComponentInChildren<Rigidbody>(true);

        if (rb == null)
        {
            Debug.LogError("AIKartController: Rigidbody not found!", gameObject);
            enabled = false;
            return;
        }

        _body = rb.transform;
    }

    public void SetRoadSpline(SplineContainer spline)
    {
        roadSpline = spline;

        if (roadSpline == null)
            Debug.LogError("AIKartController: Road spline is missing!", gameObject);
    }

    public void SetDifficulty(float newAcceleration, float newMaxSpeed, float newTurnSpeed)
    {
        acceleration = newAcceleration;
        maxSpeed = newMaxSpeed;
        turnSpeed = newTurnSpeed;
    }

    public void SetSpawnPose(Transform spawnPoint)
    {
        if (spawnPoint == null || _body == null)
            return;

        transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (_body != transform)
        {
            _body.SetPositionAndRotation(
                spawnPoint.position,
                spawnPoint.rotation
            );
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (!_raceStarted || rb == null || roadSpline == null || _body == null)
            return;

        // Convert the kart's world position into spline-local coordinates.
        Vector3 localKartPosition =
            roadSpline.transform.InverseTransformPoint(_body.position);

        float3 nearestLocalPosition;
        float splineT;

        SplineUtility.GetNearestPoint(
            roadSpline.Spline,
            (float3)localKartPosition,
            out nearestLocalPosition,
            out splineT,
            8,
            4
        );

        float targetT = splineT + lookAhead;

        if (roadSpline.Spline.Closed)
        {
            targetT %= 1f;
        }
        else
        {
            targetT = Mathf.Clamp01(targetT);
        }

        // EvaluatePosition on the container gives the world-space point.
        Vector3 targetPosition = roadSpline.EvaluatePosition(targetT);

        Vector3 direction = targetPosition - _body.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        direction.Normalize();

        float angle = Vector3.SignedAngle(
            _body.forward,
            direction,
            Vector3.up
        );

        float steering = Mathf.Clamp(
            angle / Mathf.Max(steeringSensitivity, 0.01f),
            -1f,
            1f
        );

        rb.AddForce(
            _body.forward * acceleration,
            ForceMode.Acceleration
        );

        Vector3 localVelocity =
            _body.InverseTransformDirection(rb.linearVelocity);

        localVelocity.x *= Mathf.Clamp01(
            1f - sideGrip * Time.fixedDeltaTime
        );

        rb.linearVelocity =
            _body.TransformDirection(localVelocity);

        Vector3 flatVelocity = rb.linearVelocity;
        flatVelocity.y = 0f;

        float speed = flatVelocity.magnitude;

        if (speed > 0.2f)
        {
            float speedFactor = Mathf.Clamp01(speed / Mathf.Max(maxSpeed, 0.01f));

            float turnAmount =
                steering * turnSpeed * speedFactor * Time.fixedDeltaTime;

            rb.MoveRotation(
                rb.rotation * Quaternion.Euler(0f, turnAmount, 0f)
            );
        }

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;

            rb.linearVelocity = new Vector3(
                horizontalVelocity.x,
                rb.linearVelocity.y,
                horizontalVelocity.z
            );
        }
    }

    public void StartDriving()
    {
        if (roadSpline == null)
        {
            Debug.LogError(
                "AIKartController: Cannot drive because Road Spline is missing!",
                gameObject
            );
            return;
        }

        _raceStarted = true;
    }

    public void StopDriving()
    {
        _raceStarted = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}