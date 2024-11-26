using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.IO.LowLevel.Unsafe;
using System;
using TMPro;

public class Player : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public float moveSpeed;
    public Rigidbody2D rb2d;
    public Vector2 forceToApply;
    public float forceDamping;
    public String username { get; private set; }
    public int ID { get; private set; }
    public float weight { get; private set; }
    public float speedMultiplier { get; private set; }
    public int inventoryCount { get; private set; }
    private Vector2 PlayerInput;
    private static Dictionary<int, Item> inventory = new Dictionary<int, Item>();
    public int carriedValue { get; private set; }
    public int cashedInValue { get; private set; }
    private PlayerStats statWindow;

    [SerializeField]
    private TextMeshProUGUI usernameDisplay;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkController.ID == ID)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            virtualCamera.Follow = rb2d.transform;
            statWindow = FindObjectOfType<PlayerStats>();
        }

        weight = 0;
        inventoryCount = 0;
        carriedValue = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (NetworkController.ID == ID)
        {
            PlayerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        }

        Move(PlayerInput);
    }

    public void Move(Vector2 input)
    {
        Vector2 moveForce = input * (moveSpeed * (1 - speedMultiplier));
        ApplyForce(moveForce);
    }

    void ApplyForce(Vector2 moveForce)
    {
        moveForce += forceToApply;
        forceToApply /= forceDamping;

        if (Mathf.Abs(forceToApply.x) <= 0.01f && Mathf.Abs(forceToApply.y) <= 0.01f)
        {
            forceToApply = Vector2.zero;
        }

        rb2d.velocity = moveForce;
    }

    public void AssignID(int id)
    {
        ID = id;
    }

    public void AssignUsername(String name)
    {
        username = name;
        if (NetworkController.ID != ID)
        {
            usernameDisplay.text = username;
        }
    }

    public float GetXPosition()
    {
        return rb2d.position.x;
    }

    public float GetYPosition()
    {
        return rb2d.position.y;
    }

    public Vector2 GetPosition()
    {
        return rb2d.position;
    }

    public void SetPosition(Vector2 pos)
    {
        rb2d.position = pos;
    }

    public Vector2 GetInput()
    {
        return PlayerInput;
    }

    public void SetInput(Vector2 Input)
    {
        PlayerInput = Input;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (NetworkController.isHost)
        {
            if (collision.gameObject.CompareTag("Item"))
            {
                GameObject itemObject = collision.gameObject;
                Item item = itemObject.GetComponent<Item>();
                Debug.Log($"Player {ID} has picked up item {item.ID}");
                SendItemPickupToClients(item.ID);
                PlayerManager.UpdatePlayerItems(ID, item.ID);
            }
        }
    }

    void OnTriggerEnter2D(Collision2D collision)
    {
        if (NetworkController.isHost)
        {
            if (collision.gameObject.CompareTag("SpawnArea"))
            {
                Debug.Log($"Player {ID} is on spawn area");
            }
        }
    }

    private void SendItemPickupToClients(int itemID)
    {
        // Client asks host if they can pick up an item
        TCPHost.instance.SendDataToClients($"PlayerManager:UpdatePlayerItems:{ID}:{itemID}");
    }

    public void AddItem(int itemID) {
        Item item;
        if (!ItemManager.items.TryGetValue(itemID, out item)) {
            Debug.Log($"Item {itemID} doesn't exist!");
        }

        inventory.Add(itemID, item);
        carriedValue += item.value;
        weight += item.weight;
        speedMultiplier = Math.Clamp(0.02f * (weight / 5), 0, 0.95f);
        inventoryCount++;

        // Update UI
        if (NetworkController.ID == ID)
        {
            statWindow.UpdateStats(weight, carriedValue, inventoryCount);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {

    }
}
