using Destructors;
using Mounters;
using System.Collections.Generic;
using Saving_System;
using UnityEngine;
using Upgrades;

namespace Car
{
    public class CarStats : MonoBehaviour
    {
        // this should be THE ONLY PLACE that car stats are accessed from. refactor ALL CODE to reference this script.
        // it's extremely important to have this centralized. we don't want two different scripts having different ideas of what the car's acceleration is

        // correspond directly to stats in the car destruction scirpt
        public float bodyStrength;
        public float minImpulseForDamage;

        // correspond directly to stats in the CarMovement script
        public float maxSpeed;
        public float acceleration;
        public float breakAcceleration;
        public float maxWheelAngle; // probably clamp this one to some value

        // =================================================================================================================================================
        // ======================================================== METHODS ================================================================================
        // =================================================================================================================================================

        // This applies a list of upgrades to the car
        public void ApplyUpgrades(Upgrade[] upgrades)
        {
            if (upgrades.Length == 0) return;

            var additiveUpgrades = new List<Upgrade>();
            var multiplicativeUpgrades = new List<Upgrade>();
            foreach (Upgrade upgrade in upgrades)
            {
                if (upgrade.applicationType == ApplicationType.Additive)
                    additiveUpgrades.Add(upgrade);
                else
                    multiplicativeUpgrades.Add(upgrade);
            }

            AddUpgradeAndStats(additiveUpgrades.ToArray());
            MultiplyUpgradeAndStats(multiplicativeUpgrades.ToArray());
        }

        public void ApplyUpgrade(Upgrade upgrade)
        {
            
            if (upgrade.applicationType == ApplicationType.Additive)
                AddUpgradeAndStats(new[] { upgrade });
            else
                MultiplyUpgradeAndStats(new[] { upgrade });
        }

        // This unapplies a list of upgrades to the car
        public void UnApplyUpgrades(Upgrade[] upgrades)
        {
            if (upgrades.Length == 0) return;

            var additiveUpgrades = new List<Upgrade>();
            var multiplicativeUpgrades = new List<Upgrade>();
            foreach (Upgrade upgrade in upgrades)
            {
                if (upgrade.applicationType == ApplicationType.Additive)
                    additiveUpgrades.Add(upgrade);
                else
                    multiplicativeUpgrades.Add(upgrade);
            }

            SubtractUpgradeAndStats(additiveUpgrades.ToArray());
            DivideUpgradeAndStats(multiplicativeUpgrades.ToArray());
        }

        // This unapplies a single upgrade to the car
        public void UnApplyUpgrade(Upgrade upgrade)
        {
            if (upgrade.applicationType == ApplicationType.Additive)
                SubtractUpgradeAndStats(new[] { upgrade });
            else
                DivideUpgradeAndStats(new[] { upgrade });
        }

        // =================================================================================================================================================
        // ======================================================== HELPER FUNCTIONS ========================================================================
        // =================================================================================================================================================

        // This adds the stats of a list of upgrades to the car
        public void AddUpgradeAndStats(Upgrade[] rights)
        {
            foreach (Upgrade right in rights)
            {
                bodyStrength += right.bodyStrength;
                minImpulseForDamage += right.minImpulseForDamage;
                acceleration += right.acceleration;
                breakAcceleration += right.breakAcceleration;
                maxWheelAngle += right.maxWheelAngle;
                maxSpeed += right.maxSpeed;
            }
        }

        // This subtracts the stats of a list of upgrades from the car
        public void SubtractUpgradeAndStats(Upgrade[] rights)
        {
            foreach (Upgrade right in rights)
            {
                bodyStrength -= right.bodyStrength;
                minImpulseForDamage -= right.minImpulseForDamage;
                acceleration -= right.acceleration;
                breakAcceleration -= right.breakAcceleration;
                maxWheelAngle -= right.maxWheelAngle;
                maxSpeed -= right.maxSpeed;
            }
        }

        // This multiplies the stats of a list of upgrades to the car
        public void MultiplyUpgradeAndStats(Upgrade[] rights)
        {
            if (rights.Length == 0) return;

            float bodyStrength = 0;
            float minImpulseForDamage = 0;
            float acceleration = 0;
            float breakAcceleration = 0;
            float maxWheelAngle = 0;
            float maxSpeed = 0;

            foreach (Upgrade upgrade in rights)
            {
                bodyStrength += upgrade.bodyStrength - 1;
                minImpulseForDamage += upgrade.minImpulseForDamage - 1;
                acceleration += upgrade.acceleration - 1;
                breakAcceleration += upgrade.breakAcceleration - 1;
                maxWheelAngle += upgrade.maxWheelAngle - 1;
                maxSpeed += upgrade.maxSpeed - 1;
            }

            this.bodyStrength *= 1 + bodyStrength;
            this.maxWheelAngle *= 1 + maxWheelAngle;
            this.minImpulseForDamage *= 1 + minImpulseForDamage;
            this.acceleration *= 1 + acceleration;
            this.breakAcceleration *= 1 + breakAcceleration;
            this.maxSpeed *= 1 + maxSpeed;
        }

        // This divides the stats of a list of upgrades from the car
        public void DivideUpgradeAndStats(Upgrade[] rights)
        {
            if (rights.Length == 0) return;

            float bodyStrength = 0;
            float minImpulseForDamage = 0;
            float acceleration = 0;
            float breakAcceleration = 0;
            float maxWheelAngle = 0;
            float maxSpeed = 0;

            foreach (Upgrade upgrade in rights)
            {
                bodyStrength += 1 - upgrade.bodyStrength;
                minImpulseForDamage += 1 - upgrade.minImpulseForDamage;
                acceleration += 1 - upgrade.acceleration;
                breakAcceleration += 1 - upgrade.breakAcceleration;
                maxWheelAngle += 1 - upgrade.maxWheelAngle;
                maxSpeed += 1 - upgrade.maxSpeed;
            }

            this.bodyStrength /= 1 + bodyStrength;
            this.maxWheelAngle /= 1 + maxWheelAngle;
            this.minImpulseForDamage /= 1 + minImpulseForDamage;
            this.acceleration /= 1 + acceleration;
            this.breakAcceleration /= 1 + breakAcceleration;
            this.maxSpeed /= 1 + maxSpeed;
        }

        // puts the visual upgrade on the car!
        // visual upgrades are destructible, not detachable
        // would be kinda silly if your "super bumper" just fell off
        public void ApplyVisualUpgrades(UpgradeData[] upgrades)
        {
            foreach (UpgradeData upgradeData in upgrades)
            {
                try
                {
                    // get the model!
                    Upgrade upgrade = GameManager.Instance.GetUpgradeFromID(upgradeData.id);
                    if (!upgrade.prefab) continue;
                    GameObject model = upgradeData.amount >= upgrade.amountNeededForSecondModel ? upgrade.prefab2 : upgrade.prefab;
                    // put the model on!
                    GameObject instantiatedModel = Instantiate(model, transform.GetComponent<CarSystem>().carPosition.transform, true);
                    instantiatedModel.GetComponent<IMounter>().Mount(upgrade.name);
                    GetComponent<CarMovement>().colliderRb.GetComponent<CarDestruction>().AddMesh(instantiatedModel.GetComponent<MeshFilter>());
                } catch
                {
                    // ignored
                }
            }
        }
        
        // removes the visual upgrade from the car
        public void RemoveVisualUpgrades(UpgradeData[] upgrades)
        {
            foreach (UpgradeData upgradeData in upgrades)
            {
                // get the model!
                Upgrade upgrade = GameManager.Instance.GetUpgradeFromID(upgradeData.id);
                if (!upgrade.prefab) continue;
                GameObject model = upgradeData.amount >= upgrade.amountNeededForSecondModel ? upgrade.prefab2 : upgrade.prefab;
                // remove the model!
                GameObject instantiatedModel = Instantiate(model, transform.GetComponent<CarSystem>().carPosition.transform, true);
                instantiatedModel.GetComponent<IMounter>().Dismount();
                GetComponent<CarMovement>().colliderRb.GetComponent<CarDestruction>().RemoveMesh(instantiatedModel.GetComponent<MeshFilter>());
            }
        }
        
        public float GetStatByVariableName(string inName)
        {
            return (float)GetType().GetField(inName).GetValue(this); 
        }
    }
}
