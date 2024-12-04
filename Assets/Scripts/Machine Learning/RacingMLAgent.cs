using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Car;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;
using Misc;
using Google.Protobuf;
using Unity.VisualScripting;
public class RacingMlAgent : Agent
{
    public CarStats stats;
    public CarMovement movement;
    public CarPosition carPos;

    public Vector3 startPos;
    public Quaternion startRot;
    public Vector3 lastPos;

    float timeSinceLastCheck = 0;
    float timeSpentDrifting = 0;

    public Checkpoint[] checkpoints;
    public int currentCheckpoint;
    public Checkpoint nextCheckpoint;

    public bool isAggressive;
    public Transform aggressiveTarget;
    public DecisionRequester decisionRequester;

    private void Start()
    {
        lastPos = transform.position;
        startPos = lastPos;
        startRot = transform.rotation;
        carPos.currentCheckpoint = 0;
        checkpoints = GameObject.Find("Checkpoints").GetComponentsInChildren<Checkpoint>();
        currentCheckpoint = -1;
        nextCheckpoint = checkpoints[0];
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Vector3.Dot((nextCheckpoint.transform.position - transform.position).normalized, transform.forward));
        sensor.AddObservation(timeSpentDrifting);
        AddReward(-1/2000f);

    }

    void Update()
    {
        // if an aggressive AI gets close to its target
        if (isAggressive && Vector3.Distance(transform.position, aggressiveTarget.position) < 30)
        {
            print("Trying to target player");
            Vector3 towardTarget = (aggressiveTarget.position - transform.position).normalized;
            if (Vector3.Dot(towardTarget, transform.forward) < 0.2f)
            {
                decisionRequester.enabled = true;
                return;
            }

            print("Targetting Player");

            decisionRequester.enabled = false;
            int directionToTurn = Vector3.Dot(towardTarget, transform.right) > Vector3.Dot(towardTarget, -transform.right) ? 1 : -1;
            movement.Move(1.5f, directionToTurn);
            movement.Turn(1, directionToTurn);
        } else
        {
            print("Too far to target");
            decisionRequester.enabled = true;
        }
    }

    public override void OnEpisodeBegin()
    {
        movement.motorRb.isKinematic = true;
        movement.colliderRb.isKinematic = true;

        movement.motorRb.position = startPos;
        movement.colliderRb.position = startPos;

        movement.motorRb.isKinematic = false;
        movement.colliderRb.isKinematic = false;

        transform.rotation = startRot;
        currentCheckpoint = -1;
        nextCheckpoint = checkpoints[0];
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        movement.Move(DiscreteActionToInput(actions.DiscreteActions[0]), DiscreteActionToInput(actions.DiscreteActions[1]));
        movement.Turn(DiscreteActionToInput(actions.DiscreteActions[0]), DiscreteActionToInput(actions.DiscreteActions[1]));

        if (movement.drifting && actions.DiscreteActions[2] == 0)
        {
            movement.UnDrift();
        } else if (!movement.drifting && actions.DiscreteActions[2] == 1)
        {
            movement.StartDrift((int) DiscreteActionToInput(actions.DiscreteActions[1]));
        }
        timeSinceLastCheck += Time.fixedDeltaTime;
        timeSpentDrifting += movement.drifting ? Time.fixedDeltaTime : 0;
        if (timeSinceLastCheck > 10)
        {
            if (timeSpentDrifting > 3)
            {
                print("Drifted for too long, losing 0.1 for every second spent over drifting");
                SetReward(0.1f * timeSpentDrifting - 3);
            }
            timeSinceLastCheck = 0;
            timeSpentDrifting = 0;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
        ActionSegment<int> discreteAction = actionsOut.DiscreteActions;
        discreteAction[2] = movement.drifting ? 1 : 0;
        discreteAction[0] = InputToDiscreteAction(Input.GetAxisRaw("Vertical"));
        discreteAction[1] = InputToDiscreteAction(Input.GetAxisRaw("Horizontal"));

        
    }

    int InputToDiscreteAction(float input)
    {
        if (input == 0)
            return 0;
        if (input > 0)
            return 1;
        if (input < 0)
            return 2;
        return -1;
    } 

    float DiscreteActionToInput(int action)
    {
        if (action == 0)
            return 0;
        if (action == 1)
            return 1;
        return -1;
    }

    public void HitCheckPoint(CarPosition pos, Checkpoint cp)
    {
        print("argh!");
        if (cp == nextCheckpoint)
        {
            print("yummy points: +1");
            SetReward(2);

            currentCheckpoint++;

            if (currentCheckpoint == checkpoints.Length - 1)
            {
                nextCheckpoint = checkpoints[0];
            }
            else
            {
                if (currentCheckpoint == checkpoints.Length)
                {
                    currentCheckpoint = 0;
                    FinishedLap();
                }
                    
                nextCheckpoint = checkpoints[currentCheckpoint + 1];
            }
        }
        
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // EndEpisode();
    }

    public void FinishedLap()
    {
        print("yummy points: finished lap! +1");
        SetReward(2);
       // EndEpisode();
    }
}