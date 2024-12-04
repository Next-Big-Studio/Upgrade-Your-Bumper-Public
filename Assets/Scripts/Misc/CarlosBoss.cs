using Car;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Weapons;

public class CarlosBoss : MonoBehaviour
{

    public delegate IEnumerator BossAttack(); // delegate for coroutines, tbh idk why I did this, I could have just pre-called all the coroutines
    // shut up though. don't judge me.

    public float hp = 100; // health

    List<BossAttack> bossAttackList; // list of attack coroutine delegates

    [Header("Carlos Voicelines")]
    public AudioSource carlosSource; // audio source for carlos' voicelines
    public List<AudioClip> carlosClips; // list of carlos' voicelines
    private List<bool> voicelinePlayedList; // So voicelines don't repeat
    private bool hasIntroEnded = false; // For making sure health is 100% until he stops yapping
    public GameObject terroristCar; // the terrorist car that will be instantiated
    public Transform arenaCenter; // object that keeps track of the center position of the arena
    public float arenaRadius; // radius of the arena

    public float hpDrain; // per second
    public UnityEvent onCarDestroyed;

    public Transform player;

    public Slider hpBar;

    [Header("Terrorist Car Attack")]
    public int numberOfTerroristCarsToSpawn = 5;
    public float terroristCarSpawnHeight = 50;
    public List<AudioClip> goonClips; // Selection of voicelines

    [Header("Gatling Gun Attack")]
    // weapon info is gonna be stored like all other weapon info, look there!

    public float attackDuration; // how long the attack lasts in seconds
    public float numSpins; // how many 360 degree turns he does over the duration of the attack
    public GunWeapon leftGun;
    public GunWeapon rightGun;
    public Animator rightGunAnimator;
    public Animator leftGunAnimator;

    [Header("Roulette Attack")]
    // all of these units are in degrees per second!!!!
    public float startingVelocity; // starting rotational velocity of the roulette
    public float randomVariation; // 0.1 means that the actual starting velocity could be 90%-110% of the value given above.
    public float acceleration; // how much the roullete slows down per second
    public Transform red;
    public Transform black;
    public Transform roulette;
    public float rouletteDamage;

    // Start is called before the first frame update
    void Start()
    {
        voicelinePlayedList = Enumerable.Repeat(false, carlosClips.Count).ToList();
        player = GameManager.Instance.playerSystem.transform;
        bossAttackList = new List<BossAttack>();
        bossAttackList.Add(TerroristCars);
        bossAttackList.Add(GatlingGun);
        bossAttackList.Add(Roulette);
        StartCoroutine(WaitForIntro());
        hpBar.maxValue = hp;
    }
    // summons suicide bomber cars from the sky, they fall and explode on impact with the ground. the player can see their shadow and needs to avoid them
    IEnumerator TerroristCars()
    {
        if(!voicelinePlayedList[6])
        {
            voicelinePlayedList[6] = true;
            PlayVoiceline(6);
        }
        print("I can't possibly describe this attack without sounding racist");
        for (int i = 0; i < numberOfTerroristCarsToSpawn; i++)
        {
            Vector3 spawnPosition = player.position + Vector3.up * 50;
            //Sets random goon voiceline (Only every other 3 cars)
            AudioSource terrCar = Instantiate(terroristCar, spawnPosition, Quaternion.identity).GetComponent<AudioSource>();
            if (i % 3 == 0)
            {
                terrCar.clip = goonClips[Random.Range(0, goonClips.Count)];
                terrCar.PlayDelayed(1.5f);
            }
            yield return new WaitForSeconds(0.3f);
        }
        StartCoroutine(PerformAttack());
    }

    IEnumerator GatlingGun()
    {
        if(!voicelinePlayedList[5])
        {
            voicelinePlayedList[5] = true;
            PlayVoiceline(5);
        }
        print("I'm gatling my shit rn... and by my shit I mean... my gun");
        float degreesToTurnPerSecond = numSpins / attackDuration * 360;
        float timePassed = 0;

        rightGunAnimator.Play("ToGun_Out");
        leftGunAnimator.Play("ToGun_Out");
        //skibidi-taybor

        yield return new WaitForSeconds(3);


        while (timePassed < attackDuration)
        {
            transform.Rotate(0, degreesToTurnPerSecond * Time.deltaTime, 0);
            float adjacent = leftGun.transform.position.y - player.position.y;
            float hypoteneuseL = (leftGun.transform.position - player.position).magnitude;
            float hypoteneuseR = (rightGun.transform.position - player.position).magnitude;
            float thetaL = Mathf.Atan(Mathf.Abs(adjacent / hypoteneuseL));
            float thetaR = Mathf.Atan(Mathf.Abs(adjacent / hypoteneuseR));
            leftGun.transform.parent.localRotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * -thetaL);
            rightGun.transform.parent.localRotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * -thetaR);
            timePassed += Time.deltaTime;
            leftGun.Fire();
            rightGun.Fire();
            yield return new WaitForEndOfFrame();
        }

        rightGunAnimator.Play("ToGun_In");
        leftGunAnimator.Play("ToGun_In");

        yield return new WaitForSeconds(3);

        leftGun.transform.parent.localRotation = Quaternion.Euler(0, 0, 0);
        rightGun.transform.parent.localRotation = Quaternion.Euler(0, 0, 0);

        StartCoroutine(PerformAttack());
    }

    IEnumerator Roulette()
    {
        if(!voicelinePlayedList[7])
        {
            voicelinePlayedList[7] = true;
            PlayVoiceline(7);
        }
        print("Are ya feelin' lucky, chump?");

        roulette.gameObject.SetActive(true);

        float velocity = startingVelocity + Random.Range(0, randomVariation) * startingVelocity;
        while (velocity > 0)
        {
            roulette.Rotate(0, velocity * Time.deltaTime, 0);
            velocity += acceleration * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(3);
        foreach(Transform t in roulette.GetChild(2))
        {
            t.gameObject.SetActive(true);
        }

        if (Vector3.Distance(player.position, red.position) < Vector3.Distance(player.position, black.position))
        {
            // find the closest explosion, deal damage from there
            Vector3 closestExplosionPosition = roulette.GetChild(2).GetChild(0).position;
            float closestDistance = Vector3.Distance(player.position, closestExplosionPosition);

            for (int i = 1; i < 3; i++)
            {
                if ( Vector3.Distance(roulette.GetChild(2).GetChild(i).position, player.position) < closestDistance )
                {
                    closestExplosionPosition = roulette.GetChild(2).GetChild(i).position;
                    closestDistance = Vector3.Distance(roulette.GetChild(2).GetChild(i).position, player.position);
                }
            }
            player.GetComponent<CarSystem>().carDestruction.TakeDamageAtPoint(closestExplosionPosition, player.position - red.position, rouletteDamage);
        }

        yield return new WaitForSeconds(3);

        roulette.gameObject.SetActive(false);

        foreach (Transform t in roulette.GetChild(2))
        {
            t.gameObject.SetActive(false);
        }

        StartCoroutine (PerformAttack());
    }

    private void Update()
    {
        if (!hasIntroEnded)
        {
            hp = 100;
            hpBar.value = hp;
            return;
        }

        hp -= Time.deltaTime * (hpDrain + GameManager.Instance.playerSystem.carMovement.motorRb.velocity.magnitude / 75);
        hpBar.value = hp;

        if(hp <= 75 && !voicelinePlayedList[2])
        {
            voicelinePlayedList[2] = true;
            PlayVoiceline(2);
        }
        if (hp <= 50 && !voicelinePlayedList[3])
        {
            voicelinePlayedList[3] = true;
            PlayVoiceline(3);
        }
        if (hp <= 25 && !voicelinePlayedList[4])
        {
            voicelinePlayedList[4] = true;
            PlayVoiceline(4);
        }
        if (hp <= 0 && !voicelinePlayedList[8])
        {
            voicelinePlayedList[8] = true;
            PlayVoiceline(8);
            
            // Wait until voice line is done
            StartCoroutine(WaitForEndOfVoiceline());
        }
    }
    
    IEnumerator WaitForEndOfVoiceline()
    {
        yield return new WaitForSeconds(carlosClips[8].length);
        onCarDestroyed.Invoke();
    }

    IEnumerator WaitForIntro()
    {
        AudioClip introClip = carlosClips[0];
        AudioClip maxExplainedClip = carlosClips[1];
        PlayVoiceline(0);
        yield return new WaitForSeconds(introClip.length + 0.5f);
        PlayVoiceline(1);
        yield return new WaitForSeconds(maxExplainedClip.length);
        hasIntroEnded = true;
        StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        yield return new WaitForSeconds(5); // wait 5 seconds between an attack
        StartCoroutine(bossAttackList[Random.Range(0, bossAttackList.Count)]()); // you're telling me C# has callbacks!!?? i love delegates
    }

    private void PlayVoiceline(int index)
    {
        carlosSource.clip = carlosClips[index];
        carlosSource.Play();
    }
}
