using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Upgrades;
using Saving_System;

public class UpgradeText : MonoBehaviour
{
    public GameObject upgradeTextObject;
    public TextMeshProUGUI upgradeText;

    private Dictionary<Rarity, Color> rarityColors = new Dictionary<Rarity, Color>
    {
        {Rarity.Common, Color.white},
        {Rarity.Uncommon, Color.green},
        {Rarity.Rare, Color.blue},
        {Rarity.Epic, Color.magenta},
        {Rarity.Legendary, new Color(1f, 69f/255f, 0)}
    };

    public void ShowUpgrade(Upgrade upgrade)
    {
        string text = UpgradeToString(upgrade);
        Color color = rarityColors[upgrade.rarity];
        upgradeTextObject.SetActive(true);
        StartCoroutine(ShowUpgradeText(text, color));
    }

    private string UpgradeToString(Upgrade upgrade)
    {
        return $"{upgrade.name}\n{upgrade.description}";
    }

    private IEnumerator ShowUpgradeText(string upgrade, Color color)
    {
        UpdateUpgradeText(upgrade, color);
        yield return new WaitForSeconds(4);
        upgradeTextObject.SetActive(false);
    }

    private void UpdateUpgradeText(string upgrade, Color color)
    {
        upgradeTextObject.SetActive(true);
        upgradeText.text = upgrade;
        upgradeText.color = color;
    }

    private void UpdateUpgradeText(string upgrade)
    {
        upgradeTextObject.SetActive(true);
        upgradeText.text = upgrade;
    }
}
