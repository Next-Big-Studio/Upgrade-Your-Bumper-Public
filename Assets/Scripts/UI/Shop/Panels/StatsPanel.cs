using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Car;
using UI.Framework;
using UnityEngine;
using Upgrades;
using Weapons;

namespace UI.Shop.Panels
{
    public class StatsPanel : UIPanel
    {
        private const bool DebugMode = false;

        private static readonly Dictionary<string, string> AllStats = new()
        {
            { "bodyStrength", "Body Strength" },
            { "minImpulseForDamage", "Damage Threshold" },
            { "maxSpeed", "Max Speed" },
            { "acceleration", "Acceleration" },
            { "breakAcceleration", "Break Acceleration" }
        };

        private readonly Dictionary<string, GameObject> _statsObjects = new();

        private List<Upgrade> _allUpgrades;
        private List<WeaponInfo> _allWeapons;
        public CarStats carStats;
        public CarInventory carInventory;

        [SerializeField] private GameObject statsPrefab;
        
        // Constructor
        private StatsPanel()
        {
            panelName = "stats";
            
        }
        
        public new void Awake()
        {
            base.Awake();
            Hide();
        }

        public void InitializeStatsPanel(CarStats inCarStats, CarInventory inCarInventory, List<Upgrade> inAllUpgrades, List<WeaponInfo> inAllWeapons)
        {
            carStats = inCarStats;
            carInventory = inCarInventory;
            _allUpgrades = inAllUpgrades;
            _allWeapons = inAllWeapons;

            foreach (var stat in AllStats)
            {
                float baseValue = carStats.GetStatByVariableName(stat.Key);
                StatData statData = new(stat.Key, stat.Value, baseValue, baseValue, baseValue);

                GameObject statsObject = Instantiate(statsPrefab, content.transform);
                statsObject.GetComponent<StatsObject>().SetValues(statData);
                _statsObjects.Add(stat.Key, statsObject);
            }

            Refresh();

            carInventory = ShopManager.instance.carInventory;
            carStats = ShopManager.instance.playerSystem.carStats;
        }

        private void FindMaxValue(string statName)
        {
            var upgrades = carInventory.upgrades
                .Select(data => _allUpgrades.Find(u => u.id == data.id))
                .Where(upg => IsValidUpgrade(upg, statName))
                .ToList();

            if (upgrades.Count < carInventory.maxUpgrades)
            {
                upgrades.AddRange(_allUpgrades.Where(upg => !upgrades.Contains(upg) && IsValidUpgrade(upg, statName)));
            }

            if (upgrades.Count == 0) return;

            upgrades = upgrades
                .OrderByDescending(u => GetStatValue(u, statName))
                .ThenByDescending(u => u.applicationType)
                .Take(carInventory.maxUpgrades)
                .ToList();

            float maxValue = _statsObjects[statName].GetComponent<StatsObject>().StatData.baseValue;
            foreach (Upgrade upgrade in upgrades)
            {
                float value = GetStatValue(upgrade, statName);
                maxValue = upgrade.applicationType switch
                {
                    ApplicationType.Additive => maxValue + value * upgrade.maxAmount,
                    ApplicationType.Multiplicative => maxValue * value * upgrade.maxAmount,
                    _ => maxValue
                };
            }

            _statsObjects[statName].GetComponent<StatsObject>().SetMaxValue(maxValue);

            if (DebugMode)
            {
                PrintDebugInfo(statName, maxValue, upgrades);
            }
        }

        private static bool IsValidUpgrade(Upgrade upgrade, string statName)
        {
            if (upgrade == null) return false;
            float value = GetStatValue(upgrade, statName);
            return (upgrade.applicationType == ApplicationType.Additive && value > 0) ||
                   (upgrade.applicationType == ApplicationType.Multiplicative && value > 1) ||
                   (upgrade.applicationType == ApplicationType.Additive && value < 0) ||
                   (upgrade.applicationType == ApplicationType.Multiplicative && value < 1);
        }

        private static float GetStatValue(Upgrade upgrade, string statName)
        {
            FieldInfo fieldInfo = upgrade.GetType().GetField(statName);
            if (fieldInfo == null) return 0;
    
            return fieldInfo.GetValue(upgrade) is float value ? value : 0;
        }


        private void PrintDebugInfo(string statName, float maxValue, List<Upgrade> upgrades)
        {
            print(statName + " " + maxValue);
            print("Three best upgrades for " + statName + ":");
            foreach (Upgrade upgrade in upgrades)
            {
                float value = GetStatValue(upgrade, statName);
                print(upgrade.name + " " + value + " x" + upgrade.maxAmount);
            }
        }

        protected override void Refresh()
        {
            foreach (string stat in AllStats.Keys)
            {
                FindMaxValue(stat);
                float value = carStats.GetStatByVariableName(stat);
                _statsObjects[stat].GetComponent<StatsObject>().SetValue(value);
            }
        }
    }
}
