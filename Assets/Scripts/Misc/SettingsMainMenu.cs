using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
public class SettingsMainMenu : MonoBehaviour
{
    [Header("Settings Menu Panels")]
    public GameObject settingsMenu;
    public GameObject generalMenu;
    public GameObject audioMenu;
    public GameObject controlsMenu;

    [Header("Settings Menu Buttons")]

    public Button settingsButton;
    public Button backButton;
    public Button generalButton;
    public Button audioButton;
    public Button controlsButton;

    [Header("General Settings")]
    public Toggle fullscreenToggle;

    [Header("Audio Mixers")]

    public AudioMixerGroup mainMixer;
    public AudioMixerGroup musicMixer;
    public AudioMixerGroup SFXMixer;
    public AudioMixerGroup menuMixer;
    public AudioMixerGroup ambienceMixer;

    [Header("Audio Scrollbars")]

    public Scrollbar mainScroll;
    public Scrollbar musicScroll;
    public Scrollbar SFXScroll;
    public Scrollbar menuScroll;
    public Scrollbar ambienceScroll;

    private GameObject currentSettingsMenu;

    // Start is called before the first frame update
    void Start()
    {
        //For checking what currentSettingsMenu
        currentSettingsMenu = null;

        settingsButton.onClick.AddListener(OpenSettingsMenu);
        backButton.onClick.AddListener(CloseSettingsMenu);
        generalButton.onClick.AddListener(OpenGeneralMenu);
        audioButton.onClick.AddListener(OpenAudioMenu);
        controlsButton.onClick.AddListener(OpenControlsMenu);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        InitializeSlider(mainScroll, mainMixer, "MainVolume");
        InitializeSlider(musicScroll, musicMixer, "MusicVolume");
        InitializeSlider(SFXScroll, SFXMixer, "SFXVolume");
        InitializeSlider(menuScroll, menuMixer, "MenuVolume");
        InitializeSlider(ambienceScroll, ambienceMixer, "EnvironVolume");

        mainScroll.onValueChanged.AddListener(value => SetMixerVolume(mainMixer, "MainVolume", value));
        musicScroll.onValueChanged.AddListener(value => SetMixerVolume(musicMixer, "MusicVolume", value));
        SFXScroll.onValueChanged.AddListener(value => SetMixerVolume(SFXMixer, "SFXVolume", value));
        menuScroll.onValueChanged.AddListener(value => SetMixerVolume(menuMixer, "MenuVolume", value));
        ambienceScroll.onValueChanged.AddListener(value => SetMixerVolume(ambienceMixer, "EnvironVolume", value));
    }

    void OpenSettingsMenu()
    {
        settingsMenu.SetActive(true);
        OpenGeneralMenu();
    }

    void CloseSettingsMenu()
    {
        if (currentSettingsMenu != null)
        {
            currentSettingsMenu.SetActive(false);
        }
        currentSettingsMenu = null;
        settingsMenu.SetActive(false);
    }

    void OpenGeneralMenu()
    {
        if (currentSettingsMenu != null)
        {
            currentSettingsMenu.SetActive(false);
        }
        generalMenu.SetActive(true);
        currentSettingsMenu = generalMenu;
    }

    void OpenAudioMenu()
    {
        if (currentSettingsMenu != null)
        {
            currentSettingsMenu.SetActive(false);
        }
        audioMenu.SetActive(true);
        currentSettingsMenu = audioMenu;
    }

    void OpenControlsMenu()
    {
        if (currentSettingsMenu != null)
        {
            currentSettingsMenu.SetActive(false);
        }
        controlsMenu.SetActive(true);
        currentSettingsMenu = controlsMenu;
    }

    void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    void InitializeSlider(Scrollbar scrollbar, AudioMixerGroup mixer, string parameter)
    {
        if (mixer.audioMixer.GetFloat(parameter, out float value))
        {
            scrollbar.value = Mathf.InverseLerp(-30, 0, value); // Maps dB to 0-1 for slider
        }
    }

    void SetMixerVolume(AudioMixerGroup mixer, string parameter, float sliderValue)
    {
        float dBValue = Mathf.Lerp(-30, 0, sliderValue); // Maps 0-1 to dB range
        mixer.audioMixer.SetFloat(parameter, dBValue);
    }
}
