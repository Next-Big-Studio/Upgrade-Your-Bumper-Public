using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Car.Player;
using Upgrades;
using Weapons;
using Misc;
using Car;
using Saving_System;
using UI;
using UI.Cutscenes;
using UI.Main_Menu;
using UnityEngine.Playables;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // How many races before the boss fight is triggered
    public int MaxRaces = 2;
    
    public static GameManager Instance { get; set; }
    public SaveSystem saveSystem;

    // The list of all cars
    public List<CarInfo> allCars;

    // The list of all upgrades and weapons
    public List<Upgrade> allUpgrades;
    public List<WeaponInfo> allWeapons;

    // Managers 
    public EnvironmentManager environmentManager;
    public DifficultyManager difficultyManager;
    public PodiumManager podiumManager;
    public RaceManager raceManager;
    public ShopManager shopManager;
    public CarCreator carCreator;
    private SceneManager _sceneManager;

    // Player references
    public CarSystem playerSystem;
    public PlayerControls playerControls;
    public HUD playerHUD;

    // Scene Management
    private string _loadThisScene = "";
    private List<string> _racingScenes;

    public readonly int[] LeaderboardAmounts = {1000, 500, 250 };
    public int playerGainedCurrency = 0;
    public bool BountyWon = false;

    // Time Trial Information
    public bool playedTimeTrial = false;
    public bool wonTimeTrial = false;

    // =================================================================================================================================================
    // =================================================== Initialization =============================================================================
    // =================================================================================================================================================

    // Called when the game starts
    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneChange;
            
            // Load the save system
            saveSystem = FindFirstObjectByType<SaveSystem>();
            
            // Load the racing scenes
            GetRacingScenes();
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    // Called when the game starts
    private void Start()
    {
        // Load the save system
        saveSystem = FindFirstObjectByType<SaveSystem>();
        difficultyManager = FindFirstObjectByType<DifficultyManager>();
    }

    // =================================================================================================================================================
    // =================================================== Event Listeners =============================================================================
    // =================================================================================================================================================

    // Called when the scene changes
    private void OnSceneChange(Scene scene, LoadSceneMode mode)
    {
        // Find the save system if it's null
        if (saveSystem == null)
        {
            saveSystem = FindFirstObjectByType<SaveSystem>();
        }
        
        // Check which scene is being loaded
        switch (scene.name)
        {
            case "Main Menu":
                MainMenuScene();
                break;
            case "Credits":
                // Do Nothing
                break;
            case "Shop":
                ShopScene();
                break;
            case "Game Over":
                GameOverScene();
                break;
            case "Loading":
                LoadingScreenScene();
                break;
            case "Leaderboard":
                LeaderboardScene();
                break;
            case "Boss":
                Boss();
                break;
            // CUTSCENES
            case "Intro":
                IntroCutscene();
                break;
            case "Lost":
                LostCutscene();
                break;
            case "Won":
                WonCutscene();
                break;
            // END CUTSCENES
            default:
                RaceScene();
                break;
        }
    }
    
    // Called when the race is started
    private void StartRace()
    {
        // Enable the player's controls
        playerControls.SetMovementControls(true);

        for (int i = 0; i < raceManager.CarSystems.Count; i++)
        {
            raceManager.CarSystems.ElementAt(i).Value.EnableAll();
            raceManager.CarSystems.ElementAt(i).Value.carStatus.EnableCar();
        }
    }

    // Called when the race has ended
    private void RaceEnded()
    {
        AwardCurrency();
        
        // Save player data
        saveSystem.SavePlayerFromCarSystem(playerSystem);
        
        saveSystem.gameData.playerData.raceTime = playerHUD.elapsedTime.time;
        
        if (playerSystem.carStatus.isDestroyed)
        {
            saveSystem.gameData.playerData.didFinish = false;
        }
        else
        {
            saveSystem.gameData.playerData.didFinish = playerSystem.carPosition.currentLap >= raceManager.maxLaps;
        }
        
        // Disable the player's controls
        playerControls.SetMovementControls(false);
        
        // Save the leaderboard
        saveSystem.SaveLeaderboardData(new LeaderboardData(raceManager.leaderboard));
        saveSystem.gameData.playerData.numberOfRaces++;
        
        bool playerDestroyed = playerSystem.carStatus.isDestroyed;
        _loadThisScene = playerDestroyed ? "Shop" : "Leaderboard";
        
        if (playerDestroyed)
        {
            saveSystem.gameData.playerData.timesDestroyed++;
        }
        else
        {
            saveSystem.gameData.playerData.timesSurvived++;
        }
        
        SceneLoader.LoadScene("Loading");
    }
    
    // Called when the player exits the leaderboard scene
    private void PlayerExitsLeaderboard()
    {
        // Go to shop
        _loadThisScene = "Shop";
        SceneLoader.LoadScene("Loading");
    }

    // Called when the player exits the shop scene
    private void PlayerExitsShop()
    {
        // Save
        saveSystem.SavePlayerFromCarSystem(shopManager.playerSystem);
        
        if (shopManager.playerSystem.carStatus.isDestroyed)
        {
            saveSystem.gameData.isFinished = true;
            
            // Go to game over
            _loadThisScene = "Game Over";
            SceneLoader.LoadScene("Loading");
            return;
        }

        // Go to racetrack
        _loadThisScene = saveSystem.gameData.playerData.timesSurvived >= MaxRaces ? "Boss" : GetRandomRace(); 
        SceneLoader.LoadScene("Loading");
    }
    
    // Called when the player exits to the main menu
    public void PlayerExitsToMainMenu()
    {
        // Save
        saveSystem.SavePlayerFromCarSystem(playerSystem);
        saveSystem.SaveLeaderboardData(new LeaderboardData(raceManager.leaderboard));
        
        // Go to main menu
        _loadThisScene = "Main Menu";
        SceneLoader.LoadScene("Loading");
    }

    // Called when the player dies in the boss fight
    private void PlayerDiesBoss() {
        saveSystem.gameData.playerData.carSystemData.carStatusData.isDestroyed = true;
        saveSystem.gameData.playerData.timesDestroyed++;
        saveSystem.gameData.playerData.carSystemData.carName = "Max V.";
        
        saveSystem.SavePlayerFromCarSystem(playerSystem);
        
        _loadThisScene = "Lost";
        SceneLoader.LoadScene("Loading");
    }

    // Called when the player wins the boss fight
    private void PlayerWinsBoss() {
        saveSystem.gameData.playerData.carSystemData.carStatusData.isDestroyed = false;
        saveSystem.gameData.playerData.timesSurvived++;
        saveSystem.gameData.playerData.carSystemData.carName = "Max V.";
        
        saveSystem.SavePlayerFromCarSystem(playerSystem);
        
        _loadThisScene = "Won";
        SceneLoader.LoadScene("Loading");
    }
    
    // =================================================================================================================================================
    // =================================================== SCENE  MANAGEMENT ===========================================================================
    // =================================================================================================================================================
    
    // Called when the player is in the main menu
    private void MainMenuScene()
    {
        // Get main menu handler
        MainMenuHandler mainMenuHandler = FindFirstObjectByType<MainMenuHandler>();
        
        // Do nothing
        bool saveCreated = saveSystem.CheckLatestSave();
        
        MainMenu mainMenu = FindFirstObjectByType<MainMenu>();
        mainMenu.Setup(saveCreated);

        // Set random scene
        if (saveSystem.gameData.playerData.carSystemData.carStatusData.isDestroyed)
        {
            _loadThisScene = "Shop";
        } else if (saveCreated)
        {
            _loadThisScene = "Intro";
        }
        else
        {
            GetRacingScenes();
            _loadThisScene = GetRandomRace();
        }
        
        if (difficultyManager == null)
        {
            difficultyManager = FindFirstObjectByType<DifficultyManager>();
            // Set the difficulty manager variables
            difficultyManager.SetVariables(allCars, allUpgrades, allWeapons, saveSystem.gameData.difficulty);
        }
    }

    // Called when the player is in the shop
    private void ShopScene()
    {
        if (shopManager == null)
        {
            shopManager = FindFirstObjectByType<ShopManager>();
        }

        // Initialize the shop
        shopManager.InitializeShop(allUpgrades, allWeapons);
        
        // Load player data from the save system
        shopManager.InitializePlayer(saveSystem.gameData, GetCarFromID(saveSystem.gameData.playerData.carSystemData.carID).carPrefab);
        
        // Already applied the stats

        // Apply the visual upgrades
        shopManager.playerSystem.carStats.ApplyVisualUpgrades(saveSystem.gameData.playerData.carSystemData.carInventoryData.upgrades.ToArray());

        // Mount the weapons
        shopManager.playerSystem.MountWeapons(allWeapons);

        // Listen for when the player exits the shop
        shopManager.onExit.AddListener(PlayerExitsShop);

        float timer = 0f;
        if (wonTimeTrial && playedTimeTrial)
        {
            playedTimeTrial = false;
            shopManager.carInventory.currency += 1000;
            shopManager.PopupMessage(null, "You won the job! You have been awarded $1000!", 5f);
            GameObject.Find("Job Button").SetActive(false);
            timer = 5f;
        } else if (!wonTimeTrial && playedTimeTrial)
        {
            playedTimeTrial = false;
            shopManager.PopupMessage(null, "You lost the job! Better luck next time!", 5f);
            timer = 5f;
        }
        
        // Wait 5 seconds before showing the boss count
        Invoke(nameof(shopManager.FinalizeShop), timer);
    }
    
    // Called when the player is in a race
    private void RaceScene()
    {
        // Find the managers if they are null
        // --------------------------------------------------------------------------------------------
        
        if (carCreator == null)
        {
            carCreator = FindFirstObjectByType<CarCreator>();
            carCreator.baseCars = allCars;
        }

        if (raceManager == null)
        {
            raceManager = FindFirstObjectByType<RaceManager>();
        }

        if (playerControls == null)
        {
            playerControls = FindFirstObjectByType<PlayerControls>();
        }

        if (playerHUD == null)
        {
            playerHUD = FindFirstObjectByType<HUD>();
        }
        
        if (environmentManager == null)
        {
            environmentManager = FindFirstObjectByType<EnvironmentManager>();
        }
        
        // --------------------------------------------------------------------------------------------
        
        // Listen for the ending and starting of the race
        raceManager.raceFinishing.AddListener(playerHUD.StartEnding);
        raceManager.raceFinished.AddListener(RaceEnded);
        raceManager.raceStarting.AddListener(playerHUD.StartStarting);
        raceManager.raceStarted.AddListener(StartRace);
        
        // Check if it's going to rain
        environmentManager.RollForWeather();
        
        // Initialize the race
        InitializeRace();
        
        return;

        // Called before the race is started
        void InitializeRace()
        {
            // Initialize the cars in the race
            int playerIndex = raceManager.InitializeCars(saveSystem.gameData.playerData);

            // Set player references
            playerSystem = raceManager.CarSystems.ElementAt(playerIndex).Value; // set the player's car system
            playerControls.AssignCar(raceManager.CarSystems.First().Key); // set the player's car
            playerHUD.lapText.maxLaps = raceManager.maxLaps; // set the max laps for the HUD
            playerHUD.SetPlayerCar(playerSystem); // set the player's car for the HUD
            playerSystem.mysteryBoxEvent.AddListener(onPlayerMysteryBoxEvent); // listen for when the player gets an upgrade from the mystery box

            // Listen for when the player is destroyed
            playerSystem.carStatus.onCarDestroyed.AddListener(RaceEnded);
            foreach (var carSystem in raceManager.CarSystems)
            {
                // Apply the upgrades to the car
                carSystem.Value.carStats.ApplyUpgrades(GetUpgradeFromIDs(carSystem.Value.carInventory.upgrades).ToArray());
                carSystem.Value.carStats.ApplyVisualUpgrades(carSystem.Value.carInventory.upgrades.ToArray());

                // Mount the weapons
                carSystem.Value.MountWeapons(allWeapons);
            }

            // Begins the actual race
            raceManager.StartRace();
        }
    }

    // Called when the game over scene is shown
    private void GameOverScene()
    {
        if (playerSystem == null)
        {
            playerSystem = FindFirstObjectByType<CarSystem>();
        }
        
        GameOverManager gameOverManager = FindFirstObjectByType<GameOverManager>();
        gameOverManager.Initialize(allUpgrades, allWeapons, GetCarFromID(saveSystem.gameData.playerData.carSystemData.carID).carPrefab);
        
        // Expire the save
        saveSystem.ExpireSave();
    }

    // Called when the leaderboard scene is shown
    private void LeaderboardScene()
    {
        podiumManager = FindFirstObjectByType<PodiumManager>();
        
        // ----------------------------
        
        // Initialize the leaderboard
        podiumManager.SetupPodium(saveSystem.gameData.allRaces.Last(), allCars, allUpgrades, allWeapons);
        
        // Listen for when the player exits the leaderboard
        podiumManager.onExit.AddListener(PlayerExitsLeaderboard);
    }
    
    // Called when the loading screen is shown
    private void LoadingScreenScene()
    {
        // Load the next scene after a few seconds
        StartCoroutine(LoadNextScene());
        return;

        // Load the next scene
        IEnumerator LoadNextScene()
        {
            yield return new WaitForSeconds(3);
            SceneLoader.LoadScene(_loadThisScene);
            _loadThisScene = "";
        }
    }
    
    // Called when the introduction cutscene is shown
    private void IntroCutscene()
    {
        _loadThisScene = GetRandomRace();
    }

    // Called when the lost cutscene is shown
    private void LostCutscene()
    {
        _loadThisScene = "Game Over";
        
        Button continueButton = FindFirstObjectByType<Button>();
        continueButton.onClick.AddListener(LoadLoadingScreen);
        
        // Get the playable director
        PlayableDirector director = FindFirstObjectByType<PlayableDirector>();
        director.stopped += LoadLoadingScreen;
    }
    
    // Called when the won cutscene is shown
    private void WonCutscene()
    {
        _loadThisScene = "Game Over";
        
        Button skipButton = FindFirstObjectByType<Button>();
        skipButton.onClick.AddListener(LoadLoadingScreen);
        
        // Get the playable director
        PlayableDirector director = FindFirstObjectByType<PlayableDirector>();
        director.stopped += LoadLoadingScreen;
    }

    private void LoadLoadingScreen(PlayableDirector obj)
    {
        LoadLoadingScreen();
    }

    // Called when the boss fight is triggered
    private void Boss()
    {
        // Find object called spawn point
        GameObject spawnPoint = GameObject.Find("Spawn");
        
        // Find the carlos object
        GameObject carlos = GameObject.Find("Carlos");

        playerHUD = GameObject.Find("HUD").GetComponent<HUD>();
        
        // Save System
        PlayerData playerData = saveSystem.gameData.playerData;

        int carID = saveSystem.gameData.playerData.carSystemData.carID == -1 ? 1 : saveSystem.gameData.playerData.carSystemData.carID;
        string carName = saveSystem.gameData.playerData.carSystemData.carName == "" ? "Max V." : saveSystem.gameData.playerData.carSystemData.carName;
        
        // Create the car
        GameObject playerCar = Instantiate(allCars[carID].carPrefab, spawnPoint.transform.position, Quaternion.identity);
        playerCar.name = carName; // Set the name of the car
        playerCar.GetComponent<CarInventory>().upgrades = playerData.carSystemData.carInventoryData.upgrades; // Add the upgrades
        playerCar.GetComponent<CarInventory>().weapons = playerData.carSystemData.carInventoryData.weapons; // Add the weapons
        playerCar.transform.Find("Car Audio").gameObject.SetActive(true); // Turns on car audio for race
        
        playerSystem = playerCar.GetComponent<CarSystem>();
        
        // Apply the upgrades to the car
        playerSystem.carStats.ApplyUpgrades(GetUpgradeFromIDs(playerSystem.carInventory.upgrades).ToArray());
        playerSystem.carStats.ApplyVisualUpgrades(playerSystem.carInventory.upgrades.ToArray());

        // Mount the weapons
        playerSystem.MountWeapons(allWeapons);
        
        playerSystem.carInventory.currency = playerData.carSystemData.carInventoryData.currency;

        playerSystem.carDestruction.enabled = true;
        playerSystem.carMovement.enabled = true;

        playerSystem.carStatus.canFire = true;
        playerSystem.carStatus.canBoost = true;
        playerSystem.carStatus.canDrift = true;
        playerSystem.carStatus.canMove = true;
        playerSystem.carStatus.canUseItem = true;

        playerControls = FindFirstObjectByType<PlayerControls>();
        try
        {
            playerControls.AssignCar(playerCar);
        }
        catch
        {
            Debug.Log("Error assigning car controls");
        }
        playerHUD.SetPlayerCar(playerSystem);
        
        playerSystem.carStatus.onCarDestroyed.AddListener(PlayerDiesBoss);
        carlos.GetComponent<CarlosBoss>().onCarDestroyed.AddListener(PlayerWinsBoss);
    }
    
    // =================================================================================================================================================
    // =================================================== Helper Functions ===========================================================================
    // =================================================================================================================================================

    // Find the list of CarInfos from an int
    private CarInfo GetCarFromID(int id)
    {
        return allCars.Find(car => car.id == id);
    }
    
    // Find the list of Upgrades from the list of UpgradeData
    private List<Upgrade> GetUpgradeFromIDs(List<UpgradeData> upgradeData)
    {
        if (upgradeData == null) return new List<Upgrade>();
        
        var upgrades = new List<Upgrade>();

        foreach (UpgradeData data in upgradeData)
        {
           for (int i = 0; i < data.amount; i++)
           {
               upgrades.Add(allUpgrades.Find(upgrade => upgrade.id == data.id));
           } 
        }

        return upgrades;
    }
    
    // returns the upgrade with the given id
    public Upgrade GetUpgradeFromID(int id)
    {
        return allUpgrades.Find(upgrade => upgrade.id == id);
    }
    
    // Find the list of Weapons from the list of WeaponData
    private List<WeaponInfo> GetWeaponFromIDs(List<WeaponData> weaponData)
    {
        if (weaponData == null) return new List<WeaponInfo>();
        
        var weapons = new List<WeaponInfo>();

        foreach (WeaponData data in weaponData)
        {
            for (int i = 0; i < data.amount; i++)
            {
                weapons.Add(allWeapons.Find(weapon => weapon.id == data.id));
            }
        }

        return weapons;
    }
    
    // Awards the currency to the player from the leaderboard, players kills, and bounty won or not.
    private void AwardCurrency()
    {
        playerGainedCurrency = 0;
        int numberOfKills = raceManager.GetDestroyedCarCount();
        saveSystem.gameData.playerData.raceKills = numberOfKills;
        
        raceManager.leaderboard.carSystems[0].carInventory.currency += LeaderboardAmounts[0];
        raceManager.leaderboard.carSystems[1].carInventory.currency += LeaderboardAmounts[1];
        raceManager.leaderboard.carSystems[2].carInventory.currency += LeaderboardAmounts[2];
        
        playerSystem.carInventory.currency += numberOfKills * 100;
        playerSystem.carInventory.currency += raceManager.bountyCompleted ? 1500 : 0;
        saveSystem.gameData.playerData.numberOfKills += raceManager.GetDestroyedCarCount();

        int leaderboardGained = 0;
        
        for (int i = 0; i < 3; i++)
        {
            if (raceManager.leaderboard.carSystems[i].carName != "Max V.") continue;
            
            leaderboardGained = LeaderboardAmounts[i];
            break;
        }

        playerGainedCurrency = numberOfKills * 100 + (raceManager.bountyCompleted ? 1500 : 0) + leaderboardGained;
        
        BountyWon = raceManager.bountyCompleted;
    }
    
    // This loads the loading screen with no scene name
    // Only used for the cutscenes
    public void LoadLoadingScreen()
    {
        LoadThisScene();
    }
    
    // This loads the loading screen with a scene name
    public void LoadThisScene(string scene = "")
    {
        _loadThisScene = scene == "" ? _loadThisScene : scene;
        SceneLoader.LoadScene("Loading");
    }

    // Finds all the races in the build settings
    private void GetRacingScenes()
    {
        if (_racingScenes != null) return;
        
        _racingScenes = new List<string>();
        
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            // Since Unity didn't want to fix the bug at SceneManager.GetSceneByBuildIndex(i).name returning null, I had to do this
            string scene = SceneUtility.GetScenePathByBuildIndex(i);
            if (!scene.ToLower().Contains("track")) continue;
            
            // Mmmm yes, string manipulation
            _racingScenes.Add(scene.Substring(scene.LastIndexOf('/') + 1, scene.LastIndexOf('.') - scene.LastIndexOf('/') - 1));
        }
    }

    // Gets a random race from the list of racing scenes
    private string GetRandomRace()
    {
        return _racingScenes[Random.Range(0, _racingScenes.Count)];
    }
    
    public void onPlayerMysteryBoxEvent(Upgrade upgradeData)
    {
        playerHUD.ShowMysteryBox(upgradeData);
    }
    
    // =================================================================================================================================================
    // =================================================== Utility Functions ==========================================================================
    // =================================================================================================================================================
    
    // Just in case this function is needed
    public void OnApplicationQuit()
    {
        if (saveSystem == null) return;
        
        // check what scene the player is in
        switch (SceneManager.GetActiveScene().name)
        {
            case "Shop":
                saveSystem.SavePlayerFromCarSystem(shopManager.playerSystem);
                break;
            default:
                if (playerSystem != null)
                {
                    saveSystem.SavePlayerFromCarSystem(playerSystem);
                }
                break;
        }
    }
}