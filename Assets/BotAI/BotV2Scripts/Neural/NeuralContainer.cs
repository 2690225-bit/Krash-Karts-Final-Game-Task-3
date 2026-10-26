using UnityEngine;

[CreateAssetMenu(fileName = "NeuralContainer", menuName = "Scriptable Objects/NeuralContainer")]
public class NeuralContainer : ScriptableObject
{
    public NeuralScript MotorTorqueNetwork;
    public NeuralScript BrakeTorqueNetwork;
    public NeuralScript SteeringAngleNetwork;
}
