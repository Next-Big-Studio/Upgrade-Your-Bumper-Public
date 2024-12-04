using System;
using System.Collections.Generic;
using Car;
using Saving_System;
using UnityEngine;
using Upgrades;
using Weapons;

namespace Car
{
    public class CarInventory : MonoBehaviour
    {
        // The maximum number of upgrades that can be on a car at once
        public int maxUpgrades = 3;
        // The maximum number of weapons that can be on a car at once 
        public int maxWeapons = 3;

        // The amount of currency the player has
        public int currency;

        public CarStats carStats;

        // The upgrades on the car
        // The key is the id of the upgrade, the value is the number of that upgrade
        public List<UpgradeData> upgrades;

        // The weapons on the car
        // The key is the id of the weapon, the value is the number of that weapon
        public List<WeaponData> weapons;

        public void SetWeapons(WeaponInfo[] inWeapons)
        {
            foreach (WeaponInfo weapon in inWeapons)
            {
                weapons.Add(new WeaponData(weapon));
            }
        }

        // Add an upgrade to the car
        public void AddUpgrade(Upgrade upgrade)
        {
            UpgradeData find = upgrades.Find(u => u.id == upgrade.id);
            
            if (upgrades.Contains(find))
            {
                // If the upgrade is already on the car, increase the amount
                find.amount++;
            }
            else
            {
                // If the upgrade is not on the car, add it and increase the number of upgrades on the car
                UpgradeData newUpgrade = new(upgrade)
                {
                    price = upgrade.price / 4
                };
                upgrades.Add(newUpgrade);
            }

            carStats.ApplyUpgrade(upgrade);
        }

        // Remove an upgrade from the car
        public void RemoveUpgrade(Upgrade upgrade)
        {
            UpgradeData find = upgrades.Find(u => u.id == upgrade.id);
            
            if (find.amount > 1)
            {
                find.amount--;
            }
            else
            {
                upgrades.Remove(find);
            }

            carStats.UnApplyUpgrade(upgrade);
        }

        // Add a weapon to the car
        public void AddWeapon(WeaponInfo weapon)
        {
            WeaponData find = weapons.Find(w => w.id == weapon.id);

            if (weapons.Contains(find))
            {
                // If the weapon is already on the car, increase the amount
                find.amount++;
            }
            else
            {
                WeaponData newWeapon = new(weapon)
                {
                    price = weapon.price / 4
                };
                weapons.Add(newWeapon);
            }
        }

        // Remove a weapon from the car
        public void RemoveWeapon(WeaponInfo weapon)
        {
            WeaponData find = weapons.Find(w => w.id == weapon.id);
            if (find.amount > 1)
            {
                find.amount--;
            }
            else
            {
                weapons.Remove(find);
            }

        }
    }
}
