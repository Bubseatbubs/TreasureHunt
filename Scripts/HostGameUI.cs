using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class HostGameUI : MonoBehaviour
{
    public GameObject connectMenu;
    public TMP_InputField PortInput;

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
        int port;
        if (int.TryParse(convertTMPToString(PortInput), out port))
        {
            Debug.Log($"Parsed value: {port}");
        }
        else
        {
            Debug.Log("The input could not be parsed as an integer.");
            return;
        }

        NetworkController networkController = NetworkController.Instance();
        networkController.HostGame(port);
    }

    private string convertTMPToString(TMP_InputField tmpComponent)
    {
        string text = tmpComponent.text;
        return text;
    }
}
