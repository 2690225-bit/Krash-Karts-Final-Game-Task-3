using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class BotTrainer : MonoBehaviour
{
    [SerializeField] GameObject botPrefab;
    [SerializeField] SplineContainer roadSpline;

    [Header("Training")]
    [SerializeField] int amountToTrainPerGeneration;
    [SerializeField] float generationTrainingTime;

    [Header("Brain Settings")]
    [SerializeField] Vector2 minMaxBeginningRandomValues;
    [SerializeField] float childrenRandomAdjustment;
    [SerializeField] int brainLayers;
    [SerializeField] int brainNodes;

    List<NeuralScript> botBrainsMotor;
    List<NeuralScript> botBrainsBrake;
    List<NeuralScript> botBrainsSteering;
    List<BotControl> botCars;
    List<float> botScores;

    float timer_GenerationLifetime;

    private void Start()
    {
        botBrainsMotor = new List<NeuralScript>();
        botBrainsBrake = new List<NeuralScript>();
        botBrainsSteering = new List<NeuralScript>();

        for(int i = 0; i < amountToTrainPerGeneration; i++)
        {
            NeuralScript m = new NeuralScript();
            NeuralScript b = new NeuralScript();
            NeuralScript s = new NeuralScript();

            m.InitializeNodes(brainLayers, brainNodes);
            b.InitializeNodes(brainLayers, brainNodes);
            s.InitializeNodes(brainLayers, brainNodes);

            botBrainsMotor.Add(m);
            botBrainsBrake.Add(b);
            botBrainsSteering.Add(s);
        }

        foreach(var brain in botBrainsMotor)
        {
            brain.RandomiseAll(minMaxBeginningRandomValues.x, minMaxBeginningRandomValues.y);
        }
        foreach(var brain in botBrainsBrake)
        {
            brain.RandomiseAll(minMaxBeginningRandomValues.x, minMaxBeginningRandomValues.y);
        }
        foreach(var brain in botBrainsSteering)
        {
            brain.RandomiseAll(minMaxBeginningRandomValues.x, minMaxBeginningRandomValues.y);
        }

        StartFirstGeneration();
    }

    private void StartFirstGeneration()
    {
        for(int i = 0; i < amountToTrainPerGeneration; i++)
        {
            float position = Random.value;

            BotControl bot = Instantiate(botPrefab, roadSpline.EvaluatePosition(position), Quaternion.identity).GetComponent<BotControl>();
            bot.roadSpline = roadSpline;

            bot.botObject = bot.gameObject;

            Vector3 tangent = roadSpline.EvaluateTangent(position);

            bot.transform.forward = tangent;

            bot.motorControl = botBrainsMotor[i];
            bot.brakeControl = botBrainsBrake[i];
            bot.steeringControl = botBrainsSteering[i];
        }
    }

    private void Update()
    {
        timer_GenerationLifetime += Time.deltaTime;
    }

    //Destroys the bottom 50%
    private void InitializeNewGeneration(bool destroyHalf)
    {
        
    }

    private void StartGeneration()
    {
        for(int i = 0; i < amountToTrainPerGeneration; i++)
        {
            float t = Random.value;

        }
    }

    private void RunGeneration()
    {

    }
}
