using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Misc
{
    public class HideStuff : MonoBehaviour
    {
        [SerializeField] private GameObject toHide;
        private Button _button;
        private readonly Color _showColor = new(0.5f, 0.5f, 0.5f, 1f);
        private readonly Color _hideColor = new(1f, 1f, 1f, 1f);

        private void Start()
        {
            _button = gameObject.GetComponent<Button>();
            _button.onClick.AddListener(ToggleVisibility);
        }

        private void ToggleVisibility()
        {
            _button.GetComponentInChildren<TextMeshProUGUI>().text = toHide.activeSelf ? "Show" : "Hide";
            gameObject.GetComponent<Image>().color = toHide.activeSelf ? _showColor : _hideColor;
            toHide.SetActive(!toHide.activeSelf);
        }
    }
}
