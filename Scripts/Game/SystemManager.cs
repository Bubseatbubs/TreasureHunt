using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    public static SystemManager instance;
    public double timer; // In seconds
    public bool gameBegan;

    [SerializeField]
    TextMeshProUGUI timerText;

    [SerializeField]
    GameObject timerPanel;

    [SerializeField]
    GameObject chatPanel;

    [SerializeField]
    private GameObject mazeGeneratorObject;

    [SerializeField]
    private GameObject playerStatsWindow;

    /*
    Singleton: Make sure there's only one SystemManager
    */
    void Awake()
    {
        if (instance)
        {
            return;
        }

        // No instance yet, set this to it
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        timer = 300;
    }

    public void InitializeGame()
    {
        chatPanel.SetActive(true);
        if (!NetworkController.isHost)
        {
            ChatManager.instance.SendSystemMessage($"{NetworkController.instance.username} joined the game");
        }
    }

    public void StartGame(String username)
    {
        gameBegan = true;
        MapGenerator mazeGenerator = mazeGeneratorObject.GetComponent<MapGenerator>();

        // Create player
        PlayerManager.CreateNewPlayer(NetworkController.ID, username);

        // Create map
        mazeGenerator.Instantiate();

        // Show game UI
        timerPanel.SetActive(true);
        playerStatsWindow.SetActive(true);
        Debug.Log("The game has begun!");
    }

    void FixedUpdate()
    {
        if (gameBegan)
        {
            timer -= 0.02;
            GenTimeSpanFromSeconds();
            if (timer <= 0)
            {
                EndGame();
            }
        }
    }

    public void EndGame()
    {
        gameBegan = false;

        Player winner = PlayerManager.instance.GetHighestScoringPlayer();
        if (NetworkController.isHost)
        {
            ChatManager.instance.SendSystemMessage($"The game has ended! The winner is {winner.username} with a balance of ${winner.balance}!");
        }

        timerPanel.SetActive(false);
        playerStatsWindow.SetActive(false);
    }

    public void GenTimeSpanFromSeconds()
    {
        // Create a TimeSpan object and TimeSpan string from 
        // a number of seconds.
        TimeSpan interval = TimeSpan.FromSeconds(timer);
        string timeInterval = interval.ToString(@"mm\:ss");

        timerText.text = timeInterval;
    }
}
