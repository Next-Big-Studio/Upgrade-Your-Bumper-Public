using TMPro;
using UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Shop
{
    public class StatsObject : MonoBehaviour
    {
        public StatData StatData;
        [SerializeField] public Slider slider;
        [SerializeField] public TextMeshProUGUI statDisplayName;
        
        
        public void SetValues(StatData statData) {
            StatData = statData;
            
            slider.maxValue = statData.maxValue;
            slider.value = statData.Value;
            statDisplayName.text = statData.displayName;
            
            SetColor();
        }
        
        public void SetValue(float value)
        {
            slider.value = value;
            SetColor();
        }
        
        public void SetMaxValue(float value)
        {
            slider.maxValue = value;
            SetColor();
        }

        private void SetColor()
        {
            // Green Yellow Orange Red
            if (slider.value < slider.maxValue / 4)
            {
                slider.fillRect.GetComponent<Image>().color = Color.red;
            }
            else if (slider.value < slider.maxValue / 2)
            {
                slider.fillRect.GetComponent<Image>().color = new Color(1, 0.5f, 0);
            }
            else if (slider.value < slider.maxValue * 3 / 4)
            {
                slider.fillRect.GetComponent<Image>().color = Color.yellow;
            }
            else
            {
                slider.fillRect.GetComponent<Image>().color = Color.green;
            }
            
        }
    }
}

public struct StatData
{
    public readonly string name;
    public readonly string displayName;
    public float maxValue;
    public float Value;
    public float baseValue;

    public StatData(string name, string displayName, float maxValue, float value, float baseValue)
    {
        this.name = name;
        this.displayName = displayName;
        this.maxValue = maxValue;
        Value = value;
        this.baseValue = baseValue;
    }

    public void SetValue(float inValue)
    {
        Value = inValue;
    }
    
    public void SetBaseValue(float inValue)
    {
        baseValue = inValue;
    }
    
    public void SetMaxValue(float inValue)
    {
        maxValue = inValue;
    }
}