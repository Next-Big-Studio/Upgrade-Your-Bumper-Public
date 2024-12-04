using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class MenuNavigation : MonoBehaviour
{
   //Reselects a default button if no button is selected when detecting gamepad/keyboard input
   [SerializeField] private GameObject defaultSelectedButton;
   void Start()
   {
       EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
   }


   void Update()
   {
       //ChatGPT - Added check for gamepad and keyboard input
       bool gamepadActive = Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;
       bool keyboardActive = Keyboard.current != null &&
                             (Keyboard.current.wKey.wasPressedThisFrame ||
                              Keyboard.current.aKey.wasPressedThisFrame ||
                              Keyboard.current.sKey.wasPressedThisFrame ||
                              Keyboard.current.dKey.wasPressedThisFrame ||
                              Keyboard.current.upArrowKey.wasPressedThisFrame ||
                              Keyboard.current.downArrowKey.wasPressedThisFrame ||
                              Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                              Keyboard.current.rightArrowKey.wasPressedThisFrame);
      
       if((gamepadActive || keyboardActive) && EventSystem.current.currentSelectedGameObject == null)
       {
           EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
       }
   }
}
