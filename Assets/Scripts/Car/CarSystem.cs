using System;
using System.Collections.Generic;
using System.Linq;
using Destructors;
using Mounters;
using Saving_System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Upgrades;
using Weapons;
using Random = UnityEngine.Random;

namespace Car
{
    public class CarSystem : MonoBehaviour
    {
        public int carID;
        public string carName;
        public CarDestruction carDestruction;
        public CarInventory carInventory;
        public CarMovement carMovement;
        public CarPosition carPosition;
        public CarStatus carStatus;
        public CarStats carStats;
        public Mounting carMounting;

        public GameObject carIconPrefab;
        public GameObject carIcon;
        //Color for leaderboard, body, and car icon
        public Color color;
        public GameObject nameTag;
        public UnityEvent<Upgrade> mysteryBoxEvent;

        private void Update()
        {
            // check if health is below 0
            float clampedHealth = Mathf.Clamp(carDestruction.totalDeformation, 0f, carDestruction.maxDeformation);
            
            if (clampedHealth >= carDestruction.maxDeformation)
            {
                carStatus.DestroyCar();
            }
        }

        private void Awake()
        {
            carMovement.carRef = this;
            carStatus.onCarDestroyed.AddListener(OnDestroyed);
        }

        private void OnDestroyed()
        {
            carStatus.isDestroyed = true;
            // Whatever else needs to be disabled when the car is destroyed
            DisableAll();
        }

        private void DisableAll()
        {
            try
            {
                carIcon.transform.GetChild(0).gameObject.SetActive(false);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            
            carMovement.enabled = false;
            carStatus.enabled = false;
            carDestruction.enabled = false;
            carPosition.enabled = false;
            carInventory.enabled = false;
            carStats.enabled = false;
            carMounting.enabled = false;
        }

        public void EnableAll()
        {
            carMovement.enabled = true;
            carDestruction.enabled = true;
            carPosition.enabled = true;
            carInventory.enabled = true;
            carStats.enabled = true;
            carStatus.enabled = true;
            carMounting.enabled = true;
        }
        
        // ====================================================================================================
        
        // Mount the weapons to the car
        public void MountWeapons(List<WeaponInfo> allWeapons)
        {
            if (carInventory.weapons == null)
            {
                return;
            }
            
            if (carInventory.weapons.Count == 0)
            {
                return;
            }
            
            // What did copilot cook up?
            // i also added to it!!! longest line of code i've written mayhaps

            foreach (GameObject weaponInstance in carInventory.weapons.Select(weapon => allWeapons.Find(w => w.id == weapon.id)).Select(weaponInfo => { GameObject newWeapon = Instantiate(weaponInfo.prefab, carMovement.colliderRb.transform, true); newWeapon.name = weaponInfo.name; return newWeapon; }))
            {
                weaponInstance.GetComponent<IMounter>().Mount(weaponInstance.name);
            } 
        }

        public void InitializeIcon()
        {
            carIcon = Instantiate(carIconPrefab, transform.position + Vector3.up * 100, Quaternion.Euler(90, 0, 0));
            carIcon.transform.SetParent(transform);
            carIcon.SetActive(true);
            carIcon.GetComponent<CarIcon>().SetColor(color);
        }

        // Set the color of the car (COLOR PROPERTY MUST BE SET BEFORE THIS FUNCTION IS CALLED)
        public void SetBodyColor()
        {
            // Generates shades based on original Toon shader values
            var(shade1, shade2) = GenerateShades();
            // Targets all bpdy parts
            List<string> bodyObjects = new List<string> {"Body", "Hood", "Better Engine", "Trunk"};
            // Grabs all the body parts and sets their colors corresponding color property
            carMovement.colliderRb.gameObject.GetComponentsInChildren<Transform>().ToList().ForEach(t =>
            {
                // Only bodyObjects are targeted
                if (bodyObjects.Contains(t.gameObject.name))
                {
                    // Grabs material and sets colors for the Toon shader
                    Material material = t.GetComponent<MeshRenderer>().materials[0];
                    material.SetColor("_BaseColor", color);
                    material.SetColor("_1st_ShadeColor", shade1);
                    material.SetColor("_2nd_ShadeColor", shade2);
                }
            });
        }
        
        // Generates shades based on original Toon shader values for Max
        private (Color, Color) GenerateShades(){
            float shade1Ratio = 108f / 255f;
            float shade2Ratio = 51f / 255f;

            Color shade1 = new Color(
                color.r * shade1Ratio,
                color.g * shade1Ratio,
                color.b * shade1Ratio
            );

            Color shade2 = new Color(
                color.r * shade2Ratio,
                color.g * shade2Ratio,
                color.b * shade2Ratio
            );

            return (shade1, shade2);
        }

        public void SetNameTag()
        {
            nameTag.GetComponentInChildren<TextMeshPro>().text = carName;
            nameTag.SetActive(true);
        }
    }
}