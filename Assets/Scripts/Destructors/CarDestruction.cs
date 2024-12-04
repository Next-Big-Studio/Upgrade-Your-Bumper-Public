using System.Collections.Generic;
using Car;
using UnityEngine;
using Unity.MLAgents;
using Random = UnityEngine.Random;
using UnityEngine.Audio;

/*
 * This script is responsible for the deformation of the car's body when it collides with something.
 * - Written by Josh
 */

namespace Destructors
{
    public class DamageTracker
    {
        private float playerDamage;
        private float totalDamage;

        public void AddDamage(bool isPlayer, float damage)
        {
            if (isPlayer)
            {
                playerDamage += damage;
                Debug.Log(string.Format("Player did {0}", damage));
            }
            totalDamage += damage;
        }

        public bool DidPlayerDestroy(float percentage)
        {
            return (playerDamage / totalDamage) >= percentage;
        }
    }
    public class CarDestruction : Destructor
    {
        [Header("References")]
        public CarSystem carSystem;
        private CarStats carStats;
        public List<AudioClip> crashSounds;
        public List<DetachableMeshInformation> wheels;
        public RacingMlAgent agent;

        public DamageTracker damageTracker;
        public float percentageThreshold = 0.3f; // modify threshold here
        public AudioSource audioSource;
        protected override void Awake()
        {
            try
            {
                carStats = carSystem.carStats;

                damageTracker = new DamageTracker();

                base.Awake();
            } catch
            {
                // ignored
            }
        }

        private void Start()
        {
            foreach (DetachableMeshInformation mesh in detachableMeshes)
            {
                print("hi");
                if (mesh.objectTransform.gameObject.name.Contains("Wheel"))
                {
                    print("new wheel!");
                    wheels.Add(mesh);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            DeformMesh(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (agent)
            {
                if (collision.gameObject.layer == 9 || collision.gameObject.layer == 6)
                {
                    if (agent.isAggressive && collision.gameObject.tag == "Player")
                        agent.SetReward(0.1f);
                    else
                        agent.SetReward(-0.1f);
                }
            }
        }

        // This function checks if the player initiated the collision by comparing velocity at the collision point
        private bool CollisionInitiationCheck(Collision collision, bool isPlayer)
        {
            // player wasn't in collision
            if (isPlayer == false) { return false; }

            Rigidbody playerRb = carSystem.GetComponent<Rigidbody>();
            Rigidbody otherRb = collision.rigidbody;

            if (otherRb == null || playerRb == null)
            {
                return false;
            }

            Vector3 collisionPoint = collision.GetContact(0).point;
            Vector3 playerToCollision = (collisionPoint - playerRb.position).normalized;

            float playerApproach = Vector3.Dot(playerRb.velocity.normalized, playerToCollision);
            float otherApproach = Vector3.Dot(otherRb.velocity.normalized, -playerToCollision);

            return playerApproach > otherApproach && playerApproach > 0.5f;
        }


        // Iteratively calls TakeDamageAtPoint for all points in the collision
        // It's a pretty bad name, rename it if u want please
        private void DeformMesh(Collision collision)
        {
            float strengthOfCollision = collision.impulse.magnitude;
            print("Collision strength: " + strengthOfCollision);

            print(collision.transform.name);

            // deformation code here
            if (!(Mathf.Abs(strengthOfCollision) > carStats.minImpulseForDamage)) return;

            // this code MUST maintain its position relative to the return statement above it.
            // do not write code between the previous if statement and this if statement. it will mess up AI training.

            print("ouch! me am hurt!");

            if (agent)
            {
                print("Rewarding points from hitting something: -1");
                agent.SetReward(-1f);
                return;
            }


            bool isPlayer = collision.gameObject.CompareTag("Player");
            float damage = strengthOfCollision / carStats.bodyStrength;
            bool PlayerInitiated = true; // FOR NOW THIS IS true, however, in the future I will use CollisionInitiationCheck(collision, isPlayer); to determine if the player actually crashed into the AI or if the AI crashed into player
            damageTracker.AddDamage(isPlayer, damage);

            print("WHAT THE HELL I JUST TOOK " + damage + " DAMAGE!");

            DrawArrow(collision.transform.position, collision.impulse.normalized, Color.red);

            // Play crash sound using AudioSource
            audioSource.PlayOneShot(crashSounds[Random.Range(0, crashSounds.Count)], strengthOfCollision / (4 * carStats.minImpulseForDamage));

            foreach (ContactPoint point in collision.contacts)
            {
                TakeDamageAtPoint(point.point, point.normal, point.impulse.magnitude * 4,
                    carStats.bodyStrength);
            }
        }

        public void AddMesh(MeshFilter meshFilter)
        {
            DestructableMeshInformation mesh = new DestructableMeshInformation();
            mesh.meshFilter = meshFilter;
            mesh.mesh = mesh.meshFilter.mesh;
            mesh.normalVertices = mesh.mesh.vertices; // keep a copy of the original
            mesh.modifiedVertices = mesh.mesh.vertices;
            mesh.objectTransform = mesh.meshFilter.transform;
            if (mesh.meshFilter.GetComponent<MeshCollider>())
            {
                mesh.meshCollider = mesh.meshFilter.GetComponent<MeshCollider>();
            }

            meshes.Add(mesh);
        }

        public void RemoveMesh(MeshFilter meshFilter)
        {
            DestructableMeshInformation mesh = meshes.Find(m => m.meshFilter == meshFilter);
            if (mesh != null)
            {
                meshes.Remove(mesh);
            }
        }

        protected override void LoseMesh(DetachableMeshInformation mesh)
        {
            base.LoseMesh(mesh);
            if (wheels.Contains(mesh))
            {
                wheels.Remove(mesh);

                if (wheels.Count == 0 && !carSystem.carStatus.isDestroyed)
                {
                    bool didPlayerDestroy = damageTracker.DidPlayerDestroy(percentageThreshold);
                    if (didPlayerDestroy)
                    {
                        RaceManager raceManager = FindFirstObjectByType<RaceManager>();
                        if (raceManager)
                        {
                            raceManager.OnCarDestroyed(this, carSystem.carStatus.isBounty);
                        }
                    }
                    carSystem.carStatus.DestroyCar();
                }
            }
        }

        public override void TakeDamageAtPoint(Vector3 point, Vector3 direction, float damage)
        {
            TakeDamageAtPoint(point, direction, damage, carStats.bodyStrength);
        }

        public override void TakeDamageAtPoint(Vector3 point, Vector3 direction, float damage, float bodyStrength)
        {
            base.TakeDamageAtPoint(point, direction, damage, bodyStrength);

            print("My total deformation at this point is: " + totalDeformation);

            if (maxDeformation < totalDeformation && !carSystem.carStatus.isDestroyed)
            {
                bool didPlayerDestroy = damageTracker.DidPlayerDestroy(percentageThreshold);
                if (didPlayerDestroy)
                {
                    RaceManager raceManager = FindFirstObjectByType<RaceManager>();
                    if (raceManager)
                    {
                        raceManager.OnCarDestroyed(this, carSystem.carStatus.isBounty);
                    }
                }

                carSystem.carStatus.DestroyCar();
            }
        }

        public static void DrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction, color, 30);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
                            new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
                           new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color, 30);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color, 30);
        }
    }
}