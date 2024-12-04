using UnityEngine;
using TMPro;
using System;

public class CountdownText : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    private float _countDownTime;
    private bool _isEnding;
    private bool _isStarting;

    public void Ending(float inTime)
    {
        _isEnding = true;
        _countDownTime = inTime;
        countdownText.enabled = true;
    }

    public void Starting(float inTime)
    {
        _isStarting = true;
        _countDownTime = inTime;
        countdownText.enabled = true;
    }

    public void UpdateCountdown()
    {
        // If the countdown is not starting or ending, return
        if (!_isEnding && !_isStarting) return;
        
        SetCountdownText(_countDownTime);
        _countDownTime -= Time.deltaTime;
        
        // If the countdown is not over, return
        if (!(_countDownTime <= 0)) return;
        
        _countDownTime = 0;
        _isEnding = false;
        _isStarting = false;
        countdownText.enabled = false;
    }

    private void SetCountdownText(float inText)
    {
        TimeSpan time = TimeSpan.FromSeconds(inText);
        string formattedTime = $"{time.Seconds}:{time.Milliseconds:00}";
        if (_isStarting)
        {
            countdownText.text = "Starting in " + formattedTime;
            return;
        }
        countdownText.text = "Ending in " + formattedTime;
    }
}
