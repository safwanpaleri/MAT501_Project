using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//script that controls player in the multiplayer game scene
public class PlayerController2 : MonoBehaviour
{
    //variables
    [HideInInspector] public bool isMoveForward = false, isMoveBackward = false, isKicking = false, isPunching = false, isDefending = false, isAttacking = false;

    //cache variables.
    private Animator animator;

    [SerializeField] private Text playerName;
    [SerializeField] private Slider healthSlider;
    [HideInInspector] public int health = 100;

    //Helper scripts
    [SerializeField] private PlayerMultiplayerController playerMultiplayerController;
    [SerializeField] private MultiplayerGameManager gameManager;

    //Network Scripts
    private NetworkClient client;
    private NetworkServer server;
    bool isServer = false;

    
    private void Awake()
    {
        //getting and saving references
        animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        //Getting networking script references
        client = FindObjectOfType<NetworkClient>();
        server = FindObjectOfType<NetworkServer>();

        if(server != null )
        {
            //checking if the player is server.
            isServer = server.isServer;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Handle input and Animations
        HandleInput();
        HandleAnimation();
    }

    //Function to handle input by keyboard.
    void HandleInput()
    {
        if (Input.GetKey(KeyCode.W))
            isMoveForward = true;

        if (Input.GetKeyUp(KeyCode.W))
            isMoveForward = false;

        if (Input.GetKeyDown(KeyCode.S))
            isMoveBackward = true;

        if (Input.GetKeyUp(KeyCode.S))
            isMoveBackward = false;

        if (Input.GetKeyDown(KeyCode.J))
        {
            isKicking = true;
            isAttacking = true;
        }
        if (Input.GetKeyUp(KeyCode.J))
        {
            isKicking = false;
            isAttacking = false;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            isDefending = true;
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            isDefending = false;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            isPunching = true;
            isAttacking = true;
        }
        if (Input.GetKeyUp(KeyCode.L))
        {
            isPunching = false;
            isAttacking = false;
        }
    }

    //function to handle animation and send action to client or server accordingly to reflect in that device.
    void HandleAnimation()
    {
        if (isMoveForward)
        {
            MoveForward();
            if (isServer)
                server.SendUDPMessageToClient("MoveForward");
            else
                client.SendUDPMessageToServer("MoveForward");
        }

        if (isMoveBackward)
        {
            MoveBackward();
            if (isServer)
                server.SendUDPMessageToClient("MoveBackward");
            else
                client.SendUDPMessageToServer("MoveBackward");
        }

        if (isKicking)
        {
            Kick();
            if (isServer)
                server.SendUDPMessageToClient("Kick");
            else
                client.SendUDPMessageToServer("Kick");
        }

        if (isPunching)
        {
            Punch();
            if (isServer)
                server.SendUDPMessageToClient("Punch");
            else
                client.SendUDPMessageToServer("Punch");
        }

        if (isDefending)
        {
            Defend();
            if (isServer)
                server.SendUDPMessageToClient("Defend");
            else
                client.SendUDPMessageToServer("Defend");
        }
    }

    public void MoveForward()
    {
        animator.SetTrigger("MoveForward");
    }

    public void MoveBackward()
    {
        animator.SetTrigger("MoveBackward");
    }

    public void Kick()
    {
        animator.SetTrigger("Kick");
    }

    public void Punch()
    {
        animator.SetTrigger("Punch");
    }

    public void Defend()
    {
        animator.SetTrigger("Defend");
    }

    public void Win()
    {
        animator.SetTrigger("Win");
    }

    public void Lose()
    {
        animator.SetTrigger("Lose");
    }

    //Collision detection
    //Send "Win" or "Lose" accordingly to trigger event over other device.
    public void CollisionDetected(GameObject collision)
    {
        if (collision.gameObject.tag == "Opponent")
        {
            if (!isDefending && !isAttacking && playerMultiplayerController.isAttacking)
            {
                if (isServer)
                    server.SendUDPMessageToClient("Hit");
                else
                    client.SendUDPMessageToServer("Hit");

                health -= 5;
                healthSlider.value = health;
                if (health <= 0)
                {
                    if (playerMultiplayerController != null && playerMultiplayerController.isActiveAndEnabled)
                    {
                        playerMultiplayerController.Win();
                        if (isServer)
                            server.SendUDPMessageToClient("Win");
                        else
                            client.SendUDPMessageToServer("Win");

                        gameManager.GameEnded("Lose");
                    }

                    Lose();
                }
            }
        }
    }
}
