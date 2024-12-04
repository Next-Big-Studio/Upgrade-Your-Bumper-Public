using Car;
using Misc;
using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class AggressiveAITrainingEnvironment : MonoBehaviour
{

    // trains aggressive AI by spawning targets around the map.

    public GameObject targetPrefab;
    public SplineContainer spline;
    public float spawnTime;
    public int maxTargetsOnTrack;
    public Queue<GameObject> targetQueue = new Queue<GameObject>();

    public Transform checkpointParent;
    public Transform[] checkpoints;

    private void Start()
    {
        checkpoints = new Transform[checkpointParent.childCount];
        for(int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i] = checkpointParent.GetChild(i);
        }
        for (int i = 0; i < maxTargetsOnTrack; i++) {
            SpawnTarget();
        }
        InvokeRepeating("SpawnTarget", 0, spawnTime);
        
    }

    void SpawnTarget()
    {
        if (targetQueue.Count >= maxTargetsOnTrack)
            Destroy(targetQueue.Dequeue());

        // the whole 2nd argument is just random position along the first 3/4ths of the spline (where the AI will spend most of its time) plus some random offset between sqrt(2)-5sqrt(2) units away on the X and Z axes
        GameObject newTarget = Instantiate(targetPrefab, (Vector3) spline.EvaluatePosition(Random.Range(0, 0.75f)) + new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)),  Quaternion.identity);
        newTarget.tag = "Player";
        targetQueue.Enqueue(newTarget);
    }

}
