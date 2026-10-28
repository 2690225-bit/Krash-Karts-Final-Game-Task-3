using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class BotTrainer : MonoBehaviour
{
    [SerializeField] GameObject botPrefab;
    [SerializeField] SplineContainer roadSpline;
    [SerializeField] TextMeshProUGUI generationCountText;

    [Header("Training")]
    [SerializeField] int amountToTrainPerGeneration;
    [SerializeField] float generationTrainingTime;
    [SerializeField] int inputCount = 10;
    [SerializeField] float minimumYPosition = -0.25f;
    [SerializeField] float pointsForBeingAboveExpectedVelocity;
    [SerializeField] float penaltyForBeingBelowExpectedVelocity;
    [SerializeField] float expectedVelocity = 10f;
    [SerializeField, Range(-1f, 1f)] float forwardFacingScoreRigidity = 0.5f;
    [SerializeField] float forwardFacingMultiplier;
    [SerializeField] float speedMultiplier;


    [Header("Brain Settings")]
    [SerializeField] Vector2 minMaxBeginningRandomValues;
    [SerializeField] float childrenRandomAdjustment;
    [SerializeField] int brainLayers;
    [SerializeField] int brainNodes;
    [SerializeField] string brainSavePath = "Assets/Saved_Brains/NewBrain.asset";

    List<BotReference> bots;

    List<float> cumulativeDistanceTravelled;
    List<float> cumulativeFacingDirection;

    float timer_GenerationLifetime;

    NeuralScript previousBestM;
    NeuralScript previousBestB;
    NeuralScript previousBestS;

    int generationNumber;

    float fallOffPenalty;

    int botsDeactivated = 0;

    private void Start()
    {
        bots = new List<BotReference>();
        cumulativeDistanceTravelled = new List<float>();
        cumulativeFacingDirection = new List<float>();

        for(int i = 0; i < amountToTrainPerGeneration; i++)
        {
            NeuralScript m = new NeuralScript();
            NeuralScript b = new NeuralScript();
            NeuralScript s = new NeuralScript();

            m.SetupInputLayer(inputCount);
            b.SetupInputLayer(inputCount);
            s.SetupInputLayer(inputCount);

            m.InitializeNodes(brainLayers, brainNodes);
            b.InitializeNodes(brainLayers, brainNodes);
            s.InitializeNodes(brainLayers, brainNodes);

            m.RandomiseAll(minMaxBeginningRandomValues.x, minMaxBeginningRandomValues.y);
            b.RandomiseAll(minMaxBeginningRandomValues.x, minMaxBeginningRandomValues.y);
            s.RandomiseAll(minMaxBeginningRandomValues.x, minMaxBeginningRandomValues.y);

            bots.Add(new BotReference(m, b, s, 0f, null));

            cumulativeDistanceTravelled.Add(0f);
            cumulativeFacingDirection.Add(0f);
        }

        previousBestM = bots[0].motorControl;
        previousBestB = bots[0].brakeControl;
        previousBestS = bots[0].steeringControl;

        RunGeneration();
    }

    //Final step to starting a generation, gives all bots a prefab to control
    private void RunGeneration()
    {
        generationNumber++;

        for(int i = 0; i < bots.Count; i++)
        {
            float position = Random.value;

            BotControl bot = Instantiate(botPrefab, roadSpline.EvaluatePosition(position), Quaternion.identity).GetComponent<BotControl>();
            bot.roadSpline = roadSpline;

            bot.botObject = bot.gameObject;

            Vector3 tangent = roadSpline.EvaluateTangent(position);

            bot.transform.forward = tangent;

            bot.motorControl = bots[i].motorControl;
            bot.brakeControl = bots[i].brakeControl;
            bot.steeringControl = bots[i].steeringControl;

            bots[i].control = bot;
        }
    }

    private void Update()
    {
        generationCountText.text = "#" + generationNumber.ToString();

        timer_GenerationLifetime += Time.deltaTime;

        AccumulatePoints();

        if(timer_GenerationLifetime > generationTrainingTime)
        {
            timer_GenerationLifetime = 0f;

            InitializeNewGeneration();
        }
    }

    private void AccumulatePoints()
    {
        //score will be given by the distance travelled and the time spent facing the road forward

        for(int i = 0; i < bots.Count; i++)
        {
            //Get dot between bot forward and road forward

            if (bots[i].control.botObject.transform.position.y < minimumYPosition)
            {
                bots[i].control.botObject.gameObject.SetActive(false);
                continue;

                botsDeactivated++;

                if(botsDeactivated >= bots.Count)
                {
                    timer_GenerationLifetime = generationTrainingTime;
                }
            }


            if (bots[i].control.GetVelocity() >= expectedVelocity)
            {
                bots[i].score += pointsForBeingAboveExpectedVelocity * Time.deltaTime;
            }
            else
            {
                bots[i].score -= penaltyForBeingBelowExpectedVelocity * Time.deltaTime;
            }

            float facing = bots[i].control.facingToRoadTangentDot * Time.deltaTime;

            bots[i].score += bots[i].control.GetVelocity() * speedMultiplier * Time.deltaTime;
            bots[i].score += Mathf.Clamp(facing + forwardFacingScoreRigidity, -1f, 1f) * forwardFacingMultiplier * Time.deltaTime;
        }
    }

    //Destroys the bottom 50%
    private void InitializeNewGeneration()
    {
        //Check and sort points
        botsDeactivated = 0;

        //Sorts the list in descending order
        bots.Sort((p1, p2) => p2.score.CompareTo(p1.score));

        //Creates two children for each of the top 50%
        List<BotReference> newGeneration = new List<BotReference>();

        for(int i = 0; i < bots.Count / 2; i++)
        {
            BotReference child1 = new BotReference(bots[i].motorControl, bots[i].brakeControl, bots[i].steeringControl, 0f, null);
            BotReference child2 = new BotReference(bots[i].motorControl, bots[i].brakeControl, bots[i].steeringControl, 0f, null);

            //Slightly randomises the brains of both children (evolution)
            child1.RandomAdjustAllControls(childrenRandomAdjustment);
            child2.RandomAdjustAllControls(childrenRandomAdjustment);

            newGeneration.Add(child1);
            newGeneration.Add(child2);
        }

        //Removes all current instantiated prefabs
        for(int i = 0; i < bots.Count; i++)
        {
            Destroy(bots[i].control.gameObject);
        }

        //Saves the highest scoring brain from the old generation
        previousBestM = bots[0].motorControl;
        previousBestB = bots[0].brakeControl;
        previousBestS = bots[0].steeringControl;

        bots.Clear();
        bots = newGeneration;

        RunGeneration();
    }

    //Saves the best stored brain as a new scriptable object
    public void SaveBestBrain()
    {
        NeuralContainer saved = ScriptableObject.CreateInstance<NeuralContainer>();

        saved.Write(previousBestM, previousBestB, previousBestS);

        string path = brainSavePath;

        AssetDatabase.CreateAsset(saved, path);

        EditorUtility.SetDirty(saved);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Saved new brain as {path}");
    }


}

//class to hold the brain, controller and score for each bot
public class BotReference
{
    public NeuralScript motorControl;
    public NeuralScript brakeControl;
    public NeuralScript steeringControl;
    public BotControl control;
    public float score;

    public BotReference(NeuralScript motor, NeuralScript brake, NeuralScript steer, float score, BotControl control)
    {
        motorControl = motor;
        brakeControl = brake;
        steeringControl = steer;
        this.control = control;
        this.score = score;
    }

    public void RandomAdjustAllControls(float adjustAmount)
    {
        motorControl.AdjustRandom(adjustAmount);
        brakeControl.AdjustRandom(adjustAmount);
        steeringControl.AdjustRandom(adjustAmount);
    }
}
