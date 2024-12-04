using System;
using System.Collections.Generic;
using Car;
using Misc;
using Saving_System;
using Shop;
using TMPro;
using UI.Framework;
using UI.Shop.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Upgrades;
using Weapons;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    // -=-=-=-=-= Player Objects =-=-=-=-=-
    private GameObject _playerCar; // Reference to the player's car and car system
    public CarSystem playerSystem; // Keep this public for now
    public CarInventory carInventory; // The player's car inventory

    // Reference to where the player is going to spawn in the shop
    public GameObject spawnPoint;
    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
    
    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

    // -=-=-=-=-= UI OBJECTS =-=-=-=-=-
    // TAB BUTTONS
    [Header("Shop Tabs")]
    public GameObject tabsParent;

    public List<UITab> _tabs;
    public string selectedTab;

    // PANELS
    [Header("Shop Panels")] 
    [SerializeField] public GameObject panelsParent;
    [SerializeField] public RepairPanel repairPanel;
    [SerializeField] public ShopPanel shopPanel;
    [SerializeField] public InventoryPanel inventoryPanel;
    [SerializeField] public StatsPanel statsPanel;
    [SerializeField] public GamblePanel gamblePanel;
    public GameObject jobPanel;

    // MISC UI
    [Header("Misc UI")]
    public GameObject currencyUI;
    public GameObject popupUI;
    public GameObject exitButton;

    [Header("UI Audio")]
    public AudioSource sfxSource;
    public AudioClip hoverSound;
    public AudioClip successSound;
    public AudioClip failureSound;
    public AudioClip moneySound;
    public AudioClip penClick;
    
    [Header("Radio UI")]
    public RadioManager radioManager;
    public GameObject radioObject;
    private Vector3 _visiblePosition = new(-181, -438, 0);
    private Vector3 _hiddenPosition = new(-181, -642, 0);
    private bool _isRadioVisible;
    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

    // -=-=-=-=-= EVENTS =-=-=-=-=-
    // EXIT EVENT
    public UnityEvent onExit;
    private bool _pressedExit;
    
    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

    // For categorizing audio
    private readonly ShopError[] _failErrors =
    {
        ShopError.CannotBuy, 
        ShopError.CannotSell, 
        ShopError.NotEnoughSpace, 
        ShopError.MaxAmountReached, 
        ShopError.CannotRepair, 
        ShopError.CannotRefresh, 
        ShopError.NotEnoughMoney
    };
    
    public List<Upgrade> allUpgrades; // All upgrades in the game
    public List<WeaponInfo> allWeapons; // All weapons in the game
    
    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

    // ====================================================================================================
    // ========================================== START FUNCTIONS =========================================
    // ====================================================================================================

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    
    // Initialize the shop with upgrades and weapons
    public void InitializeShop(List<Upgrade> upgrades, List<WeaponInfo> weapons)
    {
        allUpgrades = upgrades;
        allWeapons = weapons;
        
        // Enable all panels, we will hide them later.
        // We do this so we can call the Awake function in the panels
        foreach (Transform child in panelsParent.transform)
        {
            child.gameObject.SetActive(true);
        }
        
        // Get all the tabs
        _tabs = new List<UITab>();
        foreach (Transform child in tabsParent.transform)
        {
            UITab tab = child.GetComponent<UITab>();
            tab.tabName = tab.panel.panelName;
            _tabs.Add(tab);
            
            // Hide panel now
            tab.panel.Hide();
        }
        
        // Add listeners to the tabs
        foreach (UITab tab in _tabs)
        {
            tab.tabButton.onClick.AddListener(() => SelectTab(tab.panel.panelName));
        }
        
    }

    // Initialize the player in the shop
    public void InitializePlayer(GameData inData, GameObject playerPrefab)
    {
        CarSystemData playerSystemData = inData.playerData.carSystemData; // Get the player's car system data
        spawnPoint = GameObject.Find("Spawn"); // Find the spawn point
        _playerCar = Instantiate(playerPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation); // Spawn the player's car
        playerSystem = _playerCar.GetComponent<CarSystem>(); // Get the player's car system
        playerSystem.carStatus.isDestroyed = playerSystemData.carStatusData.isDestroyed; // Set the player's car status
        
        // Get all lights on the player's car
        foreach (Light li in _playerCar.GetComponentsInChildren<Light>())
        {
            li.enabled = false; // Enable the lights
        }
        
        // We do this because the stats panel needs the base car stats
        // This is later updated in the InitializeStatsPanel function at the end
        CarStats baseCarStats = playerSystem.carStats; // Get the base car stats
        CarInventory baseCarInventory = playerSystem.carInventory; // Get the player's car inventory
        
        PlayerSetup();
        
        inventoryPanel.SetInventory(carInventory); // Set the player's inventory in the inventory panel
        shopPanel.SetInventory(carInventory); // Set the player's inventory in the shop panel
        statsPanel.InitializeStatsPanel(baseCarStats, baseCarInventory, allUpgrades, allWeapons); // Initialize the stats panel
        gamblePanel.InitializePanel(carInventory, allUpgrades, allWeapons); // Initialize the gamble panel
        gamblePanel.gambleWon.AddListener(OnGambleWon); // Add the gamble won listener
        
        // Apply the player's upgrades
        foreach (UpgradeData upgrade in carInventory.upgrades)
        {
            Upgrade constUpgrade = allUpgrades.Find(u => u.id == upgrade.id); // get the fresh upgrade from the list
            for (int i = 0; i < upgrade.amount; i++) playerSystem.carStats.ApplyUpgrade(constUpgrade); // apply the upgrade to the player's car
            
            // Create a new upgrade object in the player's inventory UI
           inventoryPanel.AddUpgrade(upgrade, constUpgrade);
        }

        // Initialize the player's weapons
        foreach (WeaponData weapon in carInventory.weapons)
        {
            WeaponInfo constWeapon = allWeapons.Find(w => w.id == weapon.id); // get the fresh weapon from the list
            
            inventoryPanel.AddWeapon(weapon, constWeapon); // Create a new weapon object in the player's inventory UI
        }
        
        // Load random upgrades and weapons
        shopPanel.GetRandomUpgrades();
        shopPanel.GetRandomWeapons();
        
        // Check if the player's car is destroyed
        if (playerSystem.carStatus.isDestroyed)
        {
            // Set the cost to repair the player
            int repairCost = inData.difficulty * 250; // Set the repair cost based on the difficulty
            repairPanel.SetRepairCost(repairCost); // Set the repair cost in the repair panel
            
            // Hide the shop tab
            _tabs.Find(t => t.tabName == "shop").gameObject.SetActive(false); // Disable the shop tab
            _tabs.Find(t => t.tabName == "repair").gameObject.SetActive(true); // Enable the repair tab

            // Hide the refresh button
            shopPanel.refreshUI.SetActive(false); // Disable the refresh button

            // Select the repair tab
            selectedTab = "repair";
            SelectTab("repair"); // Select the repair tab

            // Set the exit button
            exitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Give Up"; // Change the exit button text

            // Show a popup message
            PopupMessage(ShopError.Destroyed, timer: 5f); // Show a popup message
            
            repairPanel.onRepair.AddListener(OnPlayerRepaired);
        }
        else
        {
            // Show the shop tab
            _tabs.Find(t => t.tabName == "shop").gameObject.SetActive(true); // Enable the shop tab
            _tabs.Find(t => t.tabName == "repair").gameObject.SetActive(false); // Disable the repair tab
            
            // Select the shop tab
            selectedTab = "shop";
            SelectTab("shop"); // Select the shop tab
        }

        // Update the UI
        RefreshCurrencyUI();
        return;

        void PlayerSetup()
        {
            // Disable some of the player's car system components
            playerSystem.carPosition.enabled = false; // Disable the car position script
            playerSystem.carMovement.enabled = false; // Disable the car movement script
            playerSystem.carStatus.enabled = false; // Disable the car status script

            // Set the player's car system data
            carInventory = playerSystem.carInventory; // Get the player's car inventory
            carInventory.currency = playerSystemData.carInventoryData.currency; // Set the player's currency
            playerSystem.carName = playerSystemData.carName; // Set the player's car name
            playerSystem.carID = playerSystemData.carID; // Set the player's car id

            // Set the player's upgrades and weapons
            carInventory.upgrades = playerSystemData.carInventoryData.upgrades;
            carInventory.weapons = playerSystemData.carInventoryData.weapons;
            
            playerSystem.carMovement.colliderRb.gameObject.AddComponent<Spin>(); // Add the spin script to the collider
        }
    }

    public void FinalizeShop()
    {
        // Show a popup message
        PopupMessage(ShopError.Welcome, timer: 5f); // Show a popup message
                
        // Lock everything until the boss count is shown
        _tabs.ForEach(t => t.tabButton.interactable = false);
        exitButton.GetComponentInChildren<Button>().interactable = false;
        panelsParent.SetActive(false);
                
        // Wait a few seconds before showing the boss count
        Invoke(nameof(ShowBossCount), 5f);
    }

    public void ShowBossCount()
    {
        PopupMessage(ShopError.BossCount, "Carlos", 5f);
        
        // Unlock everything after the boss count is shown
        _tabs.ForEach(t => t.tabButton.interactable = true);
        exitButton.GetComponentInChildren<Button>().interactable = true;
        panelsParent.SetActive(true);
    }
    
    // ====================================================================================================
    // ===================================== REFRESH UI FUNCTIONS =========================================
    // ====================================================================================================

    // Refresh the currency UI
    public void RefreshCurrencyUI()
    {
        currencyUI.GetComponentInChildren<TextMeshProUGUI>().text = $"${carInventory.currency.ToString()}";
    }
    
    // ====================================================================================================
    // ========================================== MISC FUNCTIONS ==========================================
    // ====================================================================================================
    
    // Show a popup message
    public void PopupMessage(ShopError? shopError, string message = "", float timer = 3f)
    {
        // Get the message based on the error | What did copilot cook...
        string response = shopError switch
        {
            ShopError.Bought => $"Successfully bought {message}!",
            ShopError.CannotBuy => "You cannot buy this!", // I don't think this should ever happen because we have specific checks for this and disable the button
            ShopError.Sold => $"Successfully sold {message}!",
            ShopError.CannotSell => "You cannot sell this! You can only sell so many items before we start charging you!",
            ShopError.NotEnoughSpace => $"You don't have enough space to buy this! Try selling some {message}.",
            ShopError.MaxAmountReached => $"You have reached the maximum amount for this {message}!",
            ShopError.NotEnoughMoney => "You don't have enough money to buy this! Try selling some upgrades or weapons.",
            ShopError.CannotRepair => "You don't have enough money to repair! Try selling some upgrades or weapons.",
            ShopError.Repaired => "Your vehicle has been repaired!",
            ShopError.Refreshed => "Shop has been refreshed!",
            ShopError.CannotRefresh => "You cannot refresh the shop anymore!",
            ShopError.Destroyed => "Your vehicle is destroyed! You need to repair it before you can buy anything or continue the game. Try selling some upgrades or weapons... or gamble!",
            ShopError.Welcome => "Welcome to the shop, please listen closely! Feel free to browse around and buy some upgrades or weapons! You can also sell your upgrades and weapons if you need some extra cash! Gambling is also an option if you're feeling lucky!",
            ShopError.BossCount => $"WARNING: You have {GameManager.Instance.MaxRaces - SaveSystem.Instance.gameData.playerData.numberOfRaces} races left until you face Carlos! Try to get as many upgrades and weapons as you can before you face him!",
            _ => message
        };

        PlayWPopupAlert(shopError);

        // Set the text of the popup
        popupUI.GetComponentInChildren<TextMeshProUGUI>().text = response;

        // Show the popup
        popupUI.SetActive(true);

        // Hide the popup after 3 seconds
        Invoke(nameof(HidePopup), timer);
    }

    // Hide the popup
    private void HidePopup()
    {
        // Hide the popup
        popupUI.SetActive(false);
    }

    private void PlayWPopupAlert(ShopError? shopError)
    {
        if (shopError == null || Array.Exists(_failErrors, error => error == shopError))
            sfxSource.PlayOneShot(failureSound);
        else
            switch (shopError)
            {
                case ShopError.Bought:
                    sfxSource.PlayOneShot(successSound);
                    break;
                case ShopError.Sold:
                    sfxSource.PlayOneShot(moneySound);
                    break;
                case ShopError.Repaired:
                case ShopError.Refreshed:
                case ShopError.Destroyed:
                case ShopError.Welcome:
                case ShopError.BossCount:
                case ShopError.CannotBuy:
                case ShopError.CannotSell:
                case ShopError.NotEnoughSpace:
                case ShopError.MaxAmountReached:
                case ShopError.NotEnoughMoney:
                case ShopError.CannotRepair:
                case ShopError.CannotRefresh:
                    break;
                default:
                    Debug.Log($"{shopError} not found in PlayWPopAlert");
                    break;
            }
    }

    // Select a tab
    private void SelectTab(string tab)
    {
        _tabs.Find(t => t.panel.panelName == selectedTab).DeselectTab();
        _tabs.Find(t => t.panel.panelName == tab).SelectTab();
        
        selectedTab = tab;
    }
    
    // ====================================================================================================
    // ========================================= EVENT FUNCTIONS ==========================================
    // ====================================================================================================
    // Exit the shop
    public void OnExit()
    {
        if (playerSystem.carStatus.isDestroyed && !_pressedExit)
        {
            // ensure that the player wants to exit the shop
            const string message = "Your vehicle is still destroyed! Are you sure you want to leave? You can still try to repair your car by gambling or selling upgrades and weapons! If you leave now, you will lose all progress!";
            PopupMessage(null, message, 5f);
            _pressedExit = true;

            // change the exit button text
            exitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Are you sure?";

            return;
        }

        onExit.Invoke();
    }

    public void JobOfferToggle()
    {
        jobPanel.SetActive(!jobPanel.activeSelf);
    }

    public void StartJob()
    {
        if (carInventory.currency < 300) return;
        carInventory.currency -= 300;
        GameManager.Instance.saveSystem.SavePlayerFromCarSystem(playerSystem);
        jobPanel.transform.GetChild(1).gameObject.SetActive(true);
        Destroy(GameObject.Find("EventSystem"));
        Destroy(jobPanel.transform.GetChild(0).gameObject);
        AudioSource.PlayClipAtPoint(penClick, transform.position);
        Invoke("LoadIntoTimeTrial", 3);
    }

    void LoadIntoTimeTrial()
    {
        GameManager.Instance.LoadThisScene("TimeTrial");
    }
    
    // Repair the player
    private void OnPlayerRepaired()
    {
        _tabs.Find(t => t.panel.panelName == "repair").Hide(); // Hide the repair tab
        _tabs.Find(t => t.panel.panelName == "shop").Show(); // Show the shop tab
        
        // Select the shop tab
        SelectTab("shop");
        
        // Enable the refresh button
        shopPanel.refreshUI.SetActive(true); // Enable the refresh button
        
        // Update the exit button
        exitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue"; // Change the exit button text
        
        // Update the UI
        RefreshCurrencyUI(); // Update the currency UI
        
        // Lock everything until the boss count is shown
        _tabs.ForEach(t => t.tabButton.interactable = false);
        exitButton.GetComponentInChildren<Button>().interactable = false;
        panelsParent.SetActive(false);
        
        // Show the boss count message after a few seconds
        Invoke(nameof(ShowBossCount), 3f);
    }
    
    private void OnGambleWon(SlotPrize prize)
    {
        switch (prize)
        {
            // Add the prize to the inventory panel
            case SlotPrizeUpgrade upgradePrize:
            {
                UpgradeData upgradeData = carInventory.upgrades.Find(u => u.id == upgradePrize.Upgrade.id); // Get the upgrade data from the player's inventory
                inventoryPanel.AddUpgrade(upgradeData, upgradePrize.Upgrade); // Create a new upgrade object in the player's inventory UI
                // playerSystem.carStats.ApplyVisualUpgrades(new []{upgradeData}); // Apply the visual upgrade to the player's car
                break;
            }
            case SlotPrizeWeapon weaponPrize:
            {
                WeaponData weaponData = carInventory.weapons.Find(w => w.id == weaponPrize.Weapon.id); // Get the weapon data from the player's inventory
                inventoryPanel.AddWeapon(weaponData, weaponPrize.Weapon); // Create a new weapon object in the player's inventory UI
                // playerSystem.MountWeapons(allWeapons); // Mount the weapons to the player's car
                break;
            }
        }

        // Update the UI
        RefreshCurrencyUI(); // Update the currency UI
    }
}

public enum ShopError
{
    Bought,
    CannotBuy,
    Sold,
    CannotSell, // Not used
    NotEnoughSpace,
    MaxAmountReached,
    NotEnoughMoney,
    CannotRepair,
    Repaired,
    Refreshed,
    CannotRefresh,
    Destroyed,
    Welcome,
    BossCount
}