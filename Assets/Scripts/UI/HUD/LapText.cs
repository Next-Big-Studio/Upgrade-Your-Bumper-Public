using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Car;

public class LapText : MonoBehaviour
{
    public int maxLaps;
    public TextMeshProUGUI lapText;
    private CarPosition _playerPosition;

    public void Initialize(CarSystem carSystem)
    {
        _playerPosition = carSystem.carPosition;
    }

    public void UpdateLapText()
    {
        lapText.text = $"{_playerPosition.currentLap}/{maxLaps}";
    }
}
