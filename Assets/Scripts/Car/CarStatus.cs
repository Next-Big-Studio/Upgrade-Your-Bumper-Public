using System;
using Car;
using Destructors;
using UnityEngine;
using UnityEngine.Events;

namespace Car
{
    public class CarStatus : MonoBehaviour
    {
        public bool canMove;
        public bool canFire;
        public bool canDrift;
        public bool canBoost;
        public bool canUseItem;
        public bool isDestroyed;
        public bool isBounty;
        
        public UnityEvent onCarDestroyed;
        
        public void DestroyCar()
        {
            DisableCar();
            isDestroyed = true;
            
            onCarDestroyed.Invoke(); // Invoke the event to disable all car components
        }
        
        public void EnableCar()
        {
            canMove = true;
            canFire = true;
            canDrift = true;
            canBoost = true;
            canUseItem = true;
        }
        
        public void DisableCar()
        {
            canMove = false;
            canFire = false;
            canDrift = false;
            canBoost = false;
            canUseItem = false;
        }
    }
}