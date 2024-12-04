using System.Collections.Generic;
using Destructors;
using UnityEngine;
using UnityEngine.Serialization;

/*
 * This script is responsible for the movement of the car.
 * Written by Josh
*/

namespace Car
{
    public class CarMovement : MonoBehaviour
    {
        /*
         the car that i'm using for the test has its direction flipped, and in unity there's no way to fix this.
         the forward vector faces the back of the car, so for all calculations using transform.forward, i'm going to
         use -transform.forward. :)
         - Josh
        */

        [Header("Config")]

        public LayerMask groundLayer;

        public float normalDrag;
        public float airDrag;
        public float driftDrag;

        public float rotationPerUnitTorque;

        public float driftDirection;
        public float alignToGroundTime;
        public bool drifting;
        public Vector3 centerOfWheelsOffset;

        #pragma warning disable CS0414
        private float timeSpentDrifting = 0;
        #pragma warning restore CS0414

        [Header("References")]
        public CarSystem carRef;
        private CarStats carStats;
        private CarDestruction carDestruction;
        private CarStatus carStatus;
        
        [FormerlySerializedAs("motorRB")] public Rigidbody motorRb;
        [FormerlySerializedAs("colliderRB")] public Rigidbody colliderRb;

        public AudioSource engineNoise;
        public AudioSource driftNoise;

        public GameObject driftParticle;
        
        // private stuff
        List<GameObject> particles = new List<GameObject>();
        private float wheelAngle;
        public float moveForce = 0;
        private float driftAngle = 0;

        private void Awake()
        { 
            carStats = carRef.carStats;
            carDestruction = carRef.carDestruction;
            carStatus = carRef.carStatus;
            
            motorRb.transform.parent = null;
            colliderRb.transform.parent = null;
        }

        // Move the car
        public void Move(float vertical, float horizontal)
        {
            // if the car is destroyed, don't move
            if (carStatus.isDestroyed || !carStatus.canMove)
                return;

            if (vertical > 0) print("Trying to go forward");
            
            // if the player's wanted acceleration is opposing the current movement direction, they want to brake.
            // therefore, let them break.

            // but if they're not moving, technically the directions don't match (forward vs not at all), therefore check for that as well

            RaycastHit hit;
            bool isCarGrounded = Physics.Raycast(motorRb.transform.position, -transform.up, out hit, 2f, groundLayer);

            Quaternion toRotateTo = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotateTo, alignToGroundTime * Time.deltaTime);

            // Update engine noise pitch based on velocity
            engineNoise.pitch = 1 + (motorRb.velocity.magnitude / 15);
                
            if (isCarGrounded)
            {
                if (drifting)
                    motorRb.drag = driftDrag;
                else
                    motorRb.drag = normalDrag;
            }
            else
                motorRb.drag = airDrag;
            
            // breaking
            if (Mathf.Sign(transform.InverseTransformDirection(motorRb.velocity).z) != Mathf.Sign(vertical) && vertical != 0 && motorRb.velocity.magnitude != 0)
            {
                moveForce += carStats.breakAcceleration * vertical * Time.deltaTime;
            }
            else // accelerating
            {
                moveForce += carStats.acceleration * vertical * Time.deltaTime;
            }

            if (vertical == 0)
            {
                moveForce -= Mathf.Sign(moveForce) * carStats.breakAcceleration * 0.5f * Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            // if the car is destroyed, don't move
            if (carStatus.isDestroyed || !carStatus.canMove)
                return;

            
            bool isCarGrounded = Physics.Raycast(motorRb.transform.position, -transform.up, 2f, groundLayer);

            moveForce = Mathf.Clamp(moveForce, -carStats.maxSpeed, carStats.maxSpeed);

            if (isCarGrounded)
            {
                
                motorRb.AddForce(transform.forward * moveForce);
            }

            transform.position = motorRb.transform.position;

            Vector3 centerOfWheels = Vector3.zero;
            foreach (DetachableMeshInformation meshInfo in carDestruction.wheels)
            {
                centerOfWheels += meshInfo.objectTransform.localPosition;
            }

            centerOfWheels /= carDestruction.wheels.Count;
            centerOfWheels += centerOfWheelsOffset;

            // print("COW: " + centerOfWheels);
            
            float deltaRotZ = centerOfWheels.x;
            float deltaRotX = centerOfWheels.z;

            colliderRb.MoveRotation(Quaternion.Euler(transform.eulerAngles + Vector3.up * driftAngle) * Quaternion.Euler(deltaRotX * -4, 0, deltaRotZ * 5));
        }

        // Turn the car
        public void Turn(float vertical, float horizontal)
        {
            // if the car is destroyed, don't move
            if (carStatus.isDestroyed || !carStatus.canMove)
                return;
            
            if (drifting)
            {
                horizontal = driftDirection * 0.9f + horizontal * 0.55f;
                driftAngle += + (horizontal * 10 - driftAngle) * 0.2f;
            } else
            {
                driftAngle *= 0.8f;
            }
                

            float amountToRotate = carStats.maxWheelAngle * horizontal * (moveForce / carStats.maxSpeed) * Time.deltaTime;
            transform.Rotate(0, amountToRotate, 0);
        }

        public void StartDrift(int driftDirection)
        {
            if (!driftNoise.isPlaying){
                driftNoise.Play();
            }
            drifting = true;
            this.driftDirection = driftDirection;
        }

        public void UnDrift()
        {
            drifting = false;
            driftNoise.Stop();
        }
        
    }
}
