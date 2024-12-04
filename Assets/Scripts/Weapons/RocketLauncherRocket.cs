using Car;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class RocketLauncherRocket : MonoBehaviour
{
    public SplineContainer spline;
    public GameObject explosionParticle;
    public Transform target;
    public float damage;

    // Start is called before the first frame update
    public void StartFollow(Transform target)
    {
        this.target = target;
        StartCoroutine(InitPosition());
    }

    IEnumerator InitPosition()
    {
        float3 startPos = transform.position;
        float3 endpos;
        
        

        SplineUtility.GetNearestPoint(spline.Spline, spline.transform.InverseTransformPoint(transform.position), out endpos, out float t, 5, 3);

        endpos = spline.transform.TransformPoint(endpos);
        print("Starting t: " + t);
        // this will take 50/50 = 1 seconds to get the rocket in its initial position
        for (float i = 0; i <= 50; i++)
        {
            transform.position = Vector3.Lerp(startPos, endpos, i/50);
            transform.rotation = Quaternion.LookRotation(((Vector3)endpos - transform.position).normalized, Vector3.up) * Quaternion.Euler(360f/50, 0, 0);
            
            yield return new WaitForFixedUpdate();
        }

        // rocket should now start following

        float tMissile;
        SplineUtility.GetNearestPoint(spline.Spline, transform.position, out _, out tMissile);

        while (Vector3.Distance(transform.position, target.position) > 50)
        {
            float tTarget;
            tMissile += 0.002f;
            if (tMissile > 1)
            {
                tMissile -= 1;
            }
            SplineUtility.GetNearestPoint(spline.Spline, target.position, out _, out tTarget);

            print("Target: " + tTarget + " Missile: " + tMissile);

            int direction = 1;

            Vector3 nextPos = spline.EvaluatePosition(tMissile + 0.002f * direction <= 1f ? tMissile + 0.002f * direction : tMissile + 0.002f - 1);
            
            Quaternion rotation = Quaternion.LookRotation((nextPos - transform.position).normalized, Vector3.up) * Quaternion.Euler(360f/50, 0, 0);
            transform.rotation = rotation;
            transform.position = nextPos;

            print("My T: " + tMissile);

            yield return new WaitForFixedUpdate();
        }

        // rocket is close, time to home in for the kill

        startPos = transform.position;

        for (int i = 0; i < 10; i++)
        {
            transform.position = Vector3.Lerp(startPos, target.position, i / 10f);
            transform.rotation = Quaternion.LookRotation((target.position - transform.position).normalized, Vector3.up);
            yield return new WaitForFixedUpdate();
        }

        // time to explode!
        Instantiate(explosionParticle, transform.position, Quaternion.identity);

        foreach(CarSystem system in GameManager.Instance.raceManager.leaderboard.carSystems)
        {
            if (Vector3.Distance(system.carPosition.transform.position, transform.position) < 50)
            {
                system.carDestruction.TakeDamageAtPoint(transform.position, (system.carPosition.transform.position - transform.position).normalized, damage);
            }
        }

        Destroy(gameObject);
    }
}
