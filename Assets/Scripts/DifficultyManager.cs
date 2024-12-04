using System.Collections.Generic;
using Car;
using Saving_System;
using UnityEngine;
using Upgrades;
using Weapons;
using Random = UnityEngine.Random;

public class DifficultyManager : MonoBehaviour
{
    private static DifficultyManager _instance;
    
    // The list of all cars
    private List<CarInfo> _allCars;

    // The list of all upgrades, weapons, and abilities
    private List<Upgrade> _allUpgrades;
    private List<WeaponInfo> _allWeapons;
    
    // The amount of upgrades to give
    private const int MinUpgradesAmount = 1;
    private const int MaxUpgradesAmount = 3;
    
    // The amount of weapons to give
    private const int MinWeaponsAmount = 1;
    private const int MaxWeaponsAmount = 3;

    // For reference:
    // Amount = amount of that upgrade/weapon
    // Count = the number of upgrades/weapons
    
    // The difficulty of the game
    // This is used to scale the amount of upgrades and weapons given not the count of upgrades and weapons
    private int _difficulty;
    
    // The number of times the player has completed races
    // This is used to scale the amount of upgrades and weapons given not the count of upgrades and weapons
    public int raceCount;

    public void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetVariables(List<CarInfo> inCars, List<Upgrade> inUpgrades, List<WeaponInfo> inWeapons, int inDifficulty = 1)
    {
        _allCars = inCars;
        _allUpgrades = inUpgrades;
        _allWeapons = inWeapons;
        _difficulty = inDifficulty;
    }
    
    public List<UpgradeData> GetRandomUpgrades()
    {
        var upgrades = new List<UpgradeData>();
        int amount = Random.Range(0, raceCount);
        
        if (_allUpgrades.Count < amount)
        {
            amount = _allUpgrades.Count;
        }
        
        for (int i = 0; i < amount; i++)
        {
            Upgrade randomUpgrade = _allUpgrades[Random.Range(0, _allUpgrades.Count)];
            UpgradeData upgrade = new(randomUpgrade)
            {
                amount = Random.Range(MinUpgradesAmount, MaxUpgradesAmount) * _difficulty
            };
            
            if (upgrades.Contains(upgrade))
            {
                i--;
                continue;
            }
            
            upgrades.Add(upgrade);
        }

        return upgrades;
    }
    
    public List<WeaponData> GetRandomWeapons()
    {
        var weapons = new List<WeaponData>();
        int amount = Random.Range(0, raceCount);
        
        if (_allWeapons.Count < amount)
        {
            amount = _allWeapons.Count;
        }
        
        for (int i = 0; i < amount; i++)
        {
            WeaponInfo randomWeapon = _allWeapons[Random.Range(0, _allWeapons.Count)];
            WeaponData weapon = new(randomWeapon)
            {
                amount = Random.Range(MinWeaponsAmount, MaxWeaponsAmount) * _difficulty
            };
            
            if (weapons.Contains(weapon))
            {
                i--;
                continue;
            }
            
            weapons.Add(weapon);
        }

        return weapons;
    }
    
    public CarInfo GetRandomCar()
    {
        if (_allCars.Count == 0)
        {
            return null;
        }
        
        return _allCars.Count == 1 ? _allCars[0] : _allCars[Random.Range(0, _allCars.Count)];
    }
}
