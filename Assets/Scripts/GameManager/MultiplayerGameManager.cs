using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


//Script that manages the Multiplayer Game Scene of the game.
public class MultiplayerGameManager : MonoBehaviour
{
    //Cache Variabale
    [SerializeField] private GameObject MobileControls;
    [SerializeField] private Text timerText;
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private GameObject loseCanvas;
    [SerializeField] private GameObject drawCanvas;
    [SerializeField] private GameObject exitCanvas;

    //Variables
    private int seconds = 0;
    private int minutes = 0;
    public bool isGameEnded = false;

    //Helper scripts
    [SerializeField] private PlayerController2 playerController;
    [SerializeField] private PlayerMultiplayerController multiplayerController;
    [SerializeField] private PlayerAIController playerAiController;
    [SerializeField] private NetworkClient client;
    [SerializeField] private NetworkServer server;

    //Checking whether the device is a android or iOS
    // if not then removing mobile control UI
    private void Awake()
    {
#if !UNITY_ANDROID || !UNITY_IOS

        MobileControls.SetActive(false);
#endif
    }
    
    // Start is called before the first frame update
    void Start()
    {
        //Start the game timer
        StartCoroutine(Timer());

        //Finding and caching server and client scripts
        server = FindAnyObjectByType<NetworkServer>();
        client = FindAnyObjectByType<NetworkClient>();

        //if we could find server networking script,
        //assign helper scripts to server script
        if (server != null)
        {
            server.playerController = playerController;
            server.playerAiController = playerAiController;
            server.multiplayerGameManager = this;
            server.playerMultiplayerController = multiplayerController;
        }

        //if we could find client networking script,
        //assign helper scripts to server scripts
        if (client != null)
        {
            client.playerController = playerController;
            client.playerAIController = playerAiController;
            client.multiplayerGameManager = this;
            client.playerMultiplayerController = multiplayerController;
        }
    }

    private void Update()
    {
        //If the player pressed escape button
        //a exit canvas ui is popped up, where player can choose to continue to play or to exit the game.
        if (Input.GetKeyDown(KeyCode.Escape))
            exitCanvas.SetActive(true);
    }

    //Function to call when game ends and show UI according to result of the game.
    public void GameEnded(string result)
    {
        if(result == "Win")
        {
            winCanvas.SetActive(true);
        }
        else if(result == "Lose")
        {
            loseCanvas.SetActive(true);
        }
        else
        {
            drawCanvas.SetActive(true);
        }
        isGameEnded = true;
    }


    //Coroutine of the timer Function
    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(1f);
        seconds++;
        if (seconds > 59)
        {
            minutes++;
            seconds= 0;
        }

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        if(!isGameEnded)
            StartCoroutine(Timer());
    }

    //A function to be attached to button to exit the game.
    public void MainMenu()
    {
        Application.Quit();

    }
}
