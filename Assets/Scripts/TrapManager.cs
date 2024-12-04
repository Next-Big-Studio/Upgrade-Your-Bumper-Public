using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Car;

public class TrapManager : MonoBehaviour
{
    // Configuration class for different trap types
    [System.Serializable]
    public class TrapType
    {
        public enum Type { EMP, OilSlick }
        public Type trapType;
        public GameObject trapPrefab;
        public float effectDuration = 3f;      // How long the trap effect lasts
        public float cooldownTime = 5f;        // Time before trap type can be spawned again
        public Color visualizationColor = Color.red;
    }

    public List<TrapType> availableTraps;

    [Header("Spawn Settings")]
    public float minSpawnDistance = 10f;       // Minimum distance from racetrack to spawn trap
    public float maxSpawnDistance = 50f;       // Maximum distance from racetrack to spawn trap
    public int maxActiveTraps = 5;            // Maximum number of traps allowed at once
    public float cleanupInterval = 30f;       // How often to check and remove inactive traps
    public int poolSize = 10;                 // Size of object pool for each trap type

    [Header("Visualization")]
    public bool showSpawnRange = true;        // Whether to show spawn range gizmos
    public float visualizationDuration = 2f;  // How long to show spawn visualization

    // Object pooling and tracking collections
    private List<GameObject> activatedTraps = new List<GameObject>();
    private Dictionary<TrapType.Type, float> trapCooldowns = new Dictionary<TrapType.Type, float>();
    private Dictionary<TrapType.Type, Queue<GameObject>> trapPools = new Dictionary<TrapType.Type, Queue<GameObject>>();

    private void Start()
    {
        InitializeTrapPools();
        StartCoroutine(CleanupRoutine());
    }

    // Initialize object pools for each trap type
    private void InitializeTrapPools()
    {
        foreach (TrapType trap in availableTraps)
        {
            trapCooldowns[trap.trapType] = 0f;
            Queue<GameObject> pool = new Queue<GameObject>();
            
            // Create pool of inactive trap instances
            for (int i = 0; i < poolSize; i++)
            {
                GameObject instance = Instantiate(trap.trapPrefab);
                instance.SetActive(false);
                pool.Enqueue(instance);
            }
            
            trapPools[trap.trapType] = pool;
        }
    }

    // Main method to spawn a trap at a given position on the racetrack
    public void SpawnTrap(Vector3 racetrackPosition)
    {
        CleanupInactiveTraps();

        if (activatedTraps.Count >= maxActiveTraps)
            return;

        TrapType selectedTrap = GetAvailableTrap();
        if (selectedTrap == null)
            return;

        Vector3 spawnPosition = CalculateSpawnPosition(racetrackPosition);
        GameObject trapInstance = GetTrapFromPool(selectedTrap.trapType);
        
        if (trapInstance == null)
            return;

        ConfigureTrap(trapInstance, selectedTrap, spawnPosition);
        StartCoroutine(VisualizeTrapSpawn(spawnPosition, selectedTrap.visualizationColor));
    }

    // Calculate random position within spawn range
    private Vector3 CalculateSpawnPosition(Vector3 racetrackPosition)
    {
        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0;
        randomDirection.Normalize();
        
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector3 spawnPosition = racetrackPosition + randomDirection * distance;
        
        // Ensure trap is placed on ground using raycast
        RaycastHit hit;
        if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            return hit.point;
        }
        
        return spawnPosition;
    }

    // Get a random trap type that isn't on cooldown
    private TrapType GetAvailableTrap()
    {
        List<TrapType> availableTypes = availableTraps.FindAll(
            trap => !trapCooldowns.ContainsKey(trap.trapType) || 
            Time.time >= trapCooldowns[trap.trapType]);

        return availableTypes.Count > 0 ? 
            availableTypes[Random.Range(0, availableTypes.Count)] : null;
    }

    // Get trap instance from object pool
    private GameObject GetTrapFromPool(TrapType.Type trapType)
    {
        if (!trapPools.ContainsKey(trapType))
            return null;

        Queue<GameObject> pool = trapPools[trapType];
        
        if (pool.Count == 0)
            return null;

        GameObject trap = pool.Dequeue();
        trap.SetActive(true);
        return trap;
    }

    // Return trap to object pool
    private void ReturnTrapToPool(GameObject trap, TrapType.Type trapType)
    {
        if (!trapPools.ContainsKey(trapType))
            return;

        trap.SetActive(false);
        trapPools[trapType].Enqueue(trap);
    }

    // Setup trap instance with necessary components and settings
    private void ConfigureTrap(GameObject trapInstance, TrapType trapType, Vector3 position)
    {
        trapInstance.transform.position = position;
        trapInstance.transform.rotation = Quaternion.identity;
        
        TrapBehavior trapBehavior = trapInstance.GetComponent<TrapBehavior>();
        if (trapBehavior == null)
            trapBehavior = trapInstance.AddComponent<TrapBehavior>();
        
        trapBehavior.Initialize(this, trapType);
        activatedTraps.Add(trapInstance);
        trapCooldowns[trapType.trapType] = Time.time + trapType.cooldownTime;
    }

    // Visual feedback when trap is spawned
    private IEnumerator VisualizeTrapSpawn(Vector3 position, Color color)
    {
        if (showSpawnRange)
        {
            Debug.DrawLine(position, position + Vector3.up * 2f, color, visualizationDuration);
            yield return new WaitForSeconds(visualizationDuration);
        }
    }

    // Remove any destroyed or deactivated traps from tracking
    private void CleanupInactiveTraps()
    {
        activatedTraps.RemoveAll(trap => trap == null || !trap.activeInHierarchy);
    }

    // Periodic cleanup routine
    private IEnumerator CleanupRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(cleanupInterval);
            CleanupInactiveTraps();
        }
    }

    // Apply trap effects to cars that trigger them
    public void ApplyTrapEffect(CarSystem carSystem, TrapType.Type trapType, float duration)
    {
        switch (trapType)
        {
            case TrapType.Type.EMP:
                StartCoroutine(EMPCoroutine(carSystem, duration));
                break;
            case TrapType.Type.OilSlick:
                StartCoroutine(OilSlickCoroutine(carSystem, duration));
                break;
        }
    }

    // EMP effect: Temporarily disable car movement
    private IEnumerator EMPCoroutine(CarSystem carSystem, float duration)
    {
        carSystem.carStatus.canMove = false;
        carSystem.carMovement.engineNoise?.Pause();
        
        yield return new WaitForSeconds(duration);
        
        carSystem.carStatus.canMove = true;
        carSystem.carMovement.engineNoise?.UnPause();
    }

    // Oil Slick effect: Reduce car speed and acceleration
    private IEnumerator OilSlickCoroutine(CarSystem carSystem, float duration)
    {
        float originalMoveForce = carSystem.carMovement.moveForce;
        float originalAcceleration = carSystem.carStats.acceleration;

        carSystem.carMovement.moveForce *= 0.5f;
        carSystem.carStats.acceleration *= 0.5f;

        yield return new WaitForSeconds(duration);

        carSystem.carMovement.moveForce = originalMoveForce;
        carSystem.carStats.acceleration = originalAcceleration;
    }

    // Remove all active traps
    public void ClearAllTraps()
    {
        foreach (GameObject trap in activatedTraps)
        {
            if (trap != null)
            {
                TrapBehavior behavior = trap.GetComponent<TrapBehavior>();
                if (behavior != null)
                    ReturnTrapToPool(trap, behavior.trapType);
            }
        }
        activatedTraps.Clear();
    }

    // Visualize spawn range in editor
    private void OnDrawGizmos()
    {
        if (showSpawnRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minSpawnDistance);
            Gizmos.DrawWireSphere(transform.position, maxSpawnDistance);
        }
    }
}

// Component attached to individual trap instances
public class TrapBehavior : MonoBehaviour 
{
    public TrapManager.TrapType.Type trapType;
    private TrapManager trapManager;
    private float effectDuration;

    public void Initialize(TrapManager manager, TrapManager.TrapType trapConfig)
    {
        trapManager = manager;
        trapType = trapConfig.trapType;
        effectDuration = trapConfig.effectDuration;
        
        // Ensure we have a collider and it's set as trigger
        if (!GetComponent<Collider>())
        {
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
        }
    }

    // Try both OnTriggerEnter and OnCollisionEnter
    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
    }

    private void OnCollisionEnter(Collision collision) 
    {
        HandleCollision(collision.collider);
    }

    private void HandleCollision(Collider other)
    {
        // Check for both direct car component and parent
        CarSystem carSystem = other.GetComponent<CarSystem>();
        if (carSystem == null)
        {
            carSystem = other.GetComponentInParent<CarSystem>();
        }

        if (carSystem != null)
        {
            Debug.Log($"Trap triggered by car: {carSystem.carName}");
            trapManager.ApplyTrapEffect(carSystem, trapType, effectDuration);
            gameObject.SetActive(false);
        }
    }
}