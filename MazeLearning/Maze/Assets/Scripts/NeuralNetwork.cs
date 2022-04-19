using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private int[] layers;
 
    // Neuron matrix
    private float[][] neurons;
 
    // Weight matrices
    private float[][][] weights;

    // Network fitness
    public float Fitness { get; set; }

    public NeuralNetwork(int[] layersP)
    {
        layers = new int[layersP.Length];
        for (int i = 0; i < layersP.Length; i++)
        {
            layers[i] = layersP[i];
        }
        InitNeurons();
        InitWeights();
    }
    
    public NeuralNetwork(NeuralNetwork copy)
    {
        layers = new int[copy.layers.Length];
        for (int i = 0; i < copy.layers.Length; i++)
        {
            layers[i] = copy.layers[i];
        }
        InitNeurons();
        InitWeights();
        CopyWeights(copy.weights);
    }

    private void InitNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);
        }
        neurons = neuronsList.ToArray();
    }
    
    private void InitWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();
        
        // For all neurons with inputs (note i starts at 1)
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>();
            int nbNeuronsInPreviousLayer = layers[i - 1];
            
            // For all neurons in this layer
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronsWeights = new float[nbNeuronsInPreviousLayer];
                // Init weights randomly between previous and next layer
                for (int k = 0; k < nbNeuronsInPreviousLayer; k++)
                {
                    neuronsWeights[k] = Random.Range(-1f, 1f);
                }
                layerWeightsList.Add(neuronsWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        weights = weightsList.ToArray();
    }
    
    private void CopyWeights(float[][][] copyWeights)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = copyWeights[i][j][k];
                }
            }
        }
    }

    public float[] FeedForward(float[] inputs)
    {
        // Add inputs in input neurons' matrix
        for (int i = 1; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        
        // For each layer and each neuron in the layer
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                // Compute the value for this neuron, then apply activation function
                float val = 0f;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    val += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                // Activation function is hyperbolic tangent
                neurons[i][j] = (float)Math.Tanh(val);
            }
        }

        // Return output layer
        return neurons[neurons.Length - 1];
    }

    public void Mutate(float condition)
    {
        // Slightly mutate all weights if a value
        // between 0 and 100 is inferior to condition
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    float weight = weights[i][j][k];
                    float randNum = UnityEngine.Random.Range(0f, 100f);
                    if (randNum <= condition)
                    {
                        weight = UnityEngine.Random.Range(-1f, 1f);
                    }

                    weights[i][j][k] = weight;
                }
            }
        }
    }

    public int CompareTo(NeuralNetwork other)
    {
        if (other == null) return 1;

        if (Fitness > other.Fitness) return 1;
        if (Fitness < other.Fitness) return -1;
        return 0;
    }

    public void AddFitness(float fitnessP)
    {
        Fitness += fitnessP;
    }
}
