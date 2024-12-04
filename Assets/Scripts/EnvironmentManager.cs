using NUnit.Framework;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using RenderSettings = UnityEngine.RenderSettings;

public class EnvironmentManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject lightRainPrefab; // Reference to the LightRain prefab
    public GameObject heavyRainPrefab; // Reference to the HeavyRain prefab
    public GameObject fogPrefab; // Reference to the Fog prefab
 
    [Header("References")]
    public GameObject rainInstance; // Reference to the rain instance
    public GameObject fogInstance; // Reference to the fog instance
    public GameObject weatherPositions; //Reference to WeatherPositions GameObjects

    [Header("Skyboxes")]
    public Material daySkybox;         // Daytime skybox
    public Material nightSkybox;       // Nighttime skybox
    public Material dayCloudySkybox;   // Daytime cloudy skybox
    public Material nightCloudySkybox; // Nighttime cloudy skybox

    // Rather than calling this on start, we can call it from the GameManager
    // This is due to the fact that the EnvironmentManager is not always active, and we can always create a new one in case we want to guarantee that it's going to rain or whatever

    [Header("Audio")]
    public AudioSource rainSource;
    public AudioClip rainSound;

    [Header("States")]
    public bool isRaining  {get; private set;}

    void Awake(){
        rainSource.clip = rainSound;
        isRaining = false;
    }

    private bool RollForTime()
    {
        return Random.value < 0.5f; // 50% chance for day (true) or night (false)
    }

    public void RollForWeather()
    {
        float roll = Random.value;

        Transform rainPosition = weatherPositions.transform.Find("Rain");
        Transform fogPosition = weatherPositions.transform.Find("Fog");

        bool isDayTime = RollForTime(); // Day if true, night if false.

        if (Random.value < 0.5f) // 50% chance of light rain
        {
            rainInstance = Instantiate(lightRainPrefab, rainPosition.position, Quaternion.identity);
            isRaining = true;

            if(roll < .25f) // 25% chance of fog when it does rain
            {
                fogInstance = Instantiate(fogPrefab, fogPosition.position, Quaternion.identity);
            }
            
            RenderSettings.skybox = isDayTime ? dayCloudySkybox : nightCloudySkybox;
        }
        else if (roll >= 0.5f && roll < 0.9f) // 40% chance of fog, if it does not light rain
        {
            fogInstance = Instantiate(fogPrefab, fogPosition.position, Quaternion.identity);
            
            RenderSettings.skybox = isDayTime ? dayCloudySkybox : nightCloudySkybox;
        }
        else if (roll <= 0.3f) // 30% chance of heavy rain, if it does not light rain
        {
            rainInstance = Instantiate(heavyRainPrefab, rainPosition.position, Quaternion.identity);
            isRaining = true;
            RenderSettings.skybox = isDayTime ? dayCloudySkybox : nightCloudySkybox;
        }
        else{
            RenderSettings.skybox = isDayTime ? daySkybox : nightSkybox;
            ToggleLights(isDayTime);
        }

        if(isRaining)
        {
            rainSource.Play();
        }
        
        Light directionalLight = GameObject.Find("Directional Light").GetComponent<Light>();
        directionalLight.shadows = isDayTime ? LightShadows.Soft : LightShadows.None; // Set shadow type
        directionalLight.shadowStrength = isDayTime ? 1 : 0; // Set shadow strength
        directionalLight.intensity = isDayTime ? 0.7f : 0.2f; // Set light intensity
        
    }

    private void ToggleLights(bool isDayTime)
    {
        // Searching for "Light" tag..
        GameObject[] streetLights = GameObject.FindGameObjectsWithTag("Light");

        foreach (GameObject lightPost in streetLights)
        {
            Light lightComponent = lightPost.GetComponentInChildren<Light>();
            if (lightComponent != null)
            {
                lightComponent.enabled = !isDayTime;
            }
        }
    }
}
