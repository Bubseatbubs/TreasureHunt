using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System;

public class HostGameUI : MonoBehaviour
{
    public GameObject connectMenu;
    public TMP_InputField PortInput;
    public TMP_InputField UsernameInput;

    public void ToggleMenu()
    {
        if (connectMenu != null)
        {
            if (connectMenu.activeSelf)
            {
                connectMenu.SetActive(false);
            }
            else
            {
                connectMenu.SetActive(true);
            }
        }
    }

    public void Host()
    {
        ToggleMenu();
        String username = convertFieldToString(UsernameInput);

        int port;
        if (int.TryParse(convertFieldToString(PortInput), out port))
        {
            Debug.Log($"Parsed value: {port}");
        }
        else
        {
            Debug.Log("The input could not be parsed as an integer.");
            return;
        }

        NetworkController networkController = NetworkController.instance;
        networkController.HostGame(port, username);
    }

    private string convertFieldToString(TMP_InputField tmpComponent)
    {
        string text = tmpComponent.text;
        return text;
    }
}