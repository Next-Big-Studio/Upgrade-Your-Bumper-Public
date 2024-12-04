using System;
using System.Collections.Generic;
using Car;
using Upgrades;
using Weapons;

namespace Saving_System
{
    [Serializable]
    public class CarSystemData
    {
        public int carID;
        public string carName;
        public CarInventoryData carInventoryData;
        public CarPositionData carPositionData;
        public CarStatusData carStatusData;
    
        public CarSystemData()
        {
            carID = -1;
            carName = "Max V.";
            carInventoryData = new CarInventoryData();
            carPositionData = new CarPositionData();
            carStatusData = new CarStatusData();
        }
    
        public CarSystemData(CarSystem carSystem)   
        {
            carID = carSystem.carID;
            carName = carSystem.carName;
            carInventoryData = new CarInventoryData(carSystem.carInventory);
            carPositionData = new CarPositionData(carSystem.carPosition);
            carStatusData = new CarStatusData(carSystem.carStatus);
        }
    }
    
    [Serializable]
    public class CarInventoryData
    {
        public int currency;
        public List<UpgradeData> upgrades;
        public List<WeaponData> weapons;

        public CarInventoryData()
        {
            currency = 0;
            upgrades = new List<UpgradeData>();
            weapons = new List<WeaponData>();
        }
    
        public CarInventoryData(CarInventory carInventory)
        {
            currency = carInventory.currency;
            upgrades = carInventory.upgrades;
            weapons = carInventory.weapons;
        }
    }
    
    [Serializable]
    public class CarPositionData
    {
        public int currentPosition;

        public CarPositionData()
        {
            currentPosition = 0;
        }
    
        public CarPositionData(CarPosition carPosition)
        {
            currentPosition = carPosition.currentPosition;
        }
    }
    
    [Serializable]
    public class CarStatusData
    {
        public bool isDestroyed;
    
        public CarStatusData()
        {
            isDestroyed = false;
        }
    
        public CarStatusData(CarStatus carStatus)
        {
            isDestroyed = carStatus.isDestroyed;
        }
    }
    
    [Serializable]
    public class UpgradeData
    {
        public int id;
        public int amount;
        public int price;

        public UpgradeData(int intID, int intAmount, int intPrice)
        {
            id = intID;
            amount = intAmount;
            price = intPrice;
        }
    
        public UpgradeData(Upgrade upgrade)
        {
            id = upgrade.id;
            amount = upgrade.amount;
            price = upgrade.price;
        }
    }
    
    [Serializable]
    public class WeaponData
    {
        public int id;
        public int amount;
        public int price;
    
        public WeaponData(int intID, int intAmount, int intPrice)
        {
            id = intID;
            amount = intAmount;
            price = intPrice;
        }

        public WeaponData(WeaponInfo weapon)
        {
            id = weapon.id;
            amount = weapon.amount;
            price = weapon.price;
        }
    }
}