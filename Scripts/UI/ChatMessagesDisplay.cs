using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ChatMessagesDisplay : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI chatText;
    public static ChatMessagesDisplay instance;

    [SerializeField]
    private RectTransform chatBox;

    [SerializeField]
    private Scrollbar scrollbar;

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
        chatText.text = newChat + "\n\n";
        scrollbar.value = 0.0f;
    }
}
