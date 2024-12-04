using System;
using UnityEngine;
using Upgrades;

namespace Upgrades
{
    [CreateAssetMenu(fileName = "Upgrade", menuName = "CarScriptableObjects/Upgrade", order = 2)]
    public class Upgrade : ScriptableObject
    {
        // Upgrades are just direct buffs to stats of the cars
        // Multiplicative upgrades are calculated after additive upgrades
        // Any given stat can be calculated from (base + sum(additive upgrades)) * sum(multiplicative upgrades)
        public Sprite icon;

        public GameObject prefab;
        public GameObject prefab2;

        public ApplicationType applicationType;

        // how many of this upgrade need to be bought for the 2nd model to be used instead of the first model?
        public int amountNeededForSecondModel;

        // the amount of slots this upgrade takes up. if you're wondering what this is,
        // dm me and i'll explain, it's kind of hard to explain as a comment
        public int price;

        // correspond directly to stats in the car destruction scirpt
        public float bodyStrength;
        public float minImpulseForDamage;
        public float deformationRequiredToDetachWheels;

        // correspond directly to stats in the carmovement script
        public float maxSpeed;
        public float acceleration;
        public float breakAcceleration;
        public float maxWheelAngle; // probably clamp this one to some value
        
        // For the car inventory
        public int maxAmount = 3;
        public bool isTemporary;
        
        // information for the player
        public new string name;
        public string description;
        public Rarity rarity;

        // for saving/loading purposes
        public int id;
        public int amount = 1;
    }

    public enum ApplicationType
    {
        Additive = 0,
        Multiplicative = 1,
    }

    public enum Rarity
    {
        Common = 5,
        Uncommon = 4,
        Rare = 3,
        Epic = 2,
        Legendary = 1
    }
}