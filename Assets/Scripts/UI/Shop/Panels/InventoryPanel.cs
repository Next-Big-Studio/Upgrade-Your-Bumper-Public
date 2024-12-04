using System.Collections.Generic;
using System.Linq;
using Car;
using Saving_System;
using TMPro;
using UI.Framework;
using UnityEngine;
using Upgrades;
using Weapons;

namespace UI.Shop.Panels
{
    public class InventoryPanel : UIPanel
    {
        public GameObject inventoryItemPrefab;
        
        // The player's upgrades and weapons
        private Dictionary<ShopObject, UpgradeData> _playerUpgradeObjects;
        private Dictionary<ShopObject, WeaponData> _playerWeaponObjects;
        
        private CarInventory _carInventory;

        public GameObject SellUI;
        public int amountOfSells = 2;

        private InventoryPanel()
        {
            panelName = "inventory";
        }
        
        private new void Awake()
        {
            base.Awake();
            
            SetSellUIText();
            
            // Initialize the player's inventory dictionaries
            _playerUpgradeObjects = new Dictionary<ShopObject, UpgradeData>();
            _playerWeaponObjects = new Dictionary<ShopObject, WeaponData>();
            
            Hide();
        }

        public void SetInventory(CarInventory carInventory)
        {
            _carInventory = carInventory;
        }
        
        public void AddUpgrade(UpgradeData upgradeData, Upgrade constUpgrade)
        {
            if (_playerUpgradeObjects.Any(u => u.Value.id == upgradeData.id))
            {
                _playerUpgradeObjects.First(u => u.Value.id == upgradeData.id).Value.amount += upgradeData.amount;
                _playerUpgradeObjects.First(u => u.Value.id == upgradeData.id).Key.UpdateAmount(upgradeData.amount.ToString());
            }
            else
            {
                CreateNewUpgradeObject(upgradeData, constUpgrade);
            }
        }
        
        public void AddWeapon(WeaponData weaponData, WeaponInfo constWeapon)
        {
            if (_playerWeaponObjects.Any(w => w.Value.id == weaponData.id))
            {
                _playerWeaponObjects.First(w => w.Value.id == weaponData.id).Value.amount += weaponData.amount;
                _playerWeaponObjects.First(w => w.Value.id == weaponData.id).Key.UpdateAmount(weaponData.amount.ToString());
            }
            else
            {
                CreateNewWeaponObject(weaponData, constWeapon);
            }
        }
        
        // Sell a weapon
        private void SellWeapon(WeaponData weaponData, ShopObject shopObj)
        {
            if (amountOfSells <= 0)
            {
                ShopManager.instance.PopupMessage(ShopError.CannotSell);
                return;
            }
            
            WeaponInfo constWeapon = ShopManager.instance.allWeapons.Find(w => w.id == weaponData.id); // get the fresh weapon from the list
            _carInventory.RemoveWeapon(constWeapon); // try to remove the weapon from the player's inventory
            _carInventory.currency += weaponData.price; // Add the weapon price to the player's currency

            // Check if the player has completely sold the weapon
            if (_carInventory.weapons.Find(w => w.id == weaponData.id) == null)
            {
                _playerWeaponObjects[shopObj].amount--;
            }

            amountOfSells--;
            
            // Update the UI
            ShopManager.instance.RefreshCurrencyUI(); // Update the currency UI
            ShopManager.instance.PopupMessage(ShopError.Sold, constWeapon.name);
            Refresh(); // Update the player's inventory UI
            
        }
        
        // Sell an upgrade
        private void SellUpgrade(UpgradeData upgradeData, ShopObject shopObj)
        {
            if (amountOfSells <= 0)
            {
                ShopManager.instance.PopupMessage(ShopError.CannotSell);
                return;
            }
            
            Upgrade constUpgrade =  ShopManager.instance.allUpgrades.Find(u => u.id == upgradeData.id); // get the fresh upgrade from the list
            _carInventory.RemoveUpgrade(constUpgrade); // try to remove the upgrade from the player's inventory
            _carInventory.currency += upgradeData.price; // Add the upgrade price to the player's currency

            // Check if the player has completely sold the upgrade
            if (_carInventory.upgrades.Find(u => u.id == upgradeData.id) == null)
            {
                _playerUpgradeObjects[shopObj].amount--;
            }
            
            amountOfSells--;

            // Update the UI
            ShopManager.instance.RefreshCurrencyUI(); // Update the currency UI
            ShopManager.instance.PopupMessage(ShopError.Sold, constUpgrade.name);
            Refresh(); // Update the player's inventory UI
            
        }

        // Create a new upgrade object in the player's inventory
        private void CreateNewUpgradeObject(UpgradeData upgradeData, Upgrade constUpgrade)
        {
            GameObject newObject = Instantiate(inventoryItemPrefab, content.transform); // instantiate the prefab
            ShopObject shopObject = newObject.GetComponent<ShopObject>(); // get the shop object from the prefab
            
            // Set the values of the shop object
            shopObject.SetValues(constUpgrade.icon, constUpgrade.name, upgradeData.price.ToString(), upgradeData.amount.ToString(), constUpgrade.description);
            
            // Set the button onClick listener
            shopObject.button.onClick.AddListener(() => SellUpgrade(upgradeData, shopObject));

            // Set parent to parentTransform
            newObject.transform.SetParent(content.transform);

            _playerUpgradeObjects.Add(shopObject, upgradeData);
        }
        
        // Create a new weapon object in the player's inventory
        private void CreateNewWeaponObject(WeaponData weaponData, WeaponInfo constWeapon)
        {
            GameObject newObject = Instantiate(inventoryItemPrefab, content.transform); // instantiate the prefab
            ShopObject shopObject = newObject.GetComponent<ShopObject>(); // get the shop object from the prefab
            
            // Set the values of the shop object
            shopObject.SetValues(constWeapon.icon, constWeapon.name, weaponData.price.ToString(), weaponData.amount.ToString(), constWeapon.description);
            
            // Set the button onClick listener
            shopObject.button.onClick.AddListener(() => SellWeapon(weaponData, shopObject));

            // Set parent to parentTransform
            newObject.transform.SetParent(content.transform);

            _playerWeaponObjects.Add(shopObject, weaponData);
        }
        
        // Refresh the player's inventory UI
        protected override void Refresh()
        {
            SetSellUIText();
            
            for (int i = _playerUpgradeObjects.Count - 1; i >= 0; i--)
            {
                var shopItem = _playerUpgradeObjects.ElementAt(i);
                if (shopItem.Value.amount <= 0)
                {
                    Destroy(shopItem.Key.gameObject);
                    _playerUpgradeObjects.Remove(shopItem.Key);
                }
                else
                {
                    // Update the amount of the upgrade in the player's inventory UI
                    shopItem.Key.UpdateAmount(shopItem.Value.amount.ToString());
                }
            }

            for (int i = _playerWeaponObjects.Count - 1; i >= 0; i--)
            {
                var shopItem = _playerWeaponObjects.ElementAt(i);
                if (shopItem.Value.amount <= 0)
                {
                    Destroy(shopItem.Key.gameObject);
                    _playerWeaponObjects.Remove(shopItem.Key);
                }
                else
                {
                    // Update the amount of the weapon in the player's inventory UI
                    shopItem.Key.UpdateAmount(shopItem.Value.amount.ToString());
                }
            }
        }
        
        // Show the player's inventory UI
        public override void Show()
        {
            base.Show();
            Refresh();
            
            SellUI.SetActive(true);
        }
        
        // Hide the player's inventory UI
        public override void Hide()
        {
            base.Hide();
            SellUI.SetActive(false);
        }
        
        private void SetSellUIText()
        {
            SellUI.GetComponentInChildren<TextMeshProUGUI>().text = $"You can only sell <color=red>{amountOfSells}</color> more {(amountOfSells > 1 ? "times" : "time")}!";
        }
    }
}