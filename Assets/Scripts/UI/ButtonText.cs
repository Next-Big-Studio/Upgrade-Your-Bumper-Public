using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem.Interactions;

public class ButtonHoverEffect : MonoBehaviour
{
    public TextMeshProUGUI targetText;  // The text to change color
    public Color originalColor;
    public Color hoverColor;

    void Start()
    {
        if (targetText == null)
        {
            Debug.LogError("Target TextMeshProUGUI is not assigned.");
            return;
        }

        originalColor = targetText.color;

        // Add EventTrigger component
        EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();

        // Add pointer enter (hover) event
        EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnterEntry.callback.AddListener((eventData) => OnHover());
        eventTrigger.triggers.Add(pointerEnterEntry);

        // Add pointer exit event
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        pointerExitEntry.callback.AddListener((eventData) => OnHoverExit());
        eventTrigger.triggers.Add(pointerExitEntry);

        // Add pointer click event
        EventTrigger.Entry pointerClickEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        pointerClickEntry.callback.AddListener((eventData) => OnClick());
        eventTrigger.triggers.Add(pointerClickEntry);
    }

    void OnHover()
    {
        targetText.color = hoverColor;
    }

    void OnHoverExit()
    {
        targetText.color = originalColor;
    }

    void OnClick()
    {
        targetText.color = originalColor;
    }
}
