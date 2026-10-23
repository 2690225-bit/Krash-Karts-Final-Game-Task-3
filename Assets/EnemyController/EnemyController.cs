using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class EnemyController : MonoBehaviour
{
    //Use PID controller to handle bot movement
    [Header("References")]
    [SerializeField] SplineContainer roadSpline;
    [SerializeField] Rigidbody rb;
    [SerializeField] WheelCollider[] wheels;
    [SerializeField] WheelCollider[] steeringWheels;

    [Header("Basic Controls")]
    [SerializeField] float minSteerBackDistance;
    [SerializeField] float maxSteerBackDistance;
    [SerializeField] float maxSteerBackAngle;
    [SerializeField] float baseTorque;
    [SerializeField] float slowTorque;
    [SerializeField] float maxTurnAngle;
    [SerializeField] float minAngleToSlowDown;
    [SerializeField, Range(0, 1)] float lookaheadTime;
    [SerializeField] float maximumSpeedAfterBraking;
    [SerializeField] float brakingTorque;

    Vector3 closest;
    float closestT;

    float turnAngle;
    bool shouldSlow;

    private void Update()
    {
        Debug.Log(rb.linearVelocity.magnitude);

        //Move Car Forward
        for (int i = 0; i < wheels.Length; i++)
        {
            if (shouldSlow && rb.linearVelocity.magnitude > maximumSpeedAfterBraking)
            {
                wheels[i].brakeTorque = brakingTorque;
            }
            else if (shouldSlow)
            {
                wheels[i].brakeTorque = 0f;
                wheels[i].motorTorque = slowTorque;
            }
            else
            {
                wheels[i].motorTorque = baseTorque;
            }
        }

        float3 botPosition = (float3)transform.position;
        float splineT;
        float3 outSplinePos;
        SplineUtility.GetNearestPoint(roadSpline.Spline, botPosition, out outSplinePos, out splineT, 4, 2);

        closest = outSplinePos;
        closestT = splineT;

        Vector3 tangent = (Vector3)roadSpline.EvaluateTangent(splineT);
        Vector3 to = Vector3.Normalize(tangent);

        Vector3 fwd = transform.TransformDirection(transform.forward);

        SteerCar();
    }

    private void SteerCar()
    {
        Vector3 botForward = transform.forward;
        Vector3 roadForward = roadSpline.EvaluateTangent(closestT);
        Vector3 lookaheadRoadForward = roadSpline.EvaluateTangent(Mathf.Clamp01(closestT + lookaheadTime));

        //if bot is greater than x dist from center then steer back.
        //if bot is less than x dist from center then even out angle

        //project both angles onto road plane (road forward already on plane)
        //then calculate angle between both the vectors
        Vector3 roadNormal = roadSpline.EvaluateUpVector(closestT);

        Vector3 botFwdProjected = Vector3.ProjectOnPlane(botForward, roadNormal);

        //should return angle between -180 and 180
        float angleDifference = Vector3.SignedAngle(botFwdProjected, roadForward, roadNormal);
        float absAngleDifference = Mathf.Abs(angleDifference);

        float expectedAngleDifference = Vector3.SignedAngle(roadForward, lookaheadRoadForward, roadNormal);

        float lookaheadAngleDifference = Vector3.SignedAngle(botFwdProjected, lookaheadRoadForward, roadNormal);
        float absLookaheadAngleDifference = Mathf.Abs(lookaheadAngleDifference);


        Vector3 dirToRoad = transform.InverseTransformDirection(closest - transform.position);
        float distToRoad = dirToRoad.x;
        //x < 0: left   - turn anticlockwise
        //x > 0: right  - turn clockwise

        float steerBackMultiplier = (Mathf.Abs(distToRoad) - minSteerBackDistance) / (maxSteerBackDistance - minSteerBackDistance);

        float directionMulti = 0f;

        if(distToRoad < -minSteerBackDistance && angleDifference < maxSteerBackAngle)
        {
            directionMulti = -1f * steerBackMultiplier;
        }
        else if(distToRoad > minSteerBackDistance && angleDifference > -maxSteerBackAngle)
        {
            directionMulti = 1f * steerBackMultiplier;
        }
        else if(distToRoad < minSteerBackDistance && distToRoad > - minSteerBackDistance) //if inside center region then steer to match road tangent
        {
            //Steers the car to face the tangent of the road            

            directionMulti = Mathf.Clamp(angleDifference, -1, 1);
        }

        //Signal car to slow down for sharp turn
        if(Mathf.Abs(expectedAngleDifference) >= minAngleToSlowDown)
        {
            Debug.Log($"Slowing Down, Angle Detected: {expectedAngleDifference}");
            shouldSlow = true;
        }
        else
        {
            shouldSlow = false;
        }

        //Turn the car
        for (int i = 0; i < steeringWheels.Length; i++)
        {
            steeringWheels[i].steerAngle = maxTurnAngle * directionMulti;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 closePos = roadSpline.EvaluatePosition(closestT);
        Vector3 aheadPos = roadSpline.EvaluatePosition(Mathf.Clamp01(closestT + lookaheadTime));

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(closePos, 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(aheadPos, 0.5f);
    }
}
