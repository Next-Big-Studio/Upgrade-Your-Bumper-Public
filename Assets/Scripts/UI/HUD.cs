using UnityEngine;
using Car;
using Destructors;
using System.Collections;
using Saving_System;
using Upgrades;

namespace UI
{

    public class HUD : MonoBehaviour
    {
        [Header("Player Scripts")]
        public CarSystem playerSystem;
        private CarMovement _playerMovement;
        private CarDestruction _playerDestructor;
        private CarPosition _playerPosition;
    
        [Header("Car Gauges")]
        public CarGauges carGauges;

        [Header("Lap Text")]
        public LapText lapText;

        [Header("Elapsed Time")]
        public ElapsedTime elapsedTime;

        [Header("Countdown Text")]
        public CountdownText countdownText;

        [Header("Leaderboard")]
        public HUDLeaderboard leaderboard;
        
        [Header("Upgrade Text")]
        public UpgradeText upgradeText;

        private bool hasInitialized = false;

        private void Start()
        {
            StartCoroutine(InitializeHUD());
        }

        private void Update()
        {
            if (!hasInitialized)
            {
                return;
            }
            if (carGauges != null)
            {
                carGauges.UpdateGauges();
            }
            if (lapText != null)
            {
                lapText.UpdateLapText();
            }
            if (elapsedTime != null)
            {
                elapsedTime.UpdateTime();
            }
            if (countdownText != null)
            {
                countdownText.UpdateCountdown();
            }
            if (leaderboard != null)
            {
                leaderboard.UpdateLeaderboard();
            }
        }
    
        public void SetPlayerCar(CarSystem inPlayerSystem)
        {
            playerSystem = inPlayerSystem;
            _playerMovement = playerSystem.carMovement;
            _playerDestructor = playerSystem.carDestruction;
            _playerPosition = playerSystem.carPosition;
        }
        
        public void ShowMysteryBox(Upgrade inUpgradeData)
        {
            upgradeText.ShowUpgrade(inUpgradeData);
        }
        
        // public void BossFight()
        // {
        //     // Gotta fix this
        //     speedNeedleTransform.gameObject.SetActive(true);
        //     RPMNeedleTransform.gameObject.SetActive(true);
        //     fuelNeedleTransform.gameObject.SetActive(true);
        //     timerText.gameObject.SetActive(true);
        //     lapText.gameObject.SetActive(false);
        //     lowHealthText.SetActive(true);
        //     countdownText.gameObject.SetActive(true);
        //     first.gameObject.SetActive(false);
        //     second.gameObject.SetActive(false);
        //     third.gameObject.SetActive(false);
        //     fourth.gameObject.SetActive(false);

        public void StartEnding(float inTime)
        {
            countdownText.Ending(inTime);
        }
        
        public void StartStarting(float inTime)
        {
            countdownText.Starting(inTime);
        }

        private IEnumerator InitializeHUD()
        {
            while (playerSystem == null)
            {
                yield return null;
            }
            if (carGauges != null)
            {
                carGauges.Initialize(playerSystem);
            }
            if (lapText != null)
            {
                lapText.Initialize(playerSystem);
            }
            hasInitialized = true;
        }
    }
}
