using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddRewardForReachingTarget : MonoBehaviour
{
    public Transform target;
    public RacingMlAgent agent;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == target)
        {
            //agent.HitCheckPoint();
        }
    }
}
