using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Upgrades;
using Car;
using Saving_System;
using Destructors;

public class MysteryBox : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject mysteryBoxPrefab;
    public GameObject particleEffect;

    [Header("Audio")]
    public AudioClip bell;
    public AudioSource audioSource;

    void Awake(){
        audioSource.clip = bell;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        GetComponent<Collider>().enabled = false;
        Destroy(transform.parent.gameObject);
        Instantiate(particleEffect, transform.position, transform.rotation);

        // We only play the audio if it is the players car entering, to ensure we do not hear it for AI cars.
        if(other.transform.root.gameObject.GetComponent<CarDestruction>().carSystem.carName == "Max V."){
            audioSource.Play();
        }
        
        Upgrade randomUpgrade = GameManager.Instance.allUpgrades[Random.Range(0, GameManager.Instance.allUpgrades.Count)];
        randomUpgrade.isTemporary = true;

        // We need to get a UpgradeData array 
        UpgradeData randomupgradeData = new UpgradeData(randomUpgrade.id, 1, randomUpgrade.price);
        
        CarDestruction carDestruction = other.transform.root.gameObject.GetComponent<CarDestruction>();

        carDestruction.carSystem.carStats.ApplyUpgrade(randomUpgrade);
        // carDestruction.carSystem.carStats.ApplyVisualUpgrades(new[] { randomupgradeData }); 
        carDestruction.carSystem.mysteryBoxEvent.Invoke(randomUpgrade);
    }
}