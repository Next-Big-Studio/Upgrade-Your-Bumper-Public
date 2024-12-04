using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ElapsedTime : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    [HideInInspector] public float time = 0;

    public void UpdateTime()
    {
        time += Time.deltaTime;
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt(time * 1000 % 1000);
        timerText.text = $"{minutes:00}:{seconds:00}:{milliseconds:000}";
    }
}
