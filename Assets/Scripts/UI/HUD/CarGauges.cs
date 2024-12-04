using System.Collections;
using System.Collections.Generic;
using Car;
using Destructors;
using UnityEngine;

public class CarGauges : MonoBehaviour
{
    [Header("Speedometer")]
    public RectTransform speedNeedleTransform;
    public float maxSpeed;
    public float minSpeedAngle;
    public float maxSpeedAngle;

    [Header("Tachometer")]
    public RectTransform RPMNeedleTransform;
    public float maxRPM;
    public float minRPMAngle;
    public float maxRPMAngle;

    [Header("Fuel Meter")]
    public RectTransform fuelNeedleTransform;
    public float minFuelAngle;
    public float maxFuelAngle;

    [Header("Low Health Text")]
    public GameObject lowHealthText;
    public AudioSource sfxSource;
    public AudioClip beepSound;
    private bool _isFlashed;

    private CarMovement _playerMovement;
    private CarDestruction _playerDestructor;

    public void Initialize(CarSystem carSystem)
    {
        _playerMovement = carSystem.carMovement;
        _playerDestructor = carSystem.carDestruction;
        maxRPM = carSystem.carStats.maxSpeed;
    }

    public void UpdateGauges()
    {
        // Speed Needle calculation
        float clampedSpeed = Mathf.Clamp(_playerMovement.motorRb.velocity.magnitude, 0f, maxSpeed);
        float speedNeedleAngle = Mathf.Lerp(minSpeedAngle, maxSpeedAngle, clampedSpeed / maxSpeed);
        speedNeedleTransform.localEulerAngles = new Vector3(0, 0, speedNeedleAngle);

        // RPM Needle calculation
        float clampedRpm = Mathf.Clamp(_playerMovement.moveForce, 0f, maxRPM);
        float RPMNeedleAngle = Mathf.Lerp(minRPMAngle, maxRPMAngle, clampedRpm / maxRPM);
        RPMNeedleTransform.localEulerAngles = new Vector3(0, 0, RPMNeedleAngle);

        // Health/Fuel Calculation
        float clampedFuel = Mathf.Clamp(_playerDestructor.totalDeformation, 0f, _playerDestructor.maxDeformation);
        float playerPercentDamage = clampedFuel / _playerDestructor.maxDeformation;
        float fuelNeedleAngle = Mathf.Lerp(minFuelAngle, maxFuelAngle, playerPercentDamage);
        fuelNeedleTransform.localEulerAngles = new Vector3(0, 0, fuelNeedleAngle);

        if (playerPercentDamage >= 0.5f && !_isFlashed)
        {
            _isFlashed = true;
            StartCoroutine(FlashLowHealthText());
        }
    }

    private IEnumerator FlashLowHealthText()
    {
        for (int i = 0; i < 6; i++)
        {
            lowHealthText.SetActive(!lowHealthText.activeSelf);
            sfxSource.PlayOneShot(beepSound, 0.3f);
            yield return new WaitForSeconds(0.5f);
        }

        lowHealthText.SetActive(false);
    }
    
}

