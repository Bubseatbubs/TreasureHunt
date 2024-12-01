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
    private static int nextItemID = 0;

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

    public static void CreateNewItem(Vector2 spawnPosition)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0.0f, 360.0f));
        Item item = Instantiate(itemTemplate, spawnPosition, rotation);
        item.AssignID(nextItemID);
        items.Add(nextItemID, item);
        Debug.Log($"Create item object with ID of {nextItemID}");
        nextItemID++;
    }

    public string SendItemPositions()
    {
        string response = "ItemManager:UpdateItemPositions:";
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
            CreateNewItem(pos);
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

    public static void HideItem(int itemID)
    {
        items[itemID].HideItem();
    }

    public static void RespawnItem(int itemID)
    {
        items[itemID].RespawnItem();
    }

    public void CreateItems(int numberOfItems)
    {
        for (int i = 0; i < numberOfItems; i++)
        {
            // Create item
            CreateNewItem(MapGenerator.instance.GetRandomSpawnPosition());
        }
    }
}
