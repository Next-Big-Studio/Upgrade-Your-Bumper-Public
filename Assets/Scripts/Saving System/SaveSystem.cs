using System;
using Car;
using UnityEngine;

namespace Saving_System
{
    public class SaveSystem : MonoBehaviour
    {
        private const string SaveDataPath = "/saves/";
        private const string ExpiredSaveDataPath = "/expired_saves/";
        private const bool PrettyPrint = true;
        private const string LatestSave = "latest.json";

        public static SaveSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // =================================================================================================================================================
        // =========================================================== DATA ================================================================================
        // =================================================================================================================================================
    
        [SerializeField] public GameData gameData = new();
    
        // Check latest save
        public bool CheckLatestSave()
        {
            return LoadSave(LatestSave);
        }
    
        // Create a new game
        private void CreateNewGame()
        {
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + SaveDataPath);
            gameData = new GameData();
            SaveGame();
        }
    
        // Save the game's data
        private void SaveGame()
        {
            string json = JsonUtility.ToJson(gameData, PrettyPrint);
            System.IO.File.WriteAllText(Application.persistentDataPath + SaveDataPath + LatestSave, json);
        }
    
        // Load the game's data
        // Returns true if the save was created and false if it was loaded
        private bool LoadSave(string inSaveName)
        {
            string path = Application.persistentDataPath + SaveDataPath + inSaveName;
        
            // check if the directory exists
            if (!System.IO.Directory.Exists(Application.persistentDataPath + SaveDataPath))
            {
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + SaveDataPath);
                CreateNewGame();
                return true;
            }
        
            // Check if the file exists
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);

                // Check if the file is empty
                if (string.IsNullOrEmpty(json))
                {
                    CreateNewGame();
                    return true;
                }

                // Try to load the data
                try
                {
                    gameData = JsonUtility.FromJson<GameData>(json);
                    
                    if (gameData == null)
                    {
                        Debug.LogError("Error loading game data: game is null");
                        CreateNewGame();
                        return true;
                    }

                    if (gameData.isFinished)
                    {
                        Debug.LogError("Error loading game data: game is finished");
                        ExpireSave();
                        CreateNewGame();
                        return true;
                    }
                }
                // If there is an error loading the data, create a new one
                catch (Exception e)
                {
                    Debug.LogError("Error loading game data: " + e);
                    Debug.LogError("Creating new game data");
                    CreateNewGame();
                    return true;
                }
                return false;
            }

            // If the file doesn't exist, create a new one
            CreateNewGame();
            return true;
        }
    
        public void ExpireSave()
        {
            string path = Application.persistentDataPath + SaveDataPath + LatestSave;
            if (System.IO.File.Exists(path))
            {
                // Set the game as finished
                gameData.isFinished = true;
                
                // Save the game
                SaveGame();
                
                if (!System.IO.Directory.Exists(Application.persistentDataPath + ExpiredSaveDataPath))
                {
                    System.IO.Directory.CreateDirectory(Application.persistentDataPath + ExpiredSaveDataPath);
                }
                
                System.IO.File.Move(path, Application.persistentDataPath + ExpiredSaveDataPath + $"game_{gameData.gameID}.json");
            } else {
                Debug.LogError("No save to expire");
            }
        }
        
        public void WipeSave()
        {
            ExpireSave();
            
            CreateNewGame();
        }
        
        // =================================================================================================================================================
        // ======================================================== PLAYER METHODS =========================================================================
        // =================================================================================================================================================
    
        public void SavePlayerFromCarSystem(CarSystem inCarSystem)
        {
            gameData.playerData.carSystemData = new CarSystemData(inCarSystem);
            SaveGame();
        }
    
        // =================================================================================================================================================
        // ======================================================== LEADERBOARD METHODS ====================================================================
        // =================================================================================================================================================
    
        public void SaveLeaderboardData(LeaderboardData inLeaderboardData)
        {
            gameData.allRaces.Add(inLeaderboardData);
            SaveGame();
        }
    }
}