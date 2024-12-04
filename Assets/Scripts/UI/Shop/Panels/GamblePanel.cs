using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Car;
using Saving_System;
using Shop;
using TMPro;
using UI.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Upgrades;
using Weapons;
using Random = UnityEngine.Random;

namespace UI.Shop.Panels
{
    public class GamblePanel : UIExtendable
    {
        // Variables
        private int _gambleCost;
        
        // Inspector
        [SerializeField] private Button gambleButton;
        private TextMeshProUGUI _gambleButtonText;
        [SerializeField] private TextMeshPro gambleResultText;
        [SerializeField] private Sprite moneySprite;
        
        // References
        private CarInventory _carInventory;
        private GambleData _gambleData;
        private SlotMachine _slotMachine;
        private List<Upgrade> _allUpgrades;
        private List<WeaponInfo> _allWeapons;
        private IList<SlotPrize> _slotPrizes;
        private SlotPrize _finalPrize;

        public UnityEvent<SlotPrize> gambleWon;

        private int _difficulty;
        private bool _isSpinning;
        
        // Constructor
        private GamblePanel()
        {
            panelName = "gamble";
        }
        
        private new void Awake()
        {
            base.Awake();
            
            EInfo = new ExtendableInfo()
            {
                OnX = true,
                OnY = false,
                OldWidth = 500,
                NewWidth = Screen.width,
                OldHeight = panelHolder.sizeDelta.y,
                NewHeight = panelHolder.sizeDelta.y,
                Duration = 0.5f
            };
            
            _finalPrize = null;
            _gambleButtonText = gambleButton.GetComponentInChildren<TextMeshProUGUI>();
            
            Hide();
        }
        
        public void InitializePanel(CarInventory carInventory, List<Upgrade> allUpgrades, List<WeaponInfo> allWeapons)
        {
            _carInventory = carInventory;
            _gambleData = SaveSystem.Instance.gameData.playerData.gambleData;
            _allUpgrades = allUpgrades;
            _allWeapons = allWeapons;
            _slotMachine = FindAnyObjectByType<SlotMachine>();

            _difficulty = SaveSystem.Instance.gameData.difficulty;
            
            RandomizeGambleCost();
            InitializePrizes();
            CheckGambleButton();

            if (_slotPrizes.Count % 2 != 0)
            {
                // Add a duplicate of a random prize to make the count even
                _slotPrizes.Add(_slotPrizes[Random.Range(0, _slotPrizes.Count)]);
            }
            
            _slotMachine.InitializeSlotPrizes(_slotPrizes, moneySprite);
            
            gambleButton.onClick.AddListener(Gamble);
        }
        
        // =================================================================================================================================================
        // ============================================================= UI FUNCTIONS ======================================================================
        // =================================================================================================================================================

        private void Gamble()
        {
            if (_isSpinning) { return; }
            
            if (_carInventory.currency < _gambleCost)
            {
                gambleResultText.text = "You don't have enough money to gamble!";
                return;
            }

            _carInventory.currency -= _gambleCost;
            _isSpinning = true;
            _slotMachine.StartSpin();
            
            ShopManager.instance.RefreshCurrencyUI();
            
            // Check the result of the gamble
            StartCoroutine(WaitForGamble());
        }
        
        private IEnumerator WaitForGamble()
        {
            while (_slotMachine.isSpinning)
            {
                yield return null;
            }
            
            gambleResultText.text = CheckGambleResult();
                
            _gambleData.amountGambled += _gambleCost;
            _gambleData.timesGambled++;
            _isSpinning = false;
                
            RandomizeGambleCost();
            CheckGambleButton();
        }
        
        private void CheckGambleButton()
        {
            // Set color of button to gray if the player doesn't have enough money
            ColorBlock colors = gambleButton.colors;
            colors.normalColor = _carInventory.currency < _gambleCost ? Color.gray : Color.white;
            gambleButton.colors = colors;
            
            // Set the text of the button to show the cost of gambling
            _gambleButtonText.text = $"Gamble (${_gambleCost})";
        }

        protected override void Refresh()
        {
            CheckGambleButton();
        }

        // =================================================================================================================================================
        // ============================================================= EVENT FUNCTIONS ===================================================================
        // =================================================================================================================================================
        
        private new void OnRectTransformDimensionsChange()
        {
            EInfo.NewWidth = Screen.width;
            EInfo.NewHeight = Screen.height;
            EInfo.OldHeight = Screen.height;
        }
        
        // =================================================================================================================================================
        // ============================================================= HELPER FUNCTIONS ==================================================================
        // =================================================================================================================================================
        // Initialize the prizes that can be won from the slot machine
        private void InitializePrizes()
        {
            _slotPrizes = new List<SlotPrize>();
            foreach (SlotPrizeUpgrade slotPrize in _allUpgrades.Select(SlotPrizeUpgrade.CreateUpgradePrize))
            {
                // The higher the weight, the more likely the prize will be won
                for (int j = 0; j < slotPrize.Weight; j++)
                {
                    _slotPrizes.Add(slotPrize);
                }
            }
            
            foreach (SlotPrizeWeapon slotPrize in _allWeapons.Select(SlotPrizeWeapon.CreateWeaponPrize))
            {
                // The higher the weight, the more likely the prize will be won
                for (int j = 0; j < slotPrize.Weight; j++)
                {
                    _slotPrizes.Add(slotPrize);
                }
            }
            
            int moneyPrizeAmount = 500 / _difficulty; // THIS CONTROLS THE AMOUNT OF MONEY THAT CAN BE WON
            int moneyPrizeCount = Random.Range(1, 4); // THIS CONTROLS THE AMOUNT OF MONEY PRIZES THAT CAN BE WON
            int moneyPrizeWeight = moneyPrizeCount; // THIS CONTROLS THE WEIGHT OF THE MONEY PRIZES, THE HIGHER THE WEIGHT, THE MORE LIKELY THE PRIZE WILL BE WON
            for (int i = 0; i < moneyPrizeCount; i++)
            {
                _slotPrizes.Add(SlotPrizeMoney.CreateMoneyPrize("Money", moneySprite, moneyPrizeAmount*i, moneyPrizeWeight));
                moneyPrizeWeight--;
            }
        }
        
        // Randomize the cost of gambling
        private void RandomizeGambleCost()
        {
            _gambleData ??= new GambleData();
            _gambleCost = Random.Range(100, 250) * _difficulty + (100 * _gambleData.timesGambled) + (100 * _gambleData.timesWon); // THIS CONTROLS THE GAMBLE COST
        }
        
        // Check the result of the gamble
        private string CheckGambleResult()
        {
            _finalPrize = _slotMachine.GetPrizes();
            if (_finalPrize != null)
            {
                string result = _finalPrize.ApplyPrize(_carInventory, _gambleData);
                gambleWon.Invoke(_finalPrize);
                _gambleData.timesWon++;

                return result;
            }
            
            _gambleData.amountLost += _gambleCost;
            _gambleData.timesLost++;
            return "You lost!";
        }
    }
}