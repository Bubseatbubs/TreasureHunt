using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

public class ConnectToHostUI : MonoBehaviour
{
    public GameObject connectMenu;
    public TMP_InputField IPAddressInput;
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

    public void ConnectToHost()
    {
        string IPAddress = convertFieldToString(IPAddressInput);
        string username = convertFieldToString(UsernameInput);

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
        networkController.ConnectToGame(IPAddress, port, username);
    }

    private string convertFieldToString(TMP_InputField field)
    {
        string text = field.text;
        return text;
    }
}
