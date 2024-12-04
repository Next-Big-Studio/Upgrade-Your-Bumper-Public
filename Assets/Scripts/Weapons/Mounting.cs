using Mounters;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    public class Mounting : MonoBehaviour
    {
        public Transform leftSideMount;
        public Transform rightSideMount;
        public Transform topMount;
        public Transform backRightMount;
        public Transform backLeftMount;
        public Transform backMount;
        public Transform frontMount;
        public Transform hoodMount;

        public Transform[] wheels;
        Transform[] generalMountingSpots;

        public List<GameObject> currentlyMountedObjects;

        private void Start()
        {
            generalMountingSpots = new Transform[] { rightSideMount, leftSideMount, topMount, frontMount, backMount, backRightMount, backLeftMount, hoodMount};
        }

        public void MountGeneral(Transform objectToMount, MountSpot spot, string name)
        {
            generalMountingSpots = new Transform[] { rightSideMount, leftSideMount, topMount, frontMount, backMount, backRightMount, backLeftMount, hoodMount};
            FindAndRemoveSimilarObjects(name);
            objectToMount.name = name;
            objectToMount.position = generalMountingSpots[(int)spot].position;
            objectToMount.rotation = Quaternion.LookRotation(transform.forward, transform.up);
            currentlyMountedObjects.Add(objectToMount.gameObject);
        }

        public void MountWeapon(Transform weaponToMount, Transform mountSpot, string name)
        {
            FindAndRemoveSimilarObjects(name);
            weaponToMount.name = name;
            weaponToMount.position = mountSpot.position;
            weaponToMount.rotation = Quaternion.LookRotation(transform.forward, transform.up);
            currentlyMountedObjects.Add(weaponToMount.gameObject);
        }

        // checks for any currently-mounted objects that have the same name. if it does, it removes the previous one in favor of the current one.
        public void FindAndRemoveSimilarObjects(string name)
        {
            for (int i = 0; i < currentlyMountedObjects.Count; i++)
            {
                if (currentlyMountedObjects[i].name.Contains(name))
                {
                    currentlyMountedObjects[i].GetComponent<IMounter>().Dismount();
                    currentlyMountedObjects.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    public enum MountSpot
    {
        Right = 0,
        Left = 1,
        Top = 2,
        Front = 3,
        Back = 4,
        BackRight = 5,
        BackLeft = 6,
        Hood = 7,
        Wheels = 8,
    }

}
