using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI weightText;

    [SerializeField]
    TextMeshProUGUI inventoryCountText;

    [SerializeField]
    TextMeshProUGUI itemValueText;

    [SerializeField]
    TextMeshProUGUI balanceText;

    void Start()
    {
        UpdateStats(0f, 0, 0, 0);
    }

    public void UpdateStats(float weight, int itemValue, int inventoryCount, int balance)
    {
        weightText.text = $"Total Weight: {weight} LB";
        itemValueText.text = $"Total Value Of Items: ${itemValue}";
        inventoryCountText.text = $"Items in Inventory: {inventoryCount}";
        balanceText.text = $"Balance: ${balance}";
    }
}
