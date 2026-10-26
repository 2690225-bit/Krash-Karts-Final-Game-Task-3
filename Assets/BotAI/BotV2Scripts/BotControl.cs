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

    [Header("Misc")]
    [SerializeField] int lookAheadPoints;
    [SerializeField, Range(0f, 1f)] float lookAheadTimeDistance;

    float motorTorque;
    float brakeTorque;
    float steeringAngle;

    List<float> inputs;

    public void Update()
    {
        GetInputs();

        GetBrainOutput();

        foreach(WheelCollider wheel in drivingWheels)
        {
            wheel.motorTorque = motorTorque;
            wheel.brakeTorque = brakeTorque;
        }
        foreach(WheelCollider steeringWheel in steeringWheels)
        {
            steeringWheel.steerAngle = steeringAngle;
        }
    }

    private void GetBrainOutput()
    {
        //Only for training, stops the creation of hundreds of scriptable objects
        if(botBrain == null)
        {
            motorTorque = Mathf.Clamp(motorControl.Calculate(inputs), minMaxMotorTorque.x, minMaxMotorTorque.y);
            brakeTorque = Mathf.Clamp(brakeControl.Calculate(inputs), minMaxBrakeTorque.x, minMaxMotorTorque.y);
            steeringAngle = Mathf.Clamp(steeringControl.Calculate(inputs), -steeringAngle, steeringAngle);

            return;
        }

        motorTorque = Mathf.Clamp(botBrain.MotorTorqueNetwork.Calculate(inputs), minMaxMotorTorque.x, minMaxMotorTorque.y);
        brakeTorque = Mathf.Clamp(botBrain.BrakeTorqueNetwork.Calculate(inputs), minMaxBrakeTorque.x, minMaxMotorTorque.y);
        steeringAngle = Mathf.Clamp(botBrain.SteeringAngleNetwork.Calculate(inputs), -steeringAngle, steeringAngle);
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
            Vector3 tangent = roadSpline.EvaluateTangent(Mathf.Repeat(t + lookAheadTimeDistance * i, 1f));

            lookAheadRelativeDirections.Add(Vector3.Dot(transform.forward, tangent));
        }

        inputs = new List<float>();

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
