using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Shop
{
    public class ShopObject : MonoBehaviour
    {
        public Button button;
    
        public GameObject icon;
        public new GameObject name;
        public GameObject price;
        public GameObject description;
        public GameObject amount;
        public bool disabled;
        
        public void Start()
        {
            button = GetComponent<Button>();
        }
        
        public void SetDisabled(bool isDisabled)
        {
            disabled = isDisabled;
            
            // Change the color of the button based on if the player can afford it
            // This is a little hacky, but it works, didn't want to use "disabled" because it would make the button
            // un-clickable and I wanted to show the player why they couldn't buy it through a popup
            ColorBlock block = button.colors;
            block.highlightedColor = isDisabled ? Color.red : Color.green;
            button.colors = block;
        }
        
        public void SetValues(Sprite inIcon, string inName, string inPrice, string inAmount, string inDescription = "", bool inDisabled = false)
        {
            icon.GetComponent<Image>().sprite = inIcon;
            name.GetComponent<TextMeshProUGUI>().text = inName;
            price.GetComponent<TextMeshProUGUI>().text = inPrice;
            description.GetComponent<TextMeshProUGUI>().text = inDescription;
            amount.GetComponent<TextMeshProUGUI>().text = inAmount;
            
            SetDisabled(inDisabled);
        }
        
        public void UpdateAmount(string inAmount)
        {
            amount.GetComponent<TextMeshProUGUI>().text = inAmount;
        }
    }
}
