using Car;
using Saving_System;
using UnityEngine;
using Upgrades;
using Weapons;

namespace Shop
{
    public abstract class SlotPrize
    {
        // Properties
        //      PrizeName: The name of the prize
        //      PrizeSprite: The sprite of the prize
        //      Weight: The weight of the prize (higher weight means higher chance of winning)
        //      Amount: The amount of the prize
        
        public string PrizeName { get; protected set; }
        public Sprite PrizeSprite { get; protected set; }
        public int Weight { get; protected set; } // Add this property for weighting
        public int Amount { get; set; } // Add this property for amount


        protected SlotPrize(string prizeName, Sprite prizeSprite, int weight)
        {
            PrizeName = prizeName;
            PrizeSprite = prizeSprite;
            Weight = weight;
            Amount = 1;
        }
        
        public abstract string ApplyPrize(CarInventory carInventory, GambleData gambleData);
    }

    public class SlotPrizeMoney : SlotPrize
    {
        // Properties
        //      PrizeValue: The value of the prize in currency
        public int PrizeValue { get; private set; }

        // Private constructor to enforce factory method usage
        private SlotPrizeMoney(string prizeName, Sprite prizeSprite, int prizeValue, int weight) : base(prizeName, prizeSprite, weight)
        {
            PrizeValue = prizeValue;
        }

        // Factory method for creating a SlotPrizeMoney instance
        public static SlotPrizeMoney CreateMoneyPrize(string prizeName, Sprite prizeSprite, int prizeValue, int weight)
        {
            return new SlotPrizeMoney(prizeName, prizeSprite, prizeValue, weight);
        }
        
        public override string ApplyPrize(CarInventory carInventory, GambleData gambleData)
        {
            int totalAmount = Amount * PrizeValue;
            carInventory.currency += totalAmount;
            gambleData.amountWon += totalAmount;
            return $"You won ${totalAmount}!";
        }
    }
    
    public class SlotPrizeUpgrade : SlotPrize
    {
        // Properties
        //      Upgrade: The upgrade prize
        public Upgrade Upgrade { get; private set; }

        // Private constructor to enforce factory method usage
        private SlotPrizeUpgrade(Upgrade upgrade, int weight) : base(upgrade.name, upgrade.icon, weight)
        {
            Upgrade = upgrade;
        }

        // Factory method for creating a SlotPrizeUpgrade instance
        public static SlotPrizeUpgrade CreateUpgradePrize(Upgrade upgrade)
        {
            return new SlotPrizeUpgrade(upgrade, (int)upgrade.rarity);
        }
        
        public override string ApplyPrize(CarInventory carInventory, GambleData gambleData)
        {
            int totalAmount = Amount * Upgrade.price;
            gambleData.amountWon += totalAmount;
            
            int leftOver = 0;
            
            for (int i = 0; i < Amount; i++)
            {
                if (carInventory.upgrades.Find(u => u.id == Upgrade.id).amount < Upgrade.maxAmount)
                {
                    carInventory.AddUpgrade(Upgrade);
                }
                else
                {
                    carInventory.currency += Upgrade.price;
                    leftOver += Upgrade.price;
                }
            }
            
            // If the player already has the maximum amount of upgrades, return the amount of currency instead
            // Terrible code, but whatever
            string result = leftOver > 0 ? $"You won {Amount} {Upgrade.name}! But you already have the maximum amount of {Upgrade.name}! You received ${leftOver} instead!" : $"You won {Amount} {Upgrade.name}!";
            
            return result;
        }
    }
    
    public class SlotPrizeWeapon : SlotPrize
    {
        // Properties
        //      Weapon: The weapon prize
        public WeaponInfo Weapon { get; private set; }

        // Private constructor to enforce factory method usage
        private SlotPrizeWeapon(WeaponInfo weaponInfo, int weight) : base(weaponInfo.name, weaponInfo.icon, weight)
        {
            Weapon = weaponInfo;
        }

        // Factory method for creating a SlotPrizeWeapon instance
        public static SlotPrizeWeapon CreateWeaponPrize(WeaponInfo weaponInfo)
        {
            return new SlotPrizeWeapon(weaponInfo, (int)weaponInfo.rarity);
        }
        
        public override string ApplyPrize(CarInventory carInventory, GambleData gambleData)
        {
            int totalAmount = Amount * Weapon.price;
            gambleData.amountWon += totalAmount;
            
            int leftOver = 0;
            
            for (int i = 0; i < Amount; i++)
            {
                if (carInventory.weapons.Find(w => w.id == Weapon.id).amount < Weapon.maxAmount)
                {
                    carInventory.AddWeapon(Weapon);
                }
                else
                {
                    carInventory.currency += Weapon.price;
                    leftOver += Weapon.price;
                }
            }
            
            // If the player already has the maximum amount of weapons, return the amount of currency instead
            // Terrible code, but whatever
            string result = leftOver > 0 ? $"You won {Amount} {Weapon.name}! But you already have the maximum amount of {Weapon.name}! You received ${leftOver} instead!" : $"You won {Amount} {Weapon.name}!";
            
            return result;
        }
    }
}
