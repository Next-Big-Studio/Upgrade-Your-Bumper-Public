using Destructors;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CaraqiExplosion : MonoBehaviour
{

    CarDestruction playerDestruction;
    public GameObject explosionParticle;
    public float explosionRange = 50;
    public float damage = 10;
    public Transform nose;

    private void Start()
    {
        transform.rotation = Quaternion.Euler(90, 0, Random.Range(0, 360f));
        playerDestruction = GameManager.Instance.playerSystem.carDestruction;
    }

    private void Update()
    {
        transform.Rotate(0, 0, 360 * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider collision)
    {
        print(collision.name);
        print(collision.tag);
        if (collision.tag == "Caraqi") return;

        GameObject particleInstance = Instantiate(explosionParticle, transform.position, Quaternion.identity);
        particleInstance.GetComponent<AudioSource>().volume = 0.5f;

        if (Vector3.Distance(playerDestruction.transform.position, transform.position) < explosionRange) {
            print("Explosion distance");
            RaycastHit hit;
            Physics.Raycast(nose.position, playerDestruction.transform.position - transform.position, out hit, 10000, 1 << 6);
            if (hit.transform)
            {
                playerDestruction.totalDeformation += damage;
                if (playerDestruction.totalDeformation > playerDestruction.maxDeformation)
                {
                    playerDestruction.carSystem.carStatus.DestroyCar();
                }
            }
            
        }
        Destroy(gameObject);
    }
}
