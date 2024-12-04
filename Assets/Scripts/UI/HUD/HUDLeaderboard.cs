using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Car;

public class HUDLeaderboard : MonoBehaviour
{
    public CarLeaderboard leaderboard;
    public TextMeshProUGUI first;
    public TextMeshProUGUI second;
    public TextMeshProUGUI third;
    public TextMeshProUGUI fourth;


    public void UpdateLeaderboard()
    {
        first.text = leaderboard.carSystems[0].carName.Split(' ')[0];
        first.color = leaderboard.carSystems[0].color;
        second.text = leaderboard.carSystems[1].carName.Split(' ')[0];
        second.color = leaderboard.carSystems[1].color;
        third.text = leaderboard.carSystems[2].carName.Split(' ')[0];
        third.color = leaderboard.carSystems[2].color;
        fourth.text = leaderboard.carSystems[3].carName.Split(' ')[0];
        fourth.color = leaderboard.carSystems[3].color;
    }
}
