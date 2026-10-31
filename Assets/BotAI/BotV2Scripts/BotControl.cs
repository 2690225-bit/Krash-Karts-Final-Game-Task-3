using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class BotControl : MonoBehaviour
{
    //Network Outputs:
    //Motor Torque (up to max)
    //Brake Torque (up to max)
    //Steering angle (up to max)

    //Network Inputs:
    //Distance From Center of Road
    //Velocity Magnitude
    //Dot( current car direction, road tangent (from multiple point further up) )
    //Current motor Torque
    //Current brake torque
    //current steering angle
    [HideInInspector] public GameObject botObject;
    [HideInInspector] public Vector3 closestPosition;
    [HideInInspector] public float facingToRoadTangentDot;
    [HideInInspector] public float distanceTravelledThisFrame;

    private Vector3 previousFramePosition;

    [SerializeField] public NeuralContainer botBrain;
    [SerializeField] public SplineContainer roadSpline;

    //Used for training only
    [HideInInspector] public NeuralScript motorControl;
    [HideInInspector] public NeuralScript brakeControl;
    [HideInInspector] public NeuralScript steeringControl;

    [Header("Object References")]
    [SerializeField] List<WheelCollider> drivingWheels;
    [SerializeField] List<WheelCollider> steeringWheels;
    [SerializeField] Rigidbody rb;

    [Header("Limits")]
    [SerializeField] Vector2 minMaxMotorTorque;
    [SerializeField] Vector2 minMaxBrakeTorque;
    [SerializeField] float maxSteeringAngle;
    [SerializeField] float minimumHeight;

    [Header("Misc")]
    [SerializeField] int lookAheadPoints;
    [SerializeField] float lookAheadDistance;
    [SerializeField] float brainMotorScalar = 10f;
    [SerializeField] float brainBrakeScalar = 3f;
    [SerializeField] float brainSteeringScalar = 2f;

    float motorTorque = 0f;
    float brakeTorque = 0f;
    float steeringAngle = 0f;

    List<float> inputs;

    private void Start()
    {
        previousFramePosition = transform.position;

        if(botBrain != null)
        {
            botBrain.Read(out motorControl, out brakeControl, out steeringControl);
        }
    }

    public void Update()
    {
        //Completely disables the car if it falls off the track
        if(transform.position.y < minimumHeight)
        {
            motorTorque = 0f;
            brakeTorque = minMaxBrakeTorque.y;
            steeringAngle = 0f;
        }
        else
        {
            GetInputs();

            GetBrainOutput();
        }


        for(int i = 0; i < drivingWheels.Count; i++)
        {
            drivingWheels[i].motorTorque = motorTorque;
            //drivingWheels[i].brakeTorque = brakeTorque;
        }
        for(int i = 0; i < steeringWheels.Count; i++)
        {
            steeringWheels[i].steerAngle = steeringAngle;
        }

        Debug.Log($"{drivingWheels[0].motorTorque} : {drivingWheels[0].brakeTorque} : {steeringWheels[0].steerAngle}");

        float d = Vector3.Distance(transform.position, previousFramePosition);
        previousFramePosition = transform.position;
        distanceTravelledThisFrame = d;
    }

    private void GetBrainOutput()
    {
        float motor = motorControl.Calculate(inputs);
        //float brake = brakeControl.Calculate(inputs);
        float steer = steeringControl.Calculate(inputs);

        motorTorque = Mathf.Clamp(motor * brainMotorScalar, minMaxMotorTorque.x, minMaxMotorTorque.y);
        //brakeTorque = Mathf.Clamp(brake * brainBrakeScalar, minMaxBrakeTorque.x, minMaxBrakeTorque.y);
        steeringAngle = Mathf.Clamp(steer * brainSteeringScalar, -maxSteeringAngle, maxSteeringAngle);
    }

    public float GetVelocity()
    {
        return rb.linearVelocity.magnitude;
    }

    private void GetInputs()
    {
        float centerOfRoadDist;
        float velocity;
        List<float> lookAheadRelativeDirections = new List<float>();
        float currentMotorTorque;
        float currentBrakeTorque;
        float currentSteeringAngle;


        velocity = rb.linearVelocity.magnitude;
        currentMotorTorque = motorTorque;
        currentBrakeTorque = brakeTorque;
        currentSteeringAngle = steeringAngle;

        float3 point = transform.position;

        float3 nearest;
        float t;
        SplineUtility.GetNearestPoint(roadSpline.Spline, point, out nearest, out t, 4, 2);

        centerOfRoadDist = Vector3.Distance(point, nearest);

        for(int i = 0; i < lookAheadPoints; i++)
        {
            Vector3 tangent = roadSpline.EvaluateTangent(Mathf.Repeat(t + Mathf.Repeat(lookAheadDistance / roadSpline.CalculateLength(), 1f) * i, 1f));

            lookAheadRelativeDirections.Add(Vector3.Dot(transform.forward, tangent));
        }

        inputs = new List<float>();

        closestPosition = nearest;
        facingToRoadTangentDot = lookAheadRelativeDirections[0];

        inputs.Add(centerOfRoadDist);
        inputs.Add(velocity);
        inputs.Add(currentMotorTorque);
        inputs.Add(currentBrakeTorque);
        inputs.Add(currentSteeringAngle);
        foreach(float f in lookAheadRelativeDirections)
        {
            inputs.Add(f);
        }
    }
}
