using UnityEngine;
using UnityEngine.UI;

namespace UI.Framework
{
    public class UITab : MonoBehaviour
    {
        [SerializeField] public UIPanel panel;
        [SerializeField] public Button tabButton;
        public string tabName;
        
        public void SelectTab()
        {
            panel.ToggleSelected(true);
            
            // change color of button
            ColorBlock block = tabButton.colors;
            block.normalColor = Color.grey;
            tabButton.colors = block;
        }
        
        public void DeselectTab()
        {
            panel.ToggleSelected(false);
            
            // change color of button
            ColorBlock currentBlock = tabButton.colors;
            currentBlock.normalColor = Color.white;
            tabButton.colors = currentBlock;
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}