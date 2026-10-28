using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NeuralContainer", menuName = "Scriptable Objects/NeuralContainer")]
public class NeuralContainer : ScriptableObject
{
    //break between value, bias and weight
    [SerializeField] char nodeValueBreakKey = '/';
    //break between individual nodes
    [SerializeField] char nodeSplitKey = ',';
    //identifies generic layers
    [SerializeField] char layerIdentifier = 'L';
    //identifies input layer
    [SerializeField] char inputLayerIndentifier = 'I';
    //identifies output bias
    [SerializeField] char outputBiasIdentifier = 'O';
    //Stopper
    [SerializeField] char scriptStopper = ';';

    //identifies each script
    [SerializeField] char motorScriptIdentifier = 'M';
    [SerializeField] char brakeScriptIdentifier = 'B';
    [SerializeField] char steerScriptIdentifier = 'S';

    //Weight-Bias-Value

    //Save entire thing as a string
    [SerializeField, TextArea(2, 20)] private string saved;

    //Write the neural scripts to this object
    public void Write(NeuralScript motor, NeuralScript brake, NeuralScript steer)
    {
        string m = WriteIndividual(motor, motorScriptIdentifier);
        string b = WriteIndividual(brake, brakeScriptIdentifier);
        string s = WriteIndividual(steer, steerScriptIdentifier);

        saved = motorScriptIdentifier + m + brakeScriptIdentifier + b + steerScriptIdentifier + s;
    }

    public void Read(out NeuralScript motor, out NeuralScript brake, out NeuralScript steer)
    {
        motor = ReadIndividual(motorScriptIdentifier);
        brake = ReadIndividual(brakeScriptIdentifier);
        steer = ReadIndividual(steerScriptIdentifier);
    }

    public string ReadRaw()
    {
        return saved;
    }

    private NeuralScript ReadIndividual(char scriptIdentifier)
    {
        NeuralScript output = new NeuralScript();

        int scanPlace = 0;

        //1 = input layer, 2 = generic layer, 3 = output bias
        int scanType = 1;

        //Find first Identifier
        for(int i = 0; i < saved.Length; i++)
        {
            char c = saved[i];

            if(c == scriptIdentifier)
            {
                scanPlace = i + 1;
                break;
            }
        }

        //1 = weight, 2 = bias, 3 = value
        int nodeValueType = 1;
        NeuralNode currentNode = new NeuralNode(0f, 0f, 0f);
        NeuralLayer current = new NeuralLayer(new List<NeuralNode>(0));
        NeuralLayer inputLayer = new NeuralLayer(new List<NeuralNode>(0));
        List<NeuralLayer> savedLayers = new List<NeuralLayer>();

        //Adds to this when not on any key (i.e. it is on a number)
        string numCumulative = "";

        while (saved[scanPlace] != scriptStopper)
        {
            if (saved[scanPlace] == inputLayerIndentifier) { scanType = 1; scanPlace++; }
            if (saved[scanPlace] == layerIdentifier)
            {
                if(scanType == 1)
                {
                    inputLayer = current;

                    current = new NeuralLayer(new List<NeuralNode>(0));

                    scanType = 2;
                }
                else if(scanType == 2)
                {
                    savedLayers.Add(current);

                    current = new NeuralLayer(new List<NeuralNode>(0));
                }

                scanPlace++;
            }
            else if (saved[scanPlace] == outputBiasIdentifier)
            {
                savedLayers.Add(current);
                current = null;

                scanType = 3;

                scanPlace++;
            }

            if(scanType == 1)
            {
                if (saved[scanPlace] == nodeSplitKey)
                {
                    current.nodes.Add(currentNode);

                    currentNode = new NeuralNode(0f, 0f, 0f);
                }
                else if (saved[scanPlace] == nodeValueBreakKey)
                {
                    currentNode.weight = float.Parse(numCumulative);
                }
                else
                {
                    numCumulative += saved[scanPlace];
                }
            }
            else if(scanType == 2)
            {
                if (saved[scanPlace] == nodeSplitKey)
                {
                    current.nodes.Add(currentNode);

                    currentNode = new NeuralNode(0f, 0f, 0f);
                    nodeValueType = 1;
                }
                else if (saved[scanPlace] == nodeValueBreakKey)
                {
                    if (nodeValueType == 1)
                    {
                        currentNode.weight = float.Parse(numCumulative);
                    }
                    else if (nodeValueType == 2)
                    {
                        currentNode.bias = float.Parse(numCumulative);
                    }
                    else
                    {
                        currentNode.value = float.Parse(numCumulative);
                    }

                    numCumulative = "";
                }
                else
                {
                    numCumulative += saved[scanPlace];
                }

            }
            else if(scanType == 3)
            {
                if (saved[scanPlace] == nodeValueBreakKey)
                {
                    output.SetOutputBias(float.Parse(numCumulative));
                }
                else
                {
                    numCumulative += saved[scanPlace];
                }
            }

            scanPlace++;
        }

        output.SetInputLayer(inputLayer);
        output.SetLayers(savedLayers);

        return output;
    }

    private string WriteIndividual(NeuralScript neural, char identifier)
    {
        string output = "";

        List<NeuralLayer> layers = new List<NeuralLayer>();
        float outputBias = 0f;
        NeuralLayer inputLayer = null;

        layers = neural.GetNeuralValues(out inputLayer, out outputBias);

        //Start with input values
        output += inputLayerIndentifier;

        //inputs technically have a bias in the code but they are not used so it can be ignored
        //input values are set by the car so that can also be ignored
        for(int i = 0; i < inputLayer.nodes.Count; i++)
        {
            //Get weight of each node 
            output += inputLayer.nodes[i].weight.ToString();

            output += nodeValueBreakKey;
            output += nodeSplitKey;
        }

        //Do the generic layers
        for(int i = 0; i < layers.Count; i++)
        {
            output += layerIdentifier;

            for(int j = 0; j < layers[i].nodes.Count; j++)
            {
                output += layers[i].nodes[j].weight.ToString();
                output += nodeValueBreakKey;
                output += layers[i].nodes[j].bias.ToString();
                output += nodeValueBreakKey;
                output += layers[i].nodes[j].value.ToString();
                output += nodeValueBreakKey;

                output += nodeSplitKey;
            }
        }

        output += outputBiasIdentifier;
        output += outputBias.ToString();
        output += nodeValueBreakKey;

        output += scriptStopper;

        return output;
    }
}
