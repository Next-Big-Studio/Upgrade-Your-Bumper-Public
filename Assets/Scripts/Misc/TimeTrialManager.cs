using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrialManager : MonoBehaviour
{
    public static TimeTrialManager Instance;
    public ElapsedTime timer;

    private void Start()
    {
        GameManager.Instance.raceManager.raceFinishing.AddListener(EndTrial);
        // prevent taking damage on this track
        GameManager.Instance.playerSystem.carDestruction.deformationPerImpulse = 0;
    }

    void EndTrial(float _)
    {
        GameManager.Instance.playedTimeTrial = true;
        GameManager.Instance.wonTimeTrial = true;
        GameManager.Instance.LoadThisScene("Shop");
    }
    
    void LoseTrial()
    {
        GameManager.Instance.playedTimeTrial = true;
        GameManager.Instance.wonTimeTrial = false;
        GameManager.Instance.LoadThisScene("Shop");
    }

    private void Update()
    {
        if (timer.time > 50)
        {
            LoseTrial();
        }
    }
}
