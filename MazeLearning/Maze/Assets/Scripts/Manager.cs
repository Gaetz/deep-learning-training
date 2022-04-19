using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class Manager : MonoBehaviour
{
    [SerializeField] private GameObject agentPrefab;

    [SerializeField] private Vector3 startPosition = new Vector3(20f, 0, -20);
    [SerializeField] private int populationSize = 40;
    [SerializeField] private float timeLimit = 6f;
    private int generationCount = 0;
    private bool isTraining = false;

    private List<NeuralNetwork> currentGeneration;
    private List<NeuralNetwork> nextGeneration = new List<NeuralNetwork>();
    private List<GameObject> agents = null;

    // 6 inputs, 2 outputs, 2 layers
    private int[] layers = new[] { 6, 8, 10, 6, 2 };
    
    private float generationFitnessMean = 0f;


    private void CloseTimer()
    {
        generationCount++;
        isTraining = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTraining)
        {
            // When training is about to start
            if (generationCount == 0)
            {
                InitAgentNetworks();
                CreateAgents();
                Invoke("CloseTimer", timeLimit);
                isTraining = true;
                Debug.Log($"Generation {generationCount}");

            }
            // When training is finished
            else
            {
                Debug.Log($"Generation {generationCount}");
                generationFitnessMean = 0;
                for (int i = 0; i < populationSize; i++)
                {
                    Controller controller = agents[i].GetComponent<Controller>();
                    float fitness = controller.fitness;
                    currentGeneration[i].Fitness = fitness;
                    generationFitnessMean += currentGeneration[i].Fitness;
                }
                
                // Compute generation mean
                generationFitnessMean /= populationSize;
                Debug.Log($"Generation fitness:{generationFitnessMean}");
                
                // Sort from best to worst fitness
                currentGeneration.Sort();
                currentGeneration.Reverse();
                
                // Next generation: duplicates the best quarter of current generation
                nextGeneration.Clear();
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < populationSize / 5; j++)
                    {
                        var network = new NeuralNetwork(currentGeneration[j]);
                        // More mutations for second, third and fourth group
                        switch (i)
                        {
                            case 1:
                            case 2:
                                network.Mutate(0.5f);
                                break;
                            case 3:
                                network.Mutate(2f);
                                break;
                            case 4:
                                network.Mutate(9f);
                                break;
                        }
                        nextGeneration.Add(network);
                    }
                }

                // Start new generation after timer
                currentGeneration = nextGeneration.ConvertAll(network => new NeuralNetwork(network));
                Invoke("CloseTimer", timeLimit);
                CreateAgents();
                isTraining = true;
            }
        }
        
        // Feedforward
        // Transfer info from the controller to the associated neural network
        for (int i = 0; i < populationSize; i++)
        {
            Controller controller = agents[i].GetComponent<Controller>();
            float vel = controller.currentVelocity / controller.maxViewDistance;
            float distForward = controller.distForward / controller.maxViewDistance;
            float distLeft = controller.distLeft / controller.maxViewDistance;
            float distDiagLeft = controller.distDiagLeft / controller.maxViewDistance;
            float distRight = controller.distRight / controller.maxViewDistance;
            float distDiagRight = controller.distDiagRight / controller.maxViewDistance;

            float[] inputs = { vel, distForward, distLeft, distDiagLeft, distRight, distDiagRight};
            var outputs = currentGeneration[i].FeedForward(inputs);
            controller.outputs = outputs;
        }
        // If you need to go quickly to the next gen
        if (Input.GetKeyDown("space"))
        {
            CloseTimer();
            CreateAgents();
        }
    }

    private void InitAgentNetworks()
    {
        currentGeneration = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Mutate(0.5f);
            currentGeneration.Add(net);
        }
    }

    private void CreateAgents()
    {
        if (agents != null)
        {
            // Destroy previous agents
            for (int i = agents.Count - 1; i >= 0; i--)
            {
                Destroy(agents[i]);
            }
            agents.Clear();
        }

        // New generation
        agents = new List<GameObject>();
        for (int i = 0; i < populationSize; i++)
        {
            GameObject agent = Instantiate(agentPrefab, startPosition, Quaternion.identity);
            agents.Add(agent);
        }
    }
}
