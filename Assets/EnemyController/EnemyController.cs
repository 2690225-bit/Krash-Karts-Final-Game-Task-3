using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class EnemyController : MonoBehaviour
{
    //Use PID controller to handle bot movement
    [Header("References")]
    [SerializeField] SplineContainer roadSpline;
    [SerializeField] Rigidbody rb;

    [Header("Basic Controls")]
    [SerializeField] float botSpeed;
    [SerializeField] float maxTurnSpeed;

    [Header("PID Controls")]
    [SerializeField] float proportionalConstant;
    [SerializeField] float integralConstant;
    [SerializeField] float derivativeConstant;

    float anglefacing;
    Vector3 closest;

    private void Update()
    {
        //Move Car Forward
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0) + transform.forward * botSpeed;



        float3 botPosition = (float3)transform.position;
        float splineT;
        float3 outSplinePos;
        SplineUtility.GetNearestPoint(roadSpline.Spline, botPosition, out outSplinePos, out splineT, 4, 2);

        closest = outSplinePos;

        Vector3 tangent = (Vector3)roadSpline.EvaluateTangent(splineT);
        Vector3 to = Vector3.Normalize(tangent);

        Vector3 fwd = transform.TransformDirection(transform.forward);

        //dot is the facing angle, if dot is 1 then the bot is facing directly forward
        float dot = Vector3.Dot(fwd, to);

        anglefacing = dot;

        SteerCar();
    }

    private void SteerCar()
    {
        //Use PID here

        //Proportional: react to current error
        Vector3 dir = transform.InverseTransformDirection(closest - transform.position);
        //x < 0: left   - turn anticlockwise
        //x > 0: right  - turn clockwise

        float proportionalTurn = 0f;

        if(dir.x < 0)
        {
            proportionalTurn = -1f;
        }
        else
        {
            proportionalTurn = 1f;
        }

        proportionalTurn *= proportionalConstant;

        transform.Rotate(transform.up, proportionalTurn * maxTurnSpeed * Time.deltaTime);
    }
}
