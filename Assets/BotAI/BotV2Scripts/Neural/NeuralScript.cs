using System.Collections.Generic;
using UnityEngine;

//Holds the generated code for the bot
[System.Serializable]
public class NeuralScript
{
    float outputBias;
    NeuralLayer inputLayer;
    List<NeuralLayer> layers;

    public NeuralScript ReturnCopy()
    {
        NeuralScript newScript = new NeuralScript();

        //Creates a copy of the input layer
        List<NeuralNode> newInputNodes = new List<NeuralNode>();

        for(int i = 0; i < inputLayer.nodes.Count; i++)
        {
            //value and bias do not matter here
            NeuralNode newNode = new NeuralNode(0f, inputLayer.nodes[i].weight, 0f);

            newInputNodes.Add(newNode);
        }

        NeuralLayer newInputLayer = new NeuralLayer(newInputNodes);

        //Creates a copy of the layers
        List<NeuralLayer> newLayers = new List<NeuralLayer>();

        for(int i = 0; i < layers.Count; i++)
        {
            List<NeuralNode> newNodes = new List<NeuralNode>();

            for(int j = 0; j < layers[i].nodes.Count; j++)
            {
                newNodes.Add(new NeuralNode(
                    layers[i].nodes[j].value,
                    layers[i].nodes[j].weight,
                    layers[i].nodes[j].bias
                ));
            }

            newLayers.Add(new NeuralLayer(newNodes));
        }

        newScript.SetOutputBias(outputBias);
        newScript.SetInputLayer(newInputLayer);
        newScript.SetLayers(newLayers);

        return newScript;
    }

    public List<NeuralLayer> GetNeuralValues(out NeuralLayer inputLayer, out float outputBias)
    {
        inputLayer = this.inputLayer;
        outputBias = this.outputBias;

        return layers;
    }

    public void SetInputLayer(NeuralLayer inputLayer)
    {
        this.inputLayer = inputLayer;
    }

    public void SetLayers(List<NeuralLayer> layers)
    {
        this.layers = layers;
    }

    public void SetOutputBias(float bias)
    {
        outputBias = bias;
    }

    public NeuralLayer GetInputLayer()
    {
        return inputLayer;
    }

    public List<NeuralLayer> GetBrainLayers()
    {
        return layers;
    }

    public float GetOutputBias()
    {
        return outputBias;
    }

    public void SetupInputLayer(int numInputs)
    {
        List<NeuralNode> inputNodes = new List<NeuralNode>();

        for(int i = 0; i < numInputs; i++)
        {
            inputNodes.Add(new NeuralNode(1f, 1f, 1f));
        }

        inputLayer = new NeuralLayer(inputNodes);
    }
    
    //Creates a new network with x nodes per y layers
    public void InitializeNodes(int layerDepth, int nodeDepth)
    {
        layers = new List<NeuralLayer>();

        for(int i = 0; i < layerDepth; i++)
        {
            List<NeuralNode> nodes = new List<NeuralNode>();

            for(int j = 0; j < nodeDepth; j++)
            {
                NeuralNode newNode = new NeuralNode(1f, 1f, 1f);
                
                nodes.Add(newNode);
            }

            NeuralLayer newLayer = new NeuralLayer(nodes);

            layers.Add(newLayer);
        }
    }

    public float Calculate(List<float> inputs)
    {
        //this bias + Sum(previous weight * previous value)

        inputLayer.SetNodeValues(inputs);

        for(int i = 0; i < layers.Count + 1; i++)
        {
            List<float> pWeights = (i>0)? layers[i-1].GetWeights() : inputLayer.GetWeights();
            List<float> pValues = (i>0)? layers[i-1].GetValues() : inputLayer.GetValues();

            float previousLayerWeightedValue = 0f;

            for(int j = 0; j < pValues.Count; j++)
            {
                previousLayerWeightedValue += pWeights[j] * pValues[j];
            }

            //check if it is the last layer
            if(i < layers.Count)
            {
                foreach (NeuralNode node in layers[i].nodes)
                {
                    node.value = node.bias + previousLayerWeightedValue;
                }
            }
            else //return one output
            {
                return previousLayerWeightedValue + outputBias;
            }
        }

        return 0f;
    }

    //adds a slight random value
    public void AdjustRandom(float r)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)
            {
                float randV = Random.Range(-r, r);
                float randW = Random.Range(-r, r);
                float randB = Random.Range(-r, r);

                layers[i].nodes[j].value += randV;
                layers[i].nodes[j].weight += randW;
                layers[i].nodes[j].bias += randB;
            }
        }
    }

    //Randomises all values, weights and biases
    public void RandomiseAll(float min, float max)
    {
        for(int i = 0; i < layers.Count; i++)
        {
            for(int j = 0; j < layers[i].nodes.Count; j++)
            {
                float randV = Random.Range(min, max);
                float randW = Random.Range(min, max);
                float randB = Random.Range(min, max);

                layers[i].nodes[j].value = randV;
                layers[i].nodes[j].weight = randW;
                layers[i].nodes[j].bias = randB;
            }
        }
    }

    public int GetLayerCount()
    {
        return layers.Count;
    }

    public int GetFirstLayerNodeCount()
    {
        return layers[0].nodes.Count;
    }

    public int GetInputNodeCount()
    {
        return inputLayer.nodes.Count;
    }

    public void ResetInputs()
    {
        foreach(NeuralNode n in inputLayer.nodes)
        {
            n.value = 0f;
        }
    }
}

[System.Serializable]
public class NeuralNode
{
    public float value;
    public float weight;
    public float bias;

    public NeuralNode(float value, float weight, float bias)
    {
        this.value = value;
        this.weight = weight;
        this.bias = bias;
    }
}

[System.Serializable]
public class NeuralLayer
{
    public List<NeuralNode> nodes;

    public NeuralLayer(List<NeuralNode> nodes)
    {
        this.nodes = nodes;
    }

    public List<float> GetValues()
    {
        List<float> values = new List<float>();

        foreach(NeuralNode node in nodes)
        {
            values.Add(node.value);
        }

        return values;
    }

    public List<float> GetWeights()
    {
        List<float> weights = new List<float>();

        foreach(NeuralNode node in nodes)
        {
            weights.Add(node.weight);
        }

        return weights;
    }

    public List<float> GetBiases()
    {
        List<float> biases = new List<float>();

        foreach(NeuralNode node in nodes)
        {
            biases.Add(node.bias);
        }

        return biases;
    }

    public void SetNodeValues(List<float> newValues)
    {
        if(newValues.Count != nodes.Count)
        {
            Debug.LogWarning($"Requested to change node values failed, Input {newValues.Count} is not equal to {nodes.Count}!");
            return;
        }

        for(int i = 0; i < newValues.Count; i++)
        {
            nodes[i].value = newValues[i];
        }
    }
}
