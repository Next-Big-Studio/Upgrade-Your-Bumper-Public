using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    public float minIntensity = 0.5f;      // Minimum intensity of the light
    public float maxIntensity = 1.5f;      // Maximum intensity of the light
    public float minFlickerDelay = 0.5f;   // Minimum time between flickers
    public float maxFlickerDelay = 2.0f;   // Maximum time between flickers

    private Light lightSource;
    private float nextFlickerTime;

    void Start()
    {
        lightSource = GetComponent<Light>();
        ScheduleNextFlicker();
    }

    void Update()
    {
        // Check if it's time to flicker
        if (Time.time >= nextFlickerTime)
        {
            // Instantly change the light intensity to create a flicker effect
            lightSource.intensity = Random.Range(minIntensity, maxIntensity);

            // Schedule the next flicker
            ScheduleNextFlicker();
        }
    }

    void ScheduleNextFlicker()
    {
        // Set the next flicker time randomly within the specified range
        nextFlickerTime = Time.time + Random.Range(minFlickerDelay, maxFlickerDelay);
    }
}
