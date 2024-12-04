using Car;
using Destructors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Weapons;

public class RocketLauncherFirerer : MonoBehaviour, IWeaponFirerer
{

    public GameObject rocketPrefab;
    public Transform spawnPos;

    public AudioSource shootSound;

    // if this is true, the car shoots ONLY the player.
    // used for AI, who will obviously not shoot their teammates
    bool willShootPlayer;

    public void Fire(Vector3 direction, WeaponInfo weaponInfo)
    {
        // find target
        // the target will be the closest car to the player

        Transform target;
        if (willShootPlayer)
        {
            target = GameManager.Instance.playerSystem.carPosition.transform;
        } else
        {
            List<CarSystem> carsBesidesFirerer = new List<CarSystem>(GameManager.Instance.raceManager.leaderboard.carSystems);
            carsBesidesFirerer.Remove(transform.parent.GetComponent<CarDestruction>().carSystem);


            CarSystem closestCar = carsBesidesFirerer[0];
            float closestDistance = Vector3.Distance(closestCar.transform.position, transform.position);
            foreach (CarSystem system in carsBesidesFirerer)
            {
                if (closestDistance > Vector3.Distance(transform.position, system.transform.position))
                {
                    closestCar = system;
                    closestDistance = Vector3.Distance(transform.position, system.transform.position);
                }
            }
            target = closestCar.transform;
        }
        shootSound.Play();
        GameObject rocket = Instantiate(rocketPrefab, spawnPos.position, Quaternion.LookRotation(direction, Vector3.up));
        rocket.GetComponent<RocketLauncherRocket>().spline = GameObject.Find("Checkpoints").GetComponent<SplineContainer>();
        rocket.GetComponent<RocketLauncherRocket>().damage = weaponInfo.damage;
        rocket.GetComponent<RocketLauncherRocket>().StartFollow(target);
    }
}
