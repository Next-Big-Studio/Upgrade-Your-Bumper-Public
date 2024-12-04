using System.Collections.Generic;
using System.Linq;
using Car;
using Saving_System;
using TMPro;
using UI.Framework;
using UnityEngine;
using UnityEngine.UI;
using Upgrades;
using Weapons;
using Random = UnityEngine.Random;

namespace UI.Shop.Panels
{
    public sealed class ShopPanel : UIPanel
    {
        public GameObject shopItemPrefab;

        // The shop's upgrades and weapons
        private Dictionary<ShopObject, UpgradeData> _upgradeObjects;
        private Dictionary<ShopObject, WeaponData> _weaponObjects;

        // The max number of upgrades and weapons shown in the shop
        private const int MaxShownUpgrades = 3;
        private const int MaxShownWeapons = 3;

        // The player's inventory
        private CarInventory _carInventory;

        // Refresh features
        public GameObject refreshUI;
        //Audio Source for shopObjects
        public AudioSource menuSFX;
        private int _refreshCount = 3; // The number of times the shop can be refreshed
        
        private ShopPanel()
        {
            panelName = "shop";
        }
        
        public new void Awake()
        {
            base.Awake();
            
            _upgradeObjects = new Dictionary<ShopObject, UpgradeData>();
            _weaponObjects = new Dictionary<ShopObject, WeaponData>();
            
            refreshUI.GetComponentInChildren<TextMeshProUGUI>().text = $"Refresh ({_refreshCount})"; // Set the refresh button text
            refreshUI.GetComponentInChildren<Button>().onClick.AddListener(RefreshContent); // Add a listener to the refresh button
            Hide();
        }

        public void SetInventory(CarInventory carInventory)
        {
            // Set the player's inventory
            _carInventory = carInventory;
        }

        // Get random upgrades for the shop
        public void GetRandomUpgrades()
        {
            var allUpgrades = ShopManager.instance.allUpgrades;
            _upgradeObjects = new Dictionary<ShopObject, UpgradeData>();

            var randomUpgrades = new List<UpgradeData>();

            // If there are fewer upgrades than the max shown upgrades, use all upgrades
            if (allUpgrades.Count <= MaxShownUpgrades)
                randomUpgrades.AddRange(allUpgrades.ConvertAll(u => new UpgradeData(u)));
            // Otherwise, use random upgrades
            else
                while (randomUpgrades.Count < MaxShownUpgrades)
                {
                    int randomIndex = Random.Range(0, allUpgrades.Count);
                    UpgradeData upgrade = new(allUpgrades[randomIndex]);
                    UpgradeData existingUpgrade = randomUpgrades.Find(u => u.id == upgrade.id);
                    
                    if (existingUpgrade == null)
                    {
                        randomUpgrades.Add(new UpgradeData(allUpgrades[randomIndex]));
                    }
                    else
                    {
                        existingUpgrade.amount++;
                    }
                }

            // Create the new upgrade objects
            foreach (UpgradeData upgradeData in randomUpgrades)
            {
                CreateNewUpgradeObject(upgradeData, allUpgrades.Find(u => u.id == upgradeData.id));
            }
        }

        // Get random weapons for the shop
        public void GetRandomWeapons()
        {
            var allWeapons = ShopManager.instance.allWeapons;
            _weaponObjects = new Dictionary<ShopObject, WeaponData>();

            var randomWeapons = new List<WeaponData>();

            // If there are fewer weapons than the max shown weapons, use all weapons
            if (allWeapons.Count <= MaxShownWeapons)
                randomWeapons.AddRange(allWeapons.ConvertAll(w => new WeaponData(w)));
            // Otherwise, use random weapons
            else
                while (randomWeapons.Count < MaxShownWeapons)
                {
                    int randomIndex = Random.Range(0, allWeapons.Count);
                    WeaponData weapon = new(allWeapons[randomIndex]);
                    WeaponData existingWeapon = randomWeapons.Find(w => w.id == weapon.id);
                    
                    if (existingWeapon == null)
                    {
                        randomWeapons.Add(weapon);
                    }
                    else
                    {
                        existingWeapon.amount++;
                    }
                }

            // Create the new weapon objects
            foreach (WeaponData weaponData in randomWeapons)
            {
                CreateNewWeaponObject(weaponData, allWeapons.Find(w => w.id == weaponData.id));
            }
        }
        
        // Buy an upgrade
        private void BuyUpgrade(UpgradeData upgradeData, ShopObject shopObj)
        {
            // Check if the player has enough money for the upgrade
            if (_carInventory.currency < upgradeData.price)
            {
                ShopManager.instance.PopupMessage(ShopError.NotEnoughMoney);
                return;
            }

            // Check if the player can buy the upgrade
            if (shopObj.disabled)
            {
                ShopManager.instance.PopupMessage(ShopError.CannotBuy);
                return;
            }

            Upgrade constUpgrade = ShopManager.instance.allUpgrades.Find(u => u.id == upgradeData.id); // Get the fresh upgrade from the list
            UpgradeData existingUpgrade = _carInventory.upgrades.FirstOrDefault(upgrade => upgrade.id == upgradeData.id); // Get the existing upgrade from the player's inventory

            // Check if the player has reached the maximum amount of the upgrade
            if (existingUpgrade != null && existingUpgrade.amount == constUpgrade.maxAmount)
            {
                ShopManager.instance.PopupMessage(ShopError.MaxAmountReached, "upgrade");
                return;
            }

            // Check if the player has enough space for the upgrade
            if (existingUpgrade == null && _carInventory.upgrades.Count >= _carInventory.maxUpgrades)
            {
                ShopManager.instance.PopupMessage(ShopError.NotEnoughSpace, "upgrades");
                return;
            }

            _carInventory.AddUpgrade(constUpgrade); // try to add the upgrade to the player's inventory
            _carInventory.currency -= upgradeData.price; // Subtract the upgrade price from the player's currency
            _upgradeObjects[shopObj].amount--; // Update the amount of the upgrade in the shop UI

            // Check if the player already had the upgrade in their inventory
            if (existingUpgrade == null)
            {
                UpgradeData existingUpgradeData = _carInventory.upgrades.Find(u => u.id == upgradeData.id); // Get that upgrade object because we just added it
                ShopManager.instance.inventoryPanel.AddUpgrade(existingUpgradeData, constUpgrade); // Create a new upgrade object in the player's inventory UI
            }

            // Updating the UI
            ShopManager.instance.RefreshCurrencyUI(); // Update the currency UI
            Refresh(); // Update the shop UI

            ShopManager.instance.PopupMessage(ShopError.Bought, constUpgrade.name);
        }

        // Buy a weapon
        private void BuyWeapon(WeaponData weaponData, ShopObject shopObj)
        {
            // Checks
            if (_carInventory.currency < weaponData.price)
            {
                ShopManager.instance.PopupMessage(ShopError.NotEnoughMoney);
                return;
            }

            if (shopObj.disabled)
            {
                ShopManager.instance.PopupMessage(ShopError.CannotBuy);
                return;
            }

            WeaponInfo constWeapon = ShopManager.instance.allWeapons.Find(w => w.id == weaponData.id); // get the fresh weapon from the list
            WeaponData existingWeapon = _carInventory.weapons.FirstOrDefault(weapon => weapon.id == weaponData.id); // get the existing weapon from the player's inventory

            // Check if the player has reached the maximum amount of the weapon
            if (existingWeapon != null && existingWeapon.amount == constWeapon.maxAmount)
            {
                ShopManager.instance.PopupMessage(ShopError.MaxAmountReached, "weapon");
                return;
            }

            // Check if the player has enough space for the weapon
            if (existingWeapon == null && _carInventory.weapons.Count >= _carInventory.maxWeapons)
            {
                ShopManager.instance.PopupMessage(ShopError.NotEnoughSpace, "weapons");
                return;
            }

            _carInventory.AddWeapon(constWeapon); // try to add the weapon to the player's inventory
            _carInventory.currency -= weaponData.price; // Subtract the weapon price from the player's currency
            _weaponObjects[shopObj].amount--; // Update the amount of the weapon in the shop UI

            // Check if the player already had the weapon in their inventory
            if (existingWeapon == null)
            {
                WeaponData existingWeaponData = _carInventory.weapons.Find(w => w.id == weaponData.id); // Get that weapon object because we just added it
                ShopManager.instance.inventoryPanel.AddWeapon(existingWeaponData, constWeapon); // Create a new weapon object in the player's inventory UI
            }

            // Updating the UI
            ShopManager.instance.RefreshCurrencyUI(); // Update the currency UI
            ShopManager.instance.PopupMessage(ShopError.Bought, constWeapon.name);
            Refresh(); // Update the shop UI
        }

        // Create a new upgrade object in the shop
        private void CreateNewUpgradeObject(UpgradeData upgradeData, Upgrade constUpgrade)
        {
            GameObject newObject = Instantiate(shopItemPrefab, content.transform); // instantiate the prefab
            ShopObject shopObject = newObject.GetComponent<ShopObject>(); // get the shop object from the prefab
            ButtonSound buttonSound = newObject.GetComponent<ButtonSound>();


            // Set the values of the shop object
            shopObject.SetValues(constUpgrade.icon, constUpgrade.name, upgradeData.price.ToString(), upgradeData.amount.ToString(), constUpgrade.description);

            // Set the button onClick listener
            shopObject.button.onClick.AddListener(() => BuyUpgrade(upgradeData, shopObject));

            // Set parent to parentTransform
            newObject.transform.SetParent(content.transform);
            
            //Sets audio source for shop object as menuSFX
            buttonSound.audioSource = menuSFX;

            _upgradeObjects.Add(shopObject, upgradeData);
        }

        // Create a new weapon object in the shop
        private void CreateNewWeaponObject(WeaponData weaponData, WeaponInfo constWeapon)
        {
            GameObject newObject = Instantiate(shopItemPrefab, content.transform); // instantiate the prefab
            ShopObject shopObject = newObject.GetComponent<ShopObject>(); // get the shop object from the prefab
            ButtonSound buttonSound = newObject.GetComponent<ButtonSound>();

            // Set the values of the shop object
            shopObject.SetValues(constWeapon.icon, constWeapon.name, weaponData.price.ToString(), weaponData.amount.ToString(), constWeapon.description);

            // Set the button onClick listener
            shopObject.button.onClick.AddListener(() => BuyWeapon(weaponData, shopObject));

            // Set parent to parentTransform
            newObject.transform.SetParent(content.transform);

            //Sets audio source for shop object as menuSFX
            buttonSound.audioSource = menuSFX;

            _weaponObjects.Add(shopObject, weaponData);
        }

        // Refresh the shop's upgrades and weapons
        private void RefreshContent()
        {
            if (_refreshCount > 0)
            {
                _refreshCount--;

                // Clear the shop's upgrades and weapons
                foreach (var upgrade in _upgradeObjects) Destroy(upgrade.Key.gameObject);

                foreach (var weapon in _weaponObjects) Destroy(weapon.Key.gameObject);

                GetRandomUpgrades(); // Get new upgrades
                GetRandomWeapons(); // Get new weapons

                Refresh(); // Refresh the shop UI
                refreshUI.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = $"Refresh ({_refreshCount})"; // Update the refresh button text

                ShopManager.instance.PopupMessage(ShopError.Refreshed); // Show a popup message

                if (_refreshCount == 0)
                {
                    refreshUI.GetComponentInChildren<Button>().interactable = false; // Disable the refresh button
                }
            }
            else
            {
                ShopManager.instance.PopupMessage(ShopError.CannotRefresh);
            }
        }

        // Refresh the Shop UI based on the player's currency
        private new void Refresh()
        {
            int currency = _carInventory.currency;
            for (int i = _upgradeObjects.Count - 1; i >= 0; i--)
            {
                var shopItem = _upgradeObjects.ElementAt(i);
                if (shopItem.Value.amount <= 0)
                {
                    Destroy(shopItem.Key.gameObject);
                    _upgradeObjects.Remove(shopItem.Key);
                }
                else
                {
                    shopItem.Key.UpdateAmount(shopItem.Value.amount.ToString());
                    shopItem.Key.SetDisabled(shopItem.Value.price > currency);
                }
            }

            for (int i = _weaponObjects.Count - 1; i >= 0; i--)
            {
                var shopItem = _weaponObjects.ElementAt(i);
                if (shopItem.Value.amount <= 0)
                {
                    Destroy(shopItem.Key.gameObject);
                    _weaponObjects.Remove(shopItem.Key);
                }
                else
                {
                    shopItem.Key.UpdateAmount(shopItem.Value.amount.ToString());
                    shopItem.Key.SetDisabled(shopItem.Value.price > currency);
                }
            }
        }
    }
}