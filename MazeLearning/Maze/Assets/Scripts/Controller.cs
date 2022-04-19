using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    private CharacterController unityController;
    private Vector3 agentPosition;

    // Outputs
    private Vector3 moveDir = Vector3.zero;
    public float currentVelocity = 0f;

    // Input
    public float distForward = 0f;
    public float distLeft = 0f;
    public float distDiagLeft = 0f;
    public float distRight = 0f;
    public float distDiagRight = 0f;

    // Parameters
    [SerializeField] private float rotationSpeed = 300f;
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float minVelocity = 0.0f;
    [SerializeField] private float maxVelocity = 6.0f;
    [SerializeField] private float accelerationRate = 2.0f;
    [SerializeField] private float decelerationRate = 0.8f;
    [SerializeField] public float maxViewDistance = 30f;
    [SerializeField] private float fitnessTimeDecreaseRate = 0.6f;
    [SerializeField] private float fitnessCheckpointIncreaseRate = 5f;

    // Learning
    public float fitness = 0f;
    private Vector3 lastPosition;
    private float distanceTraveled = 0f;

    // Neural network output: acceleration and orientation
    public float[] outputs;

    // Become inactive if hits a wall
    public bool isActive = true;


    // Start is called before the first frame update
    void Start()
    {
        unityController = GetComponent<CharacterController>();
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;
        if (outputs.Length > 0)
        {
            /*
            // Player control (test)
            if (Input.GetKey(KeyCode.UpArrow))
            {
               currentVelocity += (accelerationRate * Time.deltaTime);
            }
            else
            {
               currentVelocity -= (decelerationRate * Time.deltaTime);
            }
            */
            // Neural linear movement
            currentVelocity += (accelerationRate * Time.deltaTime) * outputs[0];

            // Agent movement
            currentVelocity = Mathf.Clamp(currentVelocity, minVelocity, maxVelocity);

            moveDir = new Vector3(0, 0, currentVelocity);
            //moveDir *= speed;
            moveDir = transform.TransformDirection(moveDir);
            unityController.Move(moveDir);

            /*
            // Player rotation
            float rotationAngle = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
            */
            // Agent rotation
            float rotationAngle = outputs[1] * rotationSpeed * Time.deltaTime;

            transform.Rotate(0, rotationAngle, 0);
        }

        // Agent vision
        RaycastVision();

        // Fitness management: increase with traveled distance but decrease with time
        agentPosition = transform.position;
        distanceTraveled += Vector3.Distance(agentPosition, lastPosition);
        lastPosition = agentPosition;
        if (distanceTraveled > 0.2f)
        {
            fitness += distanceTraveled / 100;
        }
        fitness -= Time.deltaTime * fitnessTimeDecreaseRate;
    }

    private void RaycastVision()
    {
        agentPosition = transform.position;
        Vector3 forwardDir = transform.forward;
        Vector3 rightDir = transform.right;
        Vector3 leftDir = rightDir * -1;
        Vector3 rightDiagDir = Vector3.Normalize(forwardDir + rightDir);
        Vector3 leftDiagDir = Vector3.Normalize(forwardDir + leftDir);

        Ray forwardRay = new Ray(agentPosition, forwardDir);
        Ray leftRay = new Ray(agentPosition, leftDir);
        Ray leftDiagRay = new Ray(agentPosition, leftDiagDir);
        Ray rightRay = new Ray(agentPosition, rightDir);
        Ray rightDiagRay = new Ray(agentPosition, rightDiagDir);

        RaycastHit hit;
        if (Physics.Raycast(forwardRay, out hit, maxViewDistance) && hit.transform.CompareTag("Wall"))
        {
            distForward = hit.distance;
        }

        if (Physics.Raycast(leftRay, out hit, maxViewDistance) && hit.transform.CompareTag("Wall"))
        {
            distLeft = hit.distance;
        }

        if (Physics.Raycast(leftDiagRay, out hit, maxViewDistance) && hit.transform.CompareTag("Wall"))
        {
            distDiagLeft = hit.distance;
        }

        if (Physics.Raycast(rightRay, out hit, maxViewDistance) && hit.transform.CompareTag("Wall"))
        {
            distRight = hit.distance;
        }

        if (Physics.Raycast(rightDiagRay, out hit, maxViewDistance) && hit.transform.CompareTag("Wall"))
        {
            distDiagRight = hit.distance;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            lastPosition = transform.position;
            fitness -= fitnessCheckpointIncreaseRate;
            isActive = false;
        }

        else if (other.gameObject.CompareTag("Checkpoint") && isActive)
        {
            fitness += fitnessCheckpointIncreaseRate * other.gameObject.GetComponent<Checkpoint>().RewardMultiplier;
        }
    }
}