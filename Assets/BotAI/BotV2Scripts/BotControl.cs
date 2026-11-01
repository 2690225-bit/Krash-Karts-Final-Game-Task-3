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
    [HideInInspector] public float distanceTravelledThisFrame;

    private Vector3 previousFramePosition;

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
    [SerializeField] float minimumHeight;

    [Header("Misc")]
    [SerializeField] int lookAheadPoints;
    [SerializeField] float lookAheadDistance;
    [SerializeField] float brainMotorScalar = 10f;
    [SerializeField] float brainBrakeScalar = 3f;
    [SerializeField] float brainSteeringScalar = 2f;
    [SerializeField] Vector3 raycastOffset;
    [SerializeField] float roadRaycastLength;
    [SerializeField] float roadRaycastSpreadAngle;
    [SerializeField] float roadRaycastDownAngle;
    [SerializeField] LayerMask roadRaycastLayer;

    [Header("Bot selection (training only)")]
    [SerializeField] MeshRenderer bodyRenderer;
    [SerializeField] Material bodyDefaultMaterial;

    float motorTorque = 0f;
    float brakeTorque = 0f;
    float steeringAngle = 0f;

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
        previousFramePosition = transform.position;

        if(botBrain != null)
        {
            botBrain.Read(out motorControl, out brakeControl, out steeringControl);
        }
    }

    public void Update()
    {
        if(hasStopped) { return; }
        else
        {
            GetInputs();

            GetBrainOutput();
        }

        for(int i = 0; i < drivingWheels.Count; i++)
        {
            drivingWheels[i].motorTorque = motorTorque;
            drivingWheels[i].brakeTorque = brakeTorque;
        }
        for(int i = 0; i < steeringWheels.Count; i++)
        {
            steeringWheels[i].steerAngle = steeringAngle;
        }

        Debug.DrawRay(transform.position + Vector3.up * 5f, transform.right * (steeringAngle / maxSteeringAngle) * 5f, Color.red);


        float d = Vector3.Distance(transform.position, previousFramePosition);
        previousFramePosition = transform.position;
        distanceTravelledThisFrame = d;
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
        float velocity;
        float currentMotorTorque;
        float currentBrakeTorque;
        float currentSteeringAngle;

        //0 = no hit, 1 = hit
        List<int> hitRoadRaycast = new List<int>();


        velocity = rb.linearVelocity.magnitude;
        currentMotorTorque = motorTorque;
        currentBrakeTorque = brakeTorque;
        currentSteeringAngle = steeringAngle;

        float3 point = transform.position;

        float3 nearest;
        float t;
        SplineUtility.GetNearestPoint(roadSpline.Spline, point, out nearest, out t, 4, 2);
        centerOfRoadDist = Vector3.Distance(point, nearest);
        closestPosition = nearest;


        Quaternion downRotation = Quaternion.AngleAxis(roadRaycastDownAngle, transform.right);


        for(int i = -1; i < 2; i++)
        {
            Quaternion sideRotation = Quaternion.AngleAxis(roadRaycastSpreadAngle * (float)i, transform.up);
            Vector3 dir = (sideRotation * downRotation) * transform.forward;

            if(Physics.Raycast(transform.position + raycastOffset, dir, roadRaycastLength, roadRaycastLayer))
            {
                hitRoadRaycast.Add(1);
            }
            else
            {
                hitRoadRaycast.Add(0);
            }
        }


        inputs = new List<float>();

        inputs.Add(centerOfRoadDist);
        //inputs.Add(velocity);
        //inputs.Add(currentMotorTorque);
        //inputs.Add(currentBrakeTorque);
        //inputs.Add(currentSteeringAngle);
        foreach(int f in hitRoadRaycast)
        {
            inputs.Add((float)f);
        }
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
    }

    private void OnDrawGizmosSelected()
    {
        Quaternion downRotation = Quaternion.AngleAxis(roadRaycastDownAngle, transform.right);

        for(int i = -1; i < 2; i++)
        {
            Quaternion sideRotation = Quaternion.AngleAxis(roadRaycastSpreadAngle * (float)i, transform.up);
            Vector3 dir = (sideRotation * downRotation) * transform.forward;

            Gizmos.DrawRay(transform.position + raycastOffset, dir * roadRaycastLength);
        }
    }
}
