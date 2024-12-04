using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IPointerClickHandler, ISubmitHandler
{
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip selectSound;

    // Called when the pointer enters the button area
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySound(hoverSound);
    }

    // Called when the button is selected
    public void OnSelect(BaseEventData eventData)
    {
        PlaySound(hoverSound);
    }

    // Called when the button is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySound(selectSound);
    }

    // Called when the button is submitted (e.g., by pressing the "Submit" button on a gamepad)
    public void OnSubmit(BaseEventData eventData)
    {
        PlaySound(selectSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}