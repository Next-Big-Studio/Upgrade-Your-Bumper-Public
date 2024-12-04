using System;
using System.Collections.Generic;
using Car;
using Misc;
using Saving_System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Upgrades;
using Weapons;
using Random = UnityEngine.Random;

namespace UI.Cutscenes
{
    public class GameOverManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI winloseText;
        [SerializeField] private TextMeshProUGUI statsText;
        
        private List<Upgrade> _allUpgrades;
        private List<WeaponInfo> _allWeapons;
        
        private CarSystemData _playerSystemData;
        private CarSystem _playerSystem;
        private GameObject _playerCar;
        
        public void Initialize(List<Upgrade> upgrades, List<WeaponInfo> weapons, GameObject playerPrefab)
        {
            _allUpgrades = upgrades;
            _allWeapons = weapons;
        
            _playerSystemData = SaveSystem.Instance.gameData.playerData.carSystemData; // Get the player's car system data

            // Still spawn the player's car in the original spawn point
            GameObject spawnPoint = GameObject.Find("Spawn");
            _playerCar = Instantiate(playerPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation); // Spawn the player's car
            PlayerSetup(); // Set up the player's car
        
            SetValues(); // Set the values of the game over screen
        }

        private void PlayerSetup()
        {
            _playerSystem = _playerCar.GetComponent<CarSystem>(); // Get the player's car system

            // Get all lights on the player's car
            foreach (Light li in _playerCar.GetComponentsInChildren<Light>())
            {
                li.enabled = false; // Enable the lights
            }
            
            CarStats carStats = _playerSystem.carStats; // Get the base car stats

            // Disable some of the player's car system components
            _playerSystem.carPosition.enabled = false; // Disable the car position script
            _playerSystem.carMovement.enabled = false; // Disable the car movement script
            _playerSystem.carStatus.enabled = false; // Disable the car status script
                
            if (_playerSystemData.carStatusData.isDestroyed) // If the player's car is destroyed
            {
                _playerSystem.carStatus.isDestroyed = true; // Set the player's car to destroyed
                _playerSystem.carDestruction.enabled = true; // Enable the car destruction script
            }
            else
            {
                _playerSystem.carStatus.isDestroyed = false; // Set the player's car to not destroyed
                _playerSystem.carDestruction.enabled = false; // Disable the car destruction script
            }

            // Set the player's car system data
            CarInventory carInventory = _playerSystem.carInventory; // Get the player's car inventory
            // Get the player's car inventory
            carInventory.currency = _playerSystemData.carInventoryData.currency; // Set the player's currency
            _playerSystem.carName = _playerSystemData.carName; // Set the player's car name
            _playerSystem.carID = _playerSystemData.carID; // Set the player's car id

            // Set the player's upgrades and weapons
            carInventory.upgrades = _playerSystemData.carInventoryData.upgrades;
            carInventory.weapons = _playerSystemData.carInventoryData.weapons;
            
            carStats.ApplyVisualUpgrades(_playerSystemData.carInventoryData.upgrades.ToArray()); // Apply the visual upgrades to the player's car
            _playerSystem.MountWeapons(_allWeapons); // Mount the weapons to the player's car
            
            _playerSystem.carMovement.colliderRb.gameObject.AddComponent<Spin>(); // Add the spin script to the collider
        }

        private void SetValues()
        {
            // Only use the data from the save file
            
            GameData gd = SaveSystem.Instance.gameData;
        
            winloseText.text = gd.playerData.carSystemData.carStatusData.isDestroyed ? "You Lost!" : "You Won!";

            statsText.text = $"Final Currency Earned: {gd.playerData.carSystemData.carInventoryData.currency}\n" +
                             $"Number of races: {gd.playerData.numberOfRaces}\n" +
                             $"Number of kills: {gd.playerData.numberOfKills}\n" +
                             $"Number of times survived: {gd.playerData.timesSurvived}\n" +
                             $"Number of times destroyed: {gd.playerData.timesDestroyed}\n" +
                             "\n" +
                             $"{gd.playerData.carSystemData.carInventoryData.upgrades.Count} upgrades bought:\n";
        
            foreach (UpgradeData upgrade in gd.playerData.carSystemData.carInventoryData.upgrades) 
                statsText.text += $"{_allUpgrades[upgrade.id].name}\n";

            statsText.text += "\n";
        
            statsText.text += "\n" +
                              $"{gd.playerData.carSystemData.carInventoryData.weapons.Count} weapons bought:\n";
        
            foreach (WeaponData weapon in gd.playerData.carSystemData.carInventoryData.weapons)
                statsText.text += $"{_allWeapons[weapon.id].name}\n";

            statsText.text += "\n\n";

            statsText.text += $"Game ID: {gd.gameID}\n" +
                              $"Game Started on: {gd.dateStarted}\n";
        }
    }
}
