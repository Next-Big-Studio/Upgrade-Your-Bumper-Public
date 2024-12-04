using System;
using UnityEngine;
using Weapons;

namespace Weapons
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "CarScriptableObjects/Weapon", order = 2)]
    public class WeaponInfo : ScriptableObject
    {
        public float damage;
        public float fireRate;
        public bool autoAim;
        public float range;
        public float damageRadius;
        public GameObject prefab;
        public Sprite icon;
        public new string name;
        public string description;
        public int price;
        public int amount = 1;
        public int maxAmount = 3;
        public int id;
        public Rarity rarity;
        
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
