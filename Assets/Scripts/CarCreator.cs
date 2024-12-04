using System.Collections.Generic;
using UnityEngine;
using Car;
using Saving_System;

public class CarCreator : MonoBehaviour
{
	public List<CarInfo> baseCars;
	private GameObject[] _startPositions;

	public GameObject arrowPrefab; // for bounty

	private void Awake()
	{
		GameObject startPositionsParent = GameObject.Find("Start Positions");
		_startPositions = new GameObject[startPositionsParent.transform.childCount];
		
		for (int i = 0; i < startPositionsParent.transform.childCount; i++)
		{
			_startPositions[i] = startPositionsParent.transform.GetChild(i).gameObject;
		}
	}

    public GameObject CreateCar(int baseCar, int startPosition, string carName, List<UpgradeData> upgradesToAdd, List<WeaponData> weaponsToAdd, bool isBounty)
    {
	    // Create the car
        GameObject car = Instantiate(baseCars[baseCar].carPrefab, _startPositions[startPosition].transform.position, Quaternion.identity);
        car.name = carName;
        
        // Add the upgrades
        car.GetComponent<CarInventory>().upgrades = upgradesToAdd;
        
        // Add the weapons
        car.GetComponent<CarInventory>().weapons = weaponsToAdd;
        
		// Add the marking arrow if the car is the bounty
		if (isBounty)
    	{
        	GameObject arrow = Instantiate(arrowPrefab, car.transform, true);
	        arrow.transform.localPosition = new Vector3(0, 15.0f, 0); // Adjust Y position as needed
    	}
		return car;
	}
		
}
