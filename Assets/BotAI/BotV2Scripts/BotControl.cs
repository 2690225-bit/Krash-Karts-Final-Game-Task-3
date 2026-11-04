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
    //raycasts detecting road mesh
    //Current motor Torque
    //Current brake torque
    //current steering angle
    [HideInInspector] public GameObject botObject;
    [HideInInspector] public Vector3 closestPosition;
    [HideInInspector] public float facingToRoadTangentDot;
    [HideInInspector] public float forwardProgress;
    [HideInInspector] public float steeringTargetChange;

    [SerializeField] public NeuralContainer botBrain;
    [SerializeField] public SplineContainer roadSpline;

    //Used for training only
    private NeuralScript motorControl;
    private NeuralScript brakeControl;
    private NeuralScript steeringControl;

    [Header("Object References")]
    [SerializeField] List<WheelCollider> drivingWheels;
    [SerializeField] List<WheelCollider> steeringWheels;
    [SerializeField] Rigidbody rb;

    [Header("Limits")]
    [SerializeField] Vector2 minMaxMotorTorque;
    [SerializeField] Vector2 minMaxBrakeTorque;
    [SerializeField] float maxSteeringAngle;
    [SerializeField] float angleLerpSpeed;
    [SerializeField] float torqueLerpSpeed;

    [Header("Misc")]
    [SerializeField] float brainMotorScalar = 10f;
    [SerializeField] float brainBrakeScalar = 3f;
    [SerializeField] float brainSteeringScalar = 2f;

    [Header("Bot selection (training only)")]
    [SerializeField] MeshRenderer bodyRenderer;
    [SerializeField] Material bodyDefaultMaterial;

    float motorTorque = 0f;
    float brakeTorque = 0f;
    float steeringAngle = 0f;

    

    float previousProgress = 0f;
    float previousSteeringTarget;

    List<float> inputs;

    bool hasStopped = false;

    public void SetControls(NeuralScript m, NeuralScript b, NeuralScript s)
    {
        motorControl = m;
        brakeControl = b;
        steeringControl = s;
    }

    private void Awake()
    {
        hasStopped = false;

        SplineUtility.GetNearestPoint(roadSpline.Spline, transform.position, out float3 nearest, out previousProgress, 4, 2);

        previousSteeringTarget = steeringAngle;

        if(botBrain != null)
        {
            NeuralScript m;
            NeuralScript b;
            NeuralScript s;
            botBrain.Read(out m, out b, out s);

            motorControl = m;
            brakeControl = b;
            steeringControl = s;
        }
    }

    public void Update()
    {
        if(hasStopped) { forwardProgress = 0f; return; }
        else
        {
            GetInputs();

            GetBrainOutput();
        }

        for(int i = 0; i < drivingWheels.Count; i++)
        {
            drivingWheels[i].motorTorque = Mathf.Lerp(drivingWheels[i].motorTorque, motorTorque, Time.deltaTime * torqueLerpSpeed);
            drivingWheels[i].brakeTorque = Mathf.Lerp(drivingWheels[i].brakeTorque, brakeTorque, Time.deltaTime * torqueLerpSpeed);
        }
        for(int i = 0; i < steeringWheels.Count; i++)
        {
            steeringWheels[i].steerAngle = Mathf.Lerp(steeringWheels[i].steerAngle, steeringAngle, Time.deltaTime * angleLerpSpeed);
        }

        Debug.DrawRay(transform.position + Vector3.up * 5f, transform.right * (steeringAngle / maxSteeringAngle) * 5f, Color.red);
    }

    private void GetBrainOutput()
    {
        float motor = motorControl.Calculate(inputs);
        float brake = brakeControl.Calculate(inputs);
        float steer = steeringControl.Calculate(inputs);

        motorTorque = Mathf.Clamp(motor * brainMotorScalar, minMaxMotorTorque.x, minMaxMotorTorque.y);
        brakeTorque = Mathf.Clamp(brake * brainBrakeScalar, minMaxBrakeTorque.x, minMaxBrakeTorque.y);
        steeringAngle = Mathf.Clamp(steer * brainSteeringScalar, -maxSteeringAngle, maxSteeringAngle);
    }

    public float GetVelocity()
    {
        return rb.linearVelocity.magnitude;
    }

    private void GetInputs()
    {
        float centerOfRoadDist;
        float dirToRoad;

        float3 point = transform.position;
        float3 nearest;
        float t;
        SplineUtility.GetNearestPoint(roadSpline.Spline, point, out nearest, out t, 4, 2);
        centerOfRoadDist = Vector3.Distance(point, nearest);
        closestPosition = nearest;

        float current = t;

        float d = current - previousProgress;

        previousProgress = current;

        if(d < 0.5f)
        {
            d += 1f;
        }
        else if(d > 0.5f)
        {
            d -= 1f;
        }

        forwardProgress = d * roadSpline.CalculateLength();

        float currentSteer = steeringAngle;
        float s = currentSteer - previousSteeringTarget;
        previousSteeringTarget = currentSteer;

        steeringTargetChange = s / maxSteeringAngle;

        dirToRoad = Mathf.Clamp01(Vector3.Dot(transform.forward, roadSpline.EvaluateTangent(t)));

        facingToRoadTangentDot = dirToRoad * Time.deltaTime;


        inputs = new List<float>();

        inputs.Add(dirToRoad);
        inputs.Add(centerOfRoadDist);
        inputs.Add(motorTorque);
        inputs.Add(brakeTorque);
        inputs.Add(steeringAngle);
    }

    public void SelectTraining(Material selectedMat)
    {
        bodyRenderer.material = selectedMat;
    }

    public void DeselectTraining()
    {
        bodyRenderer.material = bodyDefaultMaterial;
    }

    public void StopBot()
    {
        hasStopped = true;
        rb.isKinematic = true;   
        gameObject.SetActive(false);
    }
}
