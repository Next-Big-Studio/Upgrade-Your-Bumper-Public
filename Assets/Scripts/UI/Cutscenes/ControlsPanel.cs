using System.Linq;
using TMPro;
using UI.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Cutscenes
{
    public class ControlsPanel : UIPanel
    {
        [SerializeField] private InputActionAsset inputActionAsset;
        [SerializeField] private GameObject gamepadControls;
        [SerializeField] private GameObject keyboardControls;
        [SerializeField] private TextMeshProUGUI controlSchemeText;
        [SerializeField] private Button switchButton;
        private string _controlScheme = "Keyboard";
        
        // =============================================================================================================
        // ============================================= EVENT FUNCTIONS ==============================================
        // =============================================================================================================
        private void OnEnable()
        {
            // Subscribe to device change events
            InputSystem.onDeviceChange += OnDeviceChange;
            
            // Set initial control text based on current control scheme
            SetControlsObject();
            
            switchButton.onClick.AddListener(() =>
            {
                _controlScheme = _controlScheme == "Gamepad" ? "Keyboard" : "Gamepad";
                SetControlsObject();
            });
            
            // Show the controls panel
            Show();
        }
        
        private void OnDisable()
        {
            // Unsubscribe from device change events
            InputSystem.onDeviceChange -= OnDeviceChange;
        }
        
        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            // Only update if a new device is added or removed
            if (change is not (InputDeviceChange.Added or InputDeviceChange.Removed)) return;
            
            _controlScheme = Gamepad.current != null ? "Gamepad" : "Keyboard";
            SetControlsObject();
        }
        
        private void Start()
        {
            // Set initial controls text
            SetControlsObject();
        }
        
        // =============================================================================================================
        // ============================================= PRIVATE FUNCTIONS ============================================
        // =============================================================================================================

        private void SetControlsObject()
        {
            controlSchemeText.text = _controlScheme + " Controls";

            switch (_controlScheme)
            {
                case "Gamepad":
                    gamepadControls.SetActive(true);
                    keyboardControls.SetActive(false);
                    break;
                case "Keyboard":
                    keyboardControls.SetActive(true);
                    gamepadControls.SetActive(false);
                    break;  
            }
        }
    }
}
