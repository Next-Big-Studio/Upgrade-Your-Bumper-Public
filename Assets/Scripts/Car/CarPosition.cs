using System;
using Car;
using Misc;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Car
{
    public class CarPosition : MonoBehaviour
    {
        public GameObject frontOfCar;
        public bool isMovingForward;
        public int currentPosition;
        public int currentLap;
        public int currentCheckpoint;
        public bool isPlayer;
        public bool started;
        
        [Header("Events")]
        public UnityEvent<CarPosition, Checkpoint> onCheckpointPassed;

        public Checkpoint nextCheckpoint;
        public float distanceToNextCheckpoint;

        [Header("AI")]
        public AggressiveAITrainingEnvironment environment;
        public RacingMlAgent agent;

        // Warning system
        private const float WarningMaxTime = 4;
        public float warningTimer;
        
        private void Start()
        {
            currentCheckpoint = -1;
            currentLap = 0;
            warningTimer = WarningMaxTime;
        }
        
        private void Update()
        {
            // Calculate the distance from the next checkpoint
            float fromCheckpoint = Vector3.Distance(frontOfCar.transform.position, nextCheckpoint.transform.position);
            if (fromCheckpoint < distanceToNextCheckpoint)
            {
                isMovingForward = true;
                warningTimer = WarningMaxTime;
            } else if (fromCheckpoint > distanceToNextCheckpoint)
            {
                warningTimer -= Time.deltaTime;
                isMovingForward = false;
            }
            
            // Update the distance to the next checkpoint
            distanceToNextCheckpoint = fromCheckpoint;
            
            // Warn the player if they are going the wrong way
            if (warningTimer <= 0 && !isMovingForward)
            {
                Warn();
            }
        }
        
        // Called when a car passes a checkpoint
        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Checkpoint"))
            {
                Checkpoint checkpoint = other.GetComponent<Checkpoint>();
                onCheckpointPassed.Invoke(this, checkpoint);
            }
        }
        
        public void SetNextCheckpoint(Checkpoint checkpoint)
        {
            nextCheckpoint = checkpoint;
        }

        // Called when a car is going the wrong way
        private void Warn()
        {
            // TODO: Add warning sound
            // TODO: Add warning visual
            // TODO: Add warning vibration
        }
    }
}