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
    TextMeshProUGUI totalValueText;

    void Start()
    {
        UpdateStats(0f, 0, 0);
    }

    public void UpdateStats(float weight, int totalValue, int inventoryCount)
    {
        weightText.text = $"Total Weight: {weight} LB";
        totalValueText.text = $"Total Value: ${totalValue}";
        inventoryCountText.text = $"Items in Inventory: {inventoryCount}";
    }
}
