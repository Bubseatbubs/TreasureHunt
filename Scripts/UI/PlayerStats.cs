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
        UpdateStats(0.0, 0, 0, 0);
    }

    public void UpdateStats(double weight, double itemValue, int inventoryCount, double balance)
    {
        weightText.text = $"Weight: {weight} LB";
        itemValueText.text = $"Value Of Items: ${itemValue}";
        inventoryCountText.text = $"Items in Inventory: {inventoryCount}";
        balanceText.text = $"Balance: ${balance}";
    }

    public void ResetStats()
    {
        UpdateStats(0.0, 0, 0, 0);
    }
}
