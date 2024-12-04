using Car.Player;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;   

public class PauseManager : MonoBehaviour
{
    public PlayerControls playerControls;
    //For hiding/showing menu
    [Header("Menu Game Objects")]
    public GameObject pauseMenu;
    public GameObject settingsMenu;

    [Header("Settings Menu Panels")]
    public GameObject generalMenu;
    public GameObject audioMenu;
    public GameObject controlsMenu;

    [Header("Pause UI Elements")]
    public Button resumeButton;
    public Button settingsButton;
    public Button mainMenuButton;
    public Button quitButton;
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

    [HideInInspector]
    public bool isPaused;

    private AudioSource[] _audioSources;
    private GameObject currentSettingsMenu;

    void Start()
    {
        if (resumeButton == null)
        {
            Debug.LogError("Resume Button is not assigned in the Inspector");
            return;
        }
        //Listeners for Pause Menu click events
        resumeButton.onClick.AddListener(PauseInput);
        settingsButton.onClick.AddListener(OpenSettingsMenu);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        quitButton.onClick.AddListener(QuitGame);
        //For checking what currentSettingsMenu
        currentSettingsMenu = null;
        //Listeners for Settings Menu click events
        backButton.onClick.AddListener(BackToPause);
        generalButton.onClick.AddListener(OpenGeneralMenu);
        audioButton.onClick.AddListener(OpenAudioMenu);
        controlsButton.onClick.AddListener(OpenControlsMenu);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        //Finds all Audio Sources in scene to prepare them for pausing
        _audioSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        pauseMenu.SetActive(false); //Should be false by default
        isPaused = false;

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

    public void PauseInput(){
        if (isPaused)
        {
            playerControls.SetMovementControls(true);
            ResumeGame();
        }
        else
        {
            playerControls.SetMovementControls(false);
            PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        foreach(var audioSource in _audioSources)
        {
            audioSource.Pause();
        }
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        if (currentSettingsMenu)
        {
            currentSettingsMenu.SetActive(false);
        }
        foreach(var audioSource in _audioSources)
        {
            audioSource.UnPause();
        }
        isPaused = false;
    }

    void OpenSettingsMenu()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
        currentSettingsMenu = generalMenu;
        OpenGeneralMenu();
    }

    //FIXME: Needs to manage the game state in GameManager
    public void GoToMainMenu()
    {
        ResumeGame();
        GameManager.Instance.PlayerExitsToMainMenu();
    }

    public void QuitGame(){
        GameManager.Instance.OnApplicationQuit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // This stops play mode in the editor
        #endif
    }

    void BackToPause()
    {
        currentSettingsMenu = null;
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    void OpenGeneralMenu()
    {
        currentSettingsMenu.SetActive(false);
        generalMenu.SetActive(true);
        currentSettingsMenu = generalMenu;
    }

    void OpenAudioMenu()
    {
        currentSettingsMenu.SetActive(false);
        audioMenu.SetActive(true);
        currentSettingsMenu = audioMenu;
    }

    void OpenControlsMenu()
    {
        currentSettingsMenu.SetActive(false);
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
