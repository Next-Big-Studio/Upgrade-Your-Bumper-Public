using TMPro;
using UI.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Shop.Panels
{
    public sealed class RepairPanel : UIPanel
    {

        public UnityEvent onRepair;
        [SerializeField] private GameObject repairButton;
        [SerializeField] private TextMeshProUGUI repairText;
        public int costToRepair; // The cost to repair the player

        private RepairPanel()
        {
            panelName = "repair";
        }
        
        private new void Awake()
        {
            base.Awake();
            
            // Set the repair button and add a listener to it
            repairButton.GetComponent<Button>().onClick.AddListener(RepairPlayer);
            
            // Set the repair text and set the text
            repairText.text = $"Repair: {costToRepair}";
            
            Hide();
        }
        
        public void SetRepairCost(int cost)
        {
            costToRepair = cost;
            repairText.text = $"Repair: {costToRepair}";
            Refresh();
        }
        
        private new void Refresh()
        {
            Button rButton = repairButton.GetComponent<Button>();

            // change color of button based on if the player can afford it
            ColorBlock block = rButton.colors;
            block.highlightedColor = rButton.interactable ? Color.green : Color.red;
            rButton.colors = block;
        }
        
        // Repair the player
        private void RepairPlayer()
        {
            
            if ( ShopManager.instance.carInventory.currency < costToRepair)
            {
                ShopManager.instance.PopupMessage(ShopError.CannotRepair);
                return;
            }

            ShopManager.instance.playerSystem.carStatus.isDestroyed = false;
            ShopManager.instance.carInventory.currency -= costToRepair; // Subtract the repair cost from the player's currency
            ShopManager.instance.RefreshCurrencyUI(); // Update the currency UI
            ShopManager.instance.PopupMessage(ShopError.Repaired);
            
            onRepair.Invoke(); // Invoke the onRepair event
        }
    }
}