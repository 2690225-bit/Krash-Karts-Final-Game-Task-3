using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class BotTrainer : MonoBehaviour
{
    InputSystem_Actions actions;

    [SerializeField] NeuralContainer preTrainedBrain;

    [SerializeField] GameObject botPrefab;
    [SerializeField] SplineContainer roadSpline;
    [SerializeField] TextMeshProUGUI generationCountText;
    [SerializeField] Material selectedMaterial;
    [SerializeField] Camera mainCam;
    [SerializeField] LayerMask carLayer;

    [Header("Training")]
    [SerializeField] float spawnPosOnSpline;
    [SerializeField] bool spawnOnRandomPoint = true;
    [SerializeField] int amountToTrainPerGeneration;
    [SerializeField] int inputCount = 10;
    [SerializeField] float minimumYPosition = -0.25f;


    [Header("Brain Settings")]
    [SerializeField] Vector2 minMaxBeginningRandomValues;
    [SerializeField] float childrenRandomAdjustment;
    [SerializeField] int brainLayers;
    [SerializeField] int brainNodes;
    [SerializeField] string brainSavePath = "Assets/Saved_Brains/NewBrain.asset";
    [SerializeField] string scoreOverTimeSavePath = "Assets/ScoreOverTime.csv";

    List<BotReference> bots;

    NeuralScript previousBestM;
    NeuralScript previousBestB;
    NeuralScript previousBestS;

    int generationNumber;

    List<float> highestDistance;
    List<float> lowestDistance;
    List<float> generationMeanDistance;
    List<float> generationMedianDistance;

    List<BotReference> selectedBest = new List<BotReference>();

    BotReference bestFromLastGen;

    private void Awake()
    {
        actions = new InputSystem_Actions();
    }
    private void OnEnable() { actions.Enable(); } 
    private void OnDisable() { actions.Disable(); }

    private void Start()
    {
        actions.Player.Select.performed += SelectBot;

        bots = new List<BotReference>();

        selectedBest = new List<BotReference>();

        highestDistance = new List<float>();
        lowestDistance = new List<float>();
        generationMeanDistance = new List<float>();
        generationMedianDistance = new List<float>();

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

        AccumulateDistanceTravelled();
    }

    public void StartNextGen()
    {
        InitializeNewGeneration();
    }

    private void AccumulateDistanceTravelled()
    {
        //score will be given by the distance travelled and the time spent facing the road forward

        for(int i = 0; i < bots.Count; i++)
        {
            //skip accumulating if the car has fallen off
            if (bots[i].control.gameObject.activeSelf == false) { continue; }
            if (bots[i].control.gameObject.transform.position.y < minimumYPosition) { bots[i].control.StopBot(); continue; }


            float distFromLastFrame = bots[i].control.distanceTravelledThisFrame;
            bots[i].score += distFromLastFrame;
        }
    }

    private void InitializeNewGeneration()
    {
        if(selectedBest.Count == 0) { return; }

        //Removes all current instantiated prefabs
        for (int i = 0; i < bots.Count; i++)
        {
            Destroy(bots[i].control.gameObject);
            bots[i].control = null;
        }

        //CODE FOR PERFORMANCE ANALYSIS
        //Sorts the list in descending order
        
        bots.Sort((p1, p2) => p2.score.CompareTo(p1.score));
        Debug.Log($"Highest: {bots[0].score}, Lowest: {bots[bots.Count-1].score}");
        highestDistance.Add(bots[0].score);
        lowestDistance.Add(bots[bots.Count - 1].score);
        float cumulative = 0f;
        //get mean sccore
        for(int i = 0; i < bots.Count; i++)
        {
            cumulative += bots[i].score;
        }
        generationMeanDistance.Add(cumulative / bots.Count);
        float median = (bots.Count % 2 != 0) ? (bots[Mathf.FloorToInt((float)bots.Count / 2f)].score + bots[Mathf.CeilToInt((float)bots.Count / 2f)].score) / 2f : bots[bots.Count / 2].score;
        generationMedianDistance.Add(median);
        //===================================
        

        List<BotReference> newGeneration = new List<BotReference>();
    
        int count = amountToTrainPerGeneration / selectedBest.Count;

        for(int i = 0; i < count; i++)
        {   
            int j = Mathf.FloorToInt(Mathf.Repeat(i, selectedBest.Count));

            NeuralScript m = selectedBest[j].motorControl.ReturnCopy();
            NeuralScript b = selectedBest[j].brakeControl.ReturnCopy();
            NeuralScript s = selectedBest[j].steeringControl.ReturnCopy();

            m.ResetInputs();
            b.ResetInputs();
            s.ResetInputs();

            m.AdjustRandom(childrenRandomAdjustment);
            b.AdjustRandom(childrenRandomAdjustment);
            s.AdjustRandom(childrenRandomAdjustment);

            BotReference newBot = new BotReference(m, b, s, 0f, null);

            newGeneration.Add(newBot);
        }

        bestFromLastGen = selectedBest[0];

        //Saves the highest scoring brain from the old generation
        previousBestM = bestFromLastGen.motorControl;
        previousBestB = bestFromLastGen.brakeControl;
        previousBestS = bestFromLastGen.steeringControl;

        bots.Clear();
        bots = newGeneration;

        selectedBest.Clear();

        RunGeneration();
    }

    private void SelectBot(InputAction.CallbackContext ctx)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(actions.Player.CursorPos.ReadValue<Vector2>());

        if(Physics.Raycast(ray, out hit, float.MaxValue, carLayer))
        {
            if(hit.collider.gameObject.transform.root.TryGetComponent<BotControl>(out BotControl b))
            {
                int index = FindBotReferenceIndex(b);

                if(index == -1) { return; }

                //deselect bot
                if(selectedBest.Contains(bots[index]))
                {
                    selectedBest.Remove(bots[index]);
                    bots[index].control.DeselectTraining();
                }
                else
                {
                    selectedBest.Add(bots[index]);
                    bots[index].control.SelectTraining(selectedMaterial);
                }
            }
        }
    }

    // returns 0 if it didn't find anything
    private int FindBotReferenceIndex(BotControl b)
    {
        for(int i = 0; i < bots.Count; i++)
        {
            if(bots[i].control == b) { return i; }
        }

        return -1;
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

        for(int i = 0; i < highestDistance.Count; i++)
        {
            sr.WriteLine($"{highestDistance[i].ToString()},{lowestDistance[i].ToString()},{generationMeanDistance[i].ToString()},{generationMedianDistance[i].ToString()}");
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

        control.SetControls(motorControl, brakeControl, steeringControl);
    }

    public void RandomAdjustAllControls(float adjustAmount)
    {
        motorControl.AdjustRandom(adjustAmount);
        brakeControl.AdjustRandom(adjustAmount);
        steeringControl.AdjustRandom(adjustAmount);
    }
}
