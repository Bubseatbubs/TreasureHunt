using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatMessagesDisplay : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI chatText;
    public static ChatMessagesDisplay instance;

    /*
    Singleton Pattern: Make sure there's only one Chat Window
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
    }

    public void UpdateChatMessages(string newChat)
    {
        chatText.text = newChat;
    }
}
