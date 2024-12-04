using System;
using System.Collections.Generic;
using UnityEngine;
using Car;
using Misc;
using Saving_System;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UIElements;
using Upgrades;
using Weapons;
using Random = UnityEngine.Random;

public class PodiumManager : MonoBehaviour
{
    [Header("References")]
    public Transform firstPlacePosition;  // Position for the first place car on the podium
    public Transform secondPlacePosition; // Position for the second place car on the podium
    public Transform thirdPlacePosition;  // Position for the third place car on the podium
    
    public GameObject chipsReference;
    public GameObject chipsPrefab;
    
    public TextMeshPro firstPlaceText; // Text for the first place car
    public TextMeshPro secondPlaceText; // Text for the second place car
    public TextMeshPro thirdPlaceText; // Text for the third place car
    public TextMeshPro playerCurrencyText; // Text for the player's currency
    
    [Header("End Screen")]
    public GameObject endScreen; // End screen object
    public UnityEngine.UI.Image endScreenImage;
    public GameObject endScreenText;
    public TextMeshProUGUI endScreenTitleTMP;
    public TextMeshProUGUI endScreenTextTMP;
    
    [Header("Camera")] 
    public Camera mainCamera; // Main camera
    public PostProcessProfile postProcessProfile; // Post-processing profile for the camera
    
    private LeaderboardData _leaderboard; // Leaderboard data
    private List<CarInfo> _allCarInfos; // List of all car infos in
    private List<Upgrade> _allUpgrades; // List of all upgrades in the game
    private List<WeaponInfo> _allWeapons; // List of all weapons in the game
    
    private GameObject[] _podiumCars;      // Array to store the cars on the podium
    
    public UnityEvent onExit; // Event called when the podium sequence is finished
    [SerializeField] public GameObject exitButton;
    
    private bool _startEffects; // Flag to start the zoom out effect
    private float _lensDistortionIntensity = -55f; // Intensity of the lens distortion effect
    
    public void SetupPodium(LeaderboardData inLeaderboard, List<CarInfo> allCarInfos, List<Upgrade> allUpgrades, List<WeaponInfo> allWeapons)
    {
        // Initialize the leaderboard based on the current race positions
        _leaderboard = inLeaderboard;
        
        // Update and sort the leaderboard positions
        _allCarInfos = allCarInfos;
        _allUpgrades = allUpgrades;
        _allWeapons = allWeapons;
        
        PlaceCarsOnPodium();
        SetupChips();

        // Enable the postprocessing layer on the camera
        mainCamera.GetComponent<PostProcessLayer>().enabled = true;
        
        // Set the text for the end screen

        if (SaveSystem.Instance.gameData.playerData.didFinish)
        {
            endScreenTitleTMP.text = "You finished in " + ordinal(_leaderboard.cars.FindIndex(c => c.carName == "Max V.") + 1) + " place!";
        }
        else
        {
            endScreenTitleTMP.text = "You weren't able to finish the race... maybe next time!";
        }
        endScreenTextTMP.text = "You earned $" + GameManager.Instance.playerGainedCurrency + "!";
        endScreenTextTMP.text += "\n\n" + "Bounty: " + (GameManager.Instance.BountyWon ? "Won" : "Lost");
        endScreenTextTMP.text += "\n\n" + "Total Kills: " + SaveSystem.Instance.gameData.playerData.raceKills;
        endScreenTextTMP.text += "\n\n" + "Race Time: " + SaveSystem.Instance.gameData.playerData.raceTime.ToString("F2") + "s";
        
        // Start the effects
        _startEffects = true;
    }

    private void PlaceCarsOnPodium()
    {
        GameObject firstPlaceCar = Instantiate(_allCarInfos[_leaderboard.cars[0].carID].carPrefab, firstPlacePosition.position, firstPlacePosition.rotation); // Instantiate the first place car at the first place position
        GameObject secondPlaceCar = Instantiate(_allCarInfos[_leaderboard.cars[1].carID].carPrefab, secondPlacePosition.position, secondPlacePosition.rotation); // Instantiate the second place car at the second place position
        GameObject thirdPlaceCar = Instantiate(_allCarInfos[_leaderboard.cars[2].carID].carPrefab, thirdPlacePosition.position, thirdPlacePosition.rotation); // Instantiate the third place car at the third place position
        
        // Store the cars on the podium in an array
        _podiumCars = new[] { firstPlaceCar, secondPlaceCar, thirdPlaceCar };
        
        // Set the currency text
        playerCurrencyText.text = "Your Currency\n$" + (_leaderboard.cars.Find(c => c.carName == "Max V.")?.carInventoryData.currency.ToString() ?? "0");
        firstPlaceText.text = _leaderboard.cars[0].carName.Split(" ")[0] + "\n$" + GameManager.Instance.LeaderboardAmounts[0];
        secondPlaceText.text = _leaderboard.cars[1].carName.Split(" ")[0] + "\n$" + GameManager.Instance.LeaderboardAmounts[1];
        thirdPlaceText.text = _leaderboard.cars[2].carName.Split(" ")[0] + "\n$" + GameManager.Instance.LeaderboardAmounts[2];
        
        // Disable the car systems of the cars on the podium
        for (int i = 0; i < 3; i++)
        {
            CarSystem carSystem = _podiumCars[i].GetComponent<CarSystem>();
            
            // Set upgrades and weapons
            carSystem.carInventory.upgrades = _leaderboard.cars[i].carInventoryData.upgrades;
            carSystem.carInventory.weapons = _leaderboard.cars[i].carInventoryData.weapons;
            
            // Set the car's stats
            carSystem.carStats.ApplyUpgrades(_allUpgrades.FindAll(u => carSystem.carInventory.weapons.Exists(up => up.id == u.id)).ToArray());
            carSystem.carInventory.SetWeapons(_allWeapons.FindAll(w => carSystem.carInventory.weapons.Exists(we => we.id == w.id)).ToArray());
            
            // Mount the weapons to the car
            carSystem.MountWeapons(_allWeapons);
            
            // Set the car's position
            carSystem.carPosition.currentPosition = i;
            
            // Add the spin script to the collider
            carSystem.carMovement.colliderRb.gameObject.AddComponent<Spin>(); 
        }
    }

    private void SetupChips()
    {
        var chips = new List<Chip>
        {
            new(100, Color.magenta, Color.white),
            new(25, new Color(0, 0.5f, 0), Color.white),
            new(10, Color.blue, Color.white),
            new(5, Color.red, Color.white),
            new(1, Color.white, Color.black)
        };
        
        int playerCurrency = _leaderboard.cars.Find(c => c.carName == "Max V.")?.carInventoryData.currency ?? 0;
        var chipAmounts = new List<int>();
        
        CalculateChipAmounts();
        
        for (int i = 0; i < chipAmounts.Count; i++)
        {
            Chip chip = chips[i];
            for (int j = 0; j < chipAmounts[i]; j++)
            {
                GameObject chipObject = Instantiate(chipsPrefab, chipsReference.gameObject.transform.GetChild(i).transform);
                chipObject.transform.localPosition = new Vector3(0, 0, 0);
                
                // calculate prefab height
                float chipHeight = chipObject.GetComponent<MeshRenderer>().bounds.size.y;
                chipObject.transform.localPosition += new Vector3(0, chipHeight * j, 0);
                
                // Rotate slightly for a more realistic look
                chipObject.transform.Rotate(new Vector3(0, Random.Range(-180, 180), 0));
                
                // Set the chip color based on the chip type
                // Grabs material and sets colors for the Toon shader
                Material material = chipObject.GetComponent<MeshRenderer>().materials[0];
                material.SetColor("_BaseColor", chip.Color);
                material.SetColor("_1st_ShadeColor", Darken(chip.Color, 0.5f));
                material.SetColor("_2nd_ShadeColor", Darken(chip.Color, 0.75f));
                
                Material material2 = chipObject.GetComponent<MeshRenderer>().materials[0];
                material2.SetColor("_BaseColor", chip.AlternateColor);
                material2.SetColor("_1st_ShadeColor", Darken(chip.AlternateColor, 0.5f));
                material2.SetColor("_2nd_ShadeColor", Darken(chip.AlternateColor, 0.75f));
            }
        }
        
        return;

        void CalculateChipAmounts()
        {
            int remainingCurrency = playerCurrency;
            
            foreach (Chip chip in chips)
            {
                int chipAmount = remainingCurrency / chip.Value;
                chipAmounts.Add(chipAmount);
                remainingCurrency -= chipAmount * chip.Value;
            }
        }
        
        Color Darken(Color color, float ratio)
        {
            return new Color(
                color.r * ratio,
                color.g * ratio,
                color.b * ratio
            );
        }
    }
    
    private void Update()
    {
        if (!_startEffects) return;
        
        // Update the lens distortion effect
        _lensDistortionIntensity = Mathf.Lerp(_lensDistortionIntensity, 70f, Time.deltaTime);
        postProcessProfile.GetSetting<LensDistortion>().intensity.value = _lensDistortionIntensity;
        
        // Check if the lens distortion effect is close to its final intensity
        if (!(Mathf.Abs(_lensDistortionIntensity - 70f) < 0.1f)) return;
        
        exitButton.SetActive(true);
        endScreen.SetActive(true);

        Color color = endScreenImage.color;
        color.a += Mathf.Lerp(0, 1, Time.deltaTime);
        endScreenImage.color = color;
        
        if(!(color.a >= 1)) return;
        
        endScreenText.SetActive(true);
    }

    public void ExitScene()
    {
        onExit.Invoke();
    }
    
    public string ordinal(int num)
    {
        if( num <= 0 ) return num.ToString();

        return (num % 100) switch
        {
            11 or 12 or 13 => num + "th",
            _ => (num % 10) switch
            {
                1 => num + "st",
                2 => num + "nd",
                3 => num + "rd",
                _ => num + "th"
            }
        };
    }
}

public struct Chip
{
    public readonly int Value;
    public Color Color;
    public Color AlternateColor;
    
    public Chip(int value, Color color, Color alternateColor)
    {
        Value = value;
        Color = color;
        AlternateColor = alternateColor;
    }
}


