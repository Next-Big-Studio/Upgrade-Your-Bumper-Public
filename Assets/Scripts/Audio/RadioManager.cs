using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

[System.Serializable]
public struct RadioTrack
{
    public AudioClip audioClip;
    public string radioString;
}

public class RadioManager : MonoBehaviour
{

    // Responsible for managing radio system/music in the game - Ricardo T

    public AudioSource musicSource;
    public AudioClip radioStatic;
    public Vector3 _visiblePosition;
    public Vector3 _hiddenPosition;
    public TextMeshProUGUI nowPlayingText;
    public RectTransform textContainer;
    public List<RadioTrack> radioTracks;
    private int currentTrackIndex = 0;
    private float _scrollSpeed = 100f;
    private float _textWidth;
    private float _containerWidth;
    private bool _didRadioStart = false;
    private bool isPlaying = true;
    private bool isRadioVisible = false;
    private Coroutine scrollingCoroutine;

    //Starts with static and switches to first track in List
    void Start(){
        _containerWidth = textContainer.rect.width;
        StartCoroutine(ShowRadioOnStart());
        StartCoroutine(SwitchTrack());
    }

    private IEnumerator ShowRadioOnStart()
    {
        ToggleRadioVisibility();
        yield return new WaitForSeconds(4f);
        _didRadioStart = true;
        ToggleRadioVisibility();
    }

    public void ShowRadio()
    {
        if (_didRadioStart)
        {
            ToggleRadioVisibility();
        }
    }

    public void ToggleRadioVisibility()
    {
        isRadioVisible = !isRadioVisible;
    }

    void Update(){
        UpdateRadioDisplay();
    }

    public void UpdateRadioDisplay()
    {
        if (isRadioVisible)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _visiblePosition, Time.deltaTime * 10f);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _hiddenPosition, Time.deltaTime * 10f);
        }
    }

    //First plays static, blanks/resets screen, and then loads new track and starts scrolling
    private IEnumerator SwitchTrack(){
        musicSource.clip = radioStatic;
        musicSource.Play();
        nowPlayingText.text = "";
        nowPlayingText.rectTransform.localPosition = new Vector3(0, 0, 0);

        yield return new WaitForSeconds(radioStatic.length);
        
        isPlaying = true;
        LoadMusic(radioTracks[currentTrackIndex]);
        scrollingCoroutine = StartCoroutine(PrepareToScroll());
    }

    //Starts track between start and halfway and sets radio text
    private void LoadMusic(RadioTrack track){
        float halfDurationPoint = track.audioClip.length / 2;
        float randomStartTime = Random.Range(0, halfDurationPoint);

        musicSource.clip = track.audioClip;
        musicSource.time = randomStartTime;
        musicSource.Play();
        nowPlayingText.text = track.radioString;
        _textWidth = nowPlayingText.preferredWidth;
    }
    
    public void PlayPreviousTrack(){
        currentTrackIndex = (currentTrackIndex - 1 + radioTracks.Count) % radioTracks.Count;
        StopScrolling();
        StartCoroutine(SwitchTrack());
    }
    public void PlayNextTrack(){
        currentTrackIndex = (currentTrackIndex + 1) % radioTracks.Count;
        StopScrolling();
        StartCoroutine(SwitchTrack());
    }
    public void PausePlay(){
        isPlaying = !isPlaying;
        //If playing, it loads music/text and loads scrolling coroutine
        if(isPlaying){
            LoadMusic(radioTracks[currentTrackIndex]);
            scrollingCoroutine = StartCoroutine(PrepareToScroll());
        }
        //If not playing, stops music, scrolling and blanks text
        else{
            musicSource.Stop();
            StopScrolling();
            nowPlayingText.rectTransform.localPosition = new Vector3(0, 0, 0);
            nowPlayingText.text = "";
        }
    }
    //Method that waits to scroll at the beginning to see start of string on radio (like in real life)
    private IEnumerator PrepareToScroll()
    {
        yield return new WaitForSeconds(2.5f);
        scrollingCoroutine = StartCoroutine(ScrollingText());
    }
    
    // Simulates scrolling by moving text through its container
    private IEnumerator ScrollingText()
    {
        //Checks if end of string has reached the end of the right side of radio screen
        //If not, it keeps scrolling to the left
        while (nowPlayingText.rectTransform.localPosition.x + _textWidth >= _containerWidth)
        {
            nowPlayingText.rectTransform.localPosition += Vector3.left * _scrollSpeed * Time.deltaTime;
            yield return null;
        }
        yield return StartCoroutine(ResetTextPosition());
    }

    //Once end of string has reached end of right of radio, it waits couple seconds
    //then resets text and starts the cycle again
    private IEnumerator ResetTextPosition()
    {
        yield return new WaitForSeconds(2.5f);
        //Resets text to start of radio screen
        nowPlayingText.rectTransform.localPosition = new Vector3(0, 0, 0);
        
        scrollingCoroutine = StartCoroutine(PrepareToScroll());
    }

    //Helper method to stop scrolling text
    //Necessary for switching songs or stopping the music
    private void StopScrolling()
    {
        if (scrollingCoroutine != null)
        {
            StopCoroutine(scrollingCoroutine);
            scrollingCoroutine = null;
        }
    }
}
