using System.Collections.Generic;
using UnityEngine;
using System;
using Car;

public class CarLeaderboard : MonoBehaviour
{
    [Header("References")]
    public CarSystem[] carSystems;
    
    public void InitializeLeaderboard(Dictionary<GameObject, CarSystem> inCarSystems)
    {
        carSystems = new CarSystem[inCarSystems.Count];
        int i = 0;
        foreach (var car in inCarSystems)
        {
            carSystems[i] = car.Value;
            i++;
        }

        UpdateLeaderboard();
    }
    public void UpdateLeaderboard()
    {
        Array.Sort(carSystems, (x, y) => x.carPosition.currentPosition.CompareTo(y.carPosition.currentPosition));
    }
}
