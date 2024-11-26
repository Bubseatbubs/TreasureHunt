using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField]
    private GameObject itemPrefab;
    private static Item itemTemplate;
    public static Dictionary<int, Item> items = new Dictionary<int, Item>();
    public static ItemManager instance;

    /*
    Singleton Pattern: Make sure there's only one PlayerManager
    */
    void Awake()
    {
        if (instance)
        {
            // Remove if instance exists
            Destroy(gameObject);
            return;
        }

        // No instance yet, set this to it
        instance = this;
        itemTemplate = itemPrefab.GetComponent<Item>();
    }

    public static void CreateNewItem(int id, Vector2 spawnPosition)
    {
        if (items.ContainsKey(id))
        {
            // Item already exists
            Debug.Log($"An item with ID {id} already exists.");
            return;
        }
        else
        {
            // Instantiate a new player
            Item item = Instantiate(itemTemplate, spawnPosition, Quaternion.identity);
            item.AssignID(id);
            items.Add(id, item);
            Debug.Log($"Create item object with ID of {id}");
        }
    }

    public string SendItemPositions()
    {
        string response = "PlayerManager:UpdatePlayerPositions:";
        foreach (KeyValuePair<int, Item> i in items)
        {
            response += i.Key + "|" + i.Value.GetXPosition() + "|" + i.Value.GetYPosition() + "/";
        }

        return response;
    }

    public static void UpdateItemPosition(int id, Vector2 pos)
    {
        if (!items.ContainsKey(id))
        {
            Debug.Log("Adding item " + id);
            CreateNewItem(id, pos);
        }
        
        items[id].SetPosition(pos);
    }

    public static void UpdateItemPositions(string message)
    {
        string[] itemData = message.Split('/');
        for (int i = 0; i < itemData.Length - 1; i++)
        {
            string[] currentItemData = itemData[i].Split('|');
            int id = int.Parse(currentItemData[0]);
            Vector2 pos = new Vector2(float.Parse(currentItemData[1]), float.Parse(currentItemData[2]));
            UpdateItemPosition(id, pos);
        }
    }

    public static void HideItem(int itemID) {
        items[itemID].HideItem();
    }
}
