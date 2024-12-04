using System.Collections.Generic;
using Unity.MLAgents.Policies;
using UnityEngine.Events;
using Unity.MLAgents;
using System.Linq;
using UnityEngine;
using System.IO;
using Misc;
using Car;
using Destructors;
using Saving_System;
using Random = UnityEngine.Random;

public class RaceManager : MonoBehaviour
{
    // Variables
    private bool _isRaceFinishing;
    private bool _isRaceFinished;
    private bool _isRaceStarting;
    private bool _isRaceStarted;
    public int maxLaps = 3;
    public int maxCars = 1;
    public float countdownTime = 5f;
    private int destroyedCarCount = 0;
    public bool bountyCompleted = false;
    
    // References
    public CarLeaderboard leaderboard;
    private DifficultyManager _difficultyManager;
    private Checkpoint[] _checkpoints;
    private CarCreator _carCreator;
    private string[] _carNames;

    // Dictionaries
    public readonly Dictionary<GameObject, CarSystem> CarSystems = new();
    public UnityEvent<float> raceFinishing; // event called when the game is finishing
    public UnityEvent<float> raceStarting; // event called when the game is starting
    public UnityEvent raceFinished; // event called when the game is finished
    public UnityEvent raceStarted; // event called when the game is started

    public GameObject carVFXExplosion;
    public GameObject carVFXSmoke;
    
    // =================================================================================================================================================
    // Initialization
    // =================================================================================================================================================
    private void Awake()
    {
        // Find references
        leaderboard = FindFirstObjectByType<CarLeaderboard>();
        _difficultyManager = FindFirstObjectByType<DifficultyManager>();
        _carCreator = FindFirstObjectByType<CarCreator>();

        // Load CarNames.txt from streaming assets
        StreamReader reader = new(Application.streamingAssetsPath + "/CarNames.txt");
        _carNames = reader.ReadToEnd().Split('\n');

        // Setup checkpoints
        GameObject checkpointsParent = GameObject.Find("Checkpoints");
        _checkpoints = new Checkpoint[checkpointsParent.transform.childCount];

        for (int i = 0; i < checkpointsParent.transform.childCount; i++)
        {
            Checkpoint thisCheckpoint = checkpointsParent.transform.GetChild(i).GetChild(0).GetComponent<Checkpoint>();
            thisCheckpoint.isFinishLine = i == 0;
            thisCheckpoint.checkpointNumber = i;

            _checkpoints[i] = thisCheckpoint;
        }
    }

    // Initialize the cars
    public int InitializeCars(PlayerData playerData)
    {
        CarSystemData playerSystemData = playerData.carSystemData;

        // This is -1 when the player has a new save
        int carID = playerSystemData.carID == -1 ? 0 : playerSystemData.carID;

        // Create the player car
        GameObject playerCar = _carCreator.CreateCar(carID, 0, playerSystemData.carName, playerSystemData.carInventoryData.upgrades, playerSystemData.carInventoryData.weapons, false);
        playerCar.transform.Find("Car Audio").gameObject.SetActive(true); // Turns on car audio for race
        CarSystem carSystem = playerCar.GetComponent<CarSystem>();
        carSystem.carPosition.isPlayer = true;
        Rigidbody colliderRb = carSystem.carMovement.colliderRb;
        if (colliderRb != null)
        {
            print("it found!");
            colliderRb.gameObject.tag = "Player";
        }
        
        //Initialize the color list
        List<Color> uniqueColors = generateColors();
        // Set up the player's car
        carSystem.carName = playerSystemData.carName;
        carSystem.carID = carID;
        carSystem.carInventory.currency = playerSystemData.carInventoryData.currency;
        carSystem.color = uniqueColors[0];
        carSystem.InitializeIcon();
        carSystem.SetNameTag();
        
        // Add the player car to the dictionary
        AddNewCar(playerCar, carSystem);

        // Set the player's car to the first position, this can be changed later to a random value as long as it is < maxCars
        const int playerPosition = 0;
        carSystem.carPosition.currentPosition = playerPosition;

        // We generate a random index each race to determine the bounty 
        int bountyIndex = Random.Range(1, maxCars);

        // Create the AI cars
        for (int i = 0; i < maxCars; i++)
        {
            if (i == playerPosition) continue; // Skip the player car

            // Set the car's name
            int nameIndex = Random.Range(0, _carNames.Length); // Get a random name
            int colorIndex = Random.Range(1, uniqueColors.Count);

            // Get a random car
            CarInfo randomCar = _difficultyManager.GetRandomCar(); // Get a random car
            var upgrades = _difficultyManager.GetRandomUpgrades(); // Get random upgrades
            var weapons = _difficultyManager.GetRandomWeapons(); // Get random weapons

            GameObject newCar = _carCreator.CreateCar(randomCar.id, i, _carNames[nameIndex], upgrades, weapons, i == bountyIndex); // Create the car
            carSystem = newCar.GetComponent<CarSystem>(); // Get the car system
            carSystem.carID = randomCar.id; // Set the car ID
            carSystem.carPosition.currentPosition = i; // Set the car position
            carSystem.carName = _carNames[nameIndex]; // Set the car name
            if (i == bountyIndex)
            {
                carSystem.carStatus.isBounty = true;
                carSystem.color = Color.red; // Color to red for bounty
            }
            else
            {
                carSystem.carStatus.isBounty = false;
                carSystem.color = uniqueColors[colorIndex]; // Random color
                uniqueColors.RemoveAt(colorIndex); // remove color to prevent duplicates
            }

            // TODO: Combine these three methods into one
            carSystem.InitializeIcon();
            carSystem.SetBodyColor();
            carSystem.SetNameTag();
            newCar.transform.Find("Car Audio").gameObject.SetActive(true); // Turns on AI car audio for race

            _carNames = _carNames.Where((_, index) => index != nameIndex).ToArray(); // remove name from list

            // Enable the ML Agent
            RacingMlAgent racingMlAgent = newCar.GetComponent<RacingMlAgent>();
            racingMlAgent.enabled = true;
            
            // 50% chance to set the AI to aggressive
            if (Random.Range(0, 2) == 1)
            {
                racingMlAgent.isAggressive = true;
                racingMlAgent.aggressiveTarget = playerCar.transform;
            }

            newCar.GetComponent<DecisionRequester>().enabled = true;
            newCar.GetComponent<BehaviorParameters>().enabled = true;
            newCar.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.InferenceOnly;

            AddNewCar(newCar, carSystem); // Add the car to the dictionary
        }

        // Set the next checkpoint for each car
        for (int i = 0; i < maxCars; i++)
        {
            CarPosition carPosition = CarSystems.ElementAt(i).Value.carPosition; // Get the car position of the car
            carPosition.SetNextCheckpoint(_checkpoints[1]); // Set the next checkpoint for each car to the second checkpoint
            carPosition.onCheckpointPassed.AddListener(OnCheckpointPassed); // Subscribe to the checkpoint passed event
            
            // Enable the car systems
            CarSystems.ElementAt(i).Value.EnableAll();
        }

        // Initialize the leaderboard
        leaderboard.InitializeLeaderboard(CarSystems);
        return playerPosition;

        // Add new car to dictionary
        void AddNewCar(GameObject car, CarSystem inCarSystem)
        {
            CarSystems.Add(car, inCarSystem);
        }
    }

    // Called when the race is started
    public void StartRace()
    {
        // Countdown 3 seconds
        countdownTime = 3f;
        _isRaceStarting = true;
        raceStarting.Invoke(countdownTime);
    }

    public int GetDestroyedCarCount(){ return destroyedCarCount; }

    // =================================================================================================================================================
    // Functions called frequently
    // =================================================================================================================================================

    // Update is called once per frame
    private void Update()
    {
        if (_isRaceStarting && !_isRaceStarted)
        {
            countdownTime -= Time.deltaTime;
            if (countdownTime <= 0)
            {
                countdownTime = 5f;
                _isRaceStarting = false;
                _isRaceStarted = true;
                raceStarted.Invoke();
            }
        }

        if (_isRaceFinishing && !_isRaceFinished)
        {
            countdownTime -= Time.deltaTime;
            if (countdownTime <= 0)
            {
                _isRaceFinished = true;
                _isRaceFinishing = false;
                _difficultyManager.raceCount++; // Increase the race count
                raceFinished.Invoke();
            }
        }

        // Check for overtakes
        CheckOvertakes();

        // Update leaderboard
        leaderboard.UpdateLeaderboard();
    }

    // checks if a car has overtaken another car and swaps their positions if necessary
    private void CheckOvertakes()
    {
        // Sort the cars by lap, checkpoint, and distance to checkpoint
        // Just don't ask
        var sortedCars = CarSystems.Values.ToList();
        sortedCars.Sort((a, b) =>
        {
            if (a.carPosition.currentLap != b.carPosition.currentLap)
            {
                return b.carPosition.currentLap.CompareTo(a.carPosition.currentLap);
            }

            if (a.carPosition.currentCheckpoint != b.carPosition.currentCheckpoint)
            {
                return b.carPosition.currentCheckpoint.CompareTo(a.carPosition.currentCheckpoint);
            }

            return b.carPosition.distanceToNextCheckpoint.CompareTo(a.carPosition.distanceToNextCheckpoint);
        });
        
        // Update positions only once after sorting
        for (int i = 0; i < sortedCars.Count; i++)
        {
            sortedCars[i].carPosition.currentPosition = i; // set the new position based on the sort order
        }
    }

    // =================================================================================================================================================
    // Events
    // =================================================================================================================================================
    private void OnCheckpointPassed(CarPosition carPosition, Checkpoint checkpoint)
    {
        switch (checkpoint.isFinishLine)
        {
            case true when carPosition.currentCheckpoint == _checkpoints.Length - 1:
                OnLapPassed(carPosition);
                break;
            case true when !carPosition.started:
                carPosition.started = true;
                break;
        }
        
        if (!carPosition.started) return;

        carPosition.currentCheckpoint = checkpoint.checkpointNumber;
        carPosition.nextCheckpoint = _checkpoints[carPosition.currentCheckpoint < _checkpoints.Length - 1 ? carPosition.currentCheckpoint + 1 : 0];
    }

    private void OnLapPassed(CarPosition carPosition)
    {
        carPosition.currentLap++;
        
        if (carPosition.isPlayer)
        {
            CheckRaceFinished(carPosition); // check if race is finished
        }
    }

    private void CheckRaceFinished(CarPosition carPosition)
    {
        // Check if the car has completed all laps
        if (carPosition.currentLap < maxLaps) return;
        
        raceFinishing.Invoke(countdownTime);
        _isRaceFinishing = true;
    }

    public void OnCarDestroyed(CarDestruction cd, bool isBounty)
    {
        destroyedCarCount++;
        if(isBounty)
        {
            bountyCompleted = true;
        }
        
        // VFX EXPLOSION
        GameObject vfx = Instantiate(carVFXExplosion, cd.transform.position, Quaternion.Euler(0, 0, 0));
        Destroy(vfx, 3f);
        
        // VFX SMOKE
        GameObject vfxSmoke = Instantiate(carVFXSmoke, cd.transform.position, Quaternion.Euler(0, 0, 0));
    }
    
    //Just temp colors to see whats going on
    private List<Color> generateColors()
    {
        return new List<Color> ()
        {
            Color.gray,
            Color.blue,
            Color.cyan,
            Color.green,
            Color.magenta,
            Color.white,
            Color.yellow,
            Color.black
        };
    }
}