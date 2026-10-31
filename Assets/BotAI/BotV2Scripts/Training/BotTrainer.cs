using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class BotTrainer : MonoBehaviour
{
    [SerializeField] NeuralContainer preTrainedBrain;

    [SerializeField] GameObject botPrefab;
    [SerializeField] SplineContainer roadSpline;
    [SerializeField] TextMeshProUGUI generationCountText;

    [Header("Training")]
    [SerializeField] float spawnPosOnSpline;
    [SerializeField] bool spawnOnRandomPoint = true;
    [SerializeField] int amountToTrainPerGeneration;
    [SerializeField] float generationTrainingTime;
    [SerializeField] int inputCount = 10;
    [SerializeField] float minimumYPosition = -0.25f;
    [SerializeField] bool onlyUseBestBot;


    [Header("Brain Settings")]
    [SerializeField] Vector2 minMaxBeginningRandomValues;
    [SerializeField] float childrenRandomAdjustment;
    [SerializeField] int brainLayers;
    [SerializeField] int brainNodes;
    [SerializeField] string brainSavePath = "Assets/Saved_Brains/NewBrain.asset";
    [SerializeField] string scoreOverTimeSavePath = "Assets/ScoreOverTime.csv";

    List<BotReference> bots;

    float timer_GenerationLifetime;

    NeuralScript previousBestM;
    NeuralScript previousBestB;
    NeuralScript previousBestS;

    int generationNumber;

    List<float> highestValues;
    List<float> lowestValues;
    List<float> generationMeanScore;
    List<float> generationMedianScore;

    private void Start()
    {
        bots = new List<BotReference>();

        highestValues = new List<float>();
        lowestValues = new List<float>();
        generationMeanScore = new List<float>();
        generationMedianScore = new List<float>();

        for(int i = 0; i < amountToTrainPerGeneration; i++)
        {
            NeuralScript m = new NeuralScript();
            NeuralScript b = new NeuralScript();
            NeuralScript s = new NeuralScript();

            if(preTrainedBrain == null)
            {
                m.SetupInputLayer(inputCount);
                b.SetupInputLayer(inputCount);
                s.SetupInputLayer(inputCount);

                m.InitializeNodes(brainLayers, brainNodes);
                b.InitializeNodes(brainLayers, brainNodes);
                s.InitializeNodes(brainLayers, brainNodes);

                m.RandomiseAll(minMaxBeginningRandomValues.x, minMaxBeginningRandomValues.y);
                b.RandomiseAll(minMaxBeginningRandomValues.x, minMaxBeginningRandomValues.y);
                s.RandomiseAll(minMaxBeginningRandomValues.x, minMaxBeginningRandomValues.y);
            }
            else
            {
                preTrainedBrain.Read(out m, out b, out s);
            }


            bots.Add(new BotReference(m, b, s, 0f, null));
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
            float position = 0f;
            if (spawnOnRandomPoint) { position = Random.value; }
            else { position = spawnPosOnSpline; }

            BotControl botC = Instantiate(botPrefab, roadSpline.EvaluatePosition(position), Quaternion.identity).GetComponent<BotControl>();
            botC.roadSpline = roadSpline;

            botC.botObject = botC.gameObject;

            Vector3 tangent = roadSpline.EvaluateTangent(position);

            botC.transform.forward = tangent;

            bots[i].SetBotControl(botC);
        }
    }

    private void Update()
    {
        generationCountText.text = "#" + generationNumber.ToString();

        timer_GenerationLifetime += Time.deltaTime;

        AccumulatePoints();

        if(timer_GenerationLifetime >= generationTrainingTime)
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
            //skip accumulating if the car has fallen off
            if (bots[i].control.gameObject.activeSelf == false) { continue; }
            if (bots[i].control.gameObject.transform.position.y < minimumYPosition) { bots[i].control.gameObject.SetActive(false); continue; }


            float distFromLastFrame = bots[i].control.distanceTravelledThisFrame;

            bots[i].score += bots[i].control.facingToRoadTangentDot;
            bots[i].score += distFromLastFrame;
        }
    }

    //Destroys the bottom 50%
    private void InitializeNewGeneration()
    {
        //Removes all current instantiated prefabs
        for (int i = 0; i < bots.Count; i++)
        {
            Destroy(bots[i].control.gameObject);
        }

        //Sorts the list in descending order
        bots.Sort((p1, p2) => p2.score.CompareTo(p1.score));

        Debug.Log($"Highest: {bots[0].score}, Lowest: {bots[bots.Count-1].score}");

        highestValues.Add(bots[0].score);
        lowestValues.Add(bots[bots.Count - 1].score);

        float cumulative = 0f;
        //get mean sccore
        for(int i = 0; i < bots.Count; i++)
        {
            cumulative += bots[i].score;
        }

        generationMeanScore.Add(cumulative / bots.Count);

        float median = (bots.Count % 2 != 0) ? (bots[Mathf.FloorToInt((float)bots.Count / 2f)].score + bots[Mathf.CeilToInt((float)bots.Count / 2f)].score) / 2f : bots[bots.Count / 2].score;

        generationMedianScore.Add(median);

        //Creates two children for each of the top 50%
        List<BotReference> newGeneration = new List<BotReference>();
        
        if(!onlyUseBestBot)
        {
            for (int i = 0; i < bots.Count / 2; i++)
            {
                BotReference child1 = new BotReference(bots[i].motorControl, bots[i].brakeControl, bots[i].steeringControl, 0f, null);
                BotReference child2 = new BotReference(bots[i].motorControl, bots[i].brakeControl, bots[i].steeringControl, 0f, null);

                //Slightly randomises the brains of both children (evolution)
                child1.RandomAdjustAllControls(childrenRandomAdjustment);
                child2.RandomAdjustAllControls(childrenRandomAdjustment);

                newGeneration.Add(child1);
                newGeneration.Add(child2);
            }
        }
        else
        {
            NeuralScript m = bots[0].motorControl;
            NeuralScript b = bots[0].brakeControl;
            NeuralScript s = bots[0].steeringControl;

            for(int i = 0; i < bots.Count; i++)
            {
                BotReference child = new BotReference(m, b, s, 0f, null);

                child.RandomAdjustAllControls(childrenRandomAdjustment);

                newGeneration.Add(child);
            }
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
        
        string path;

        path = brainSavePath;

        AssetDatabase.CreateAsset(saved, path);

        EditorUtility.SetDirty(saved);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Saved new brain as {path}");
    }

    public void SaveScoreOverTime()
    {
        string path = scoreOverTimeSavePath;

        StreamWriter sr = new StreamWriter(path);

        sr.WriteLine("Highest Score,Lowest Score,Mean Score,Median Score");

        for(int i = 0; i < highestValues.Count; i++)
        {
            sr.WriteLine($"{highestValues[i].ToString()},{lowestValues[i].ToString()},{generationMeanScore[i].ToString()},{generationMedianScore[i].ToString()}");
        }

        sr.Flush();
        sr.Close();

        Debug.Log("Graph values saved successfully!");
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

    public void SetBotControl(BotControl control)
    {
        this.control = control;

        control.motorControl = motorControl;
        control.brakeControl = brakeControl;
        control.steeringControl = steeringControl;
    }

    public void RandomAdjustAllControls(float adjustAmount)
    {
        motorControl.AdjustRandom(adjustAmount);
        brakeControl.AdjustRandom(adjustAmount);
        steeringControl.AdjustRandom(adjustAmount);
    }
}
