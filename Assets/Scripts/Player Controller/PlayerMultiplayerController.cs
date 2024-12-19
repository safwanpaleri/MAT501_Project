using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Script that controls opponent in the multiplayer game scene.
public class PlayerMultiplayerController : MonoBehaviour
{
    //cache variables
    private NetworkClient networkClient;
    private NetworkServer networkServer;
    private bool isServer = false;
    private Animator animator;

    [SerializeField] private Text playerName;
    [SerializeField] private Slider healthSlider;
    [HideInInspector] private int health = 100;

    [SerializeField] private PlayerController2 playerController;
    //varibales
    [HideInInspector] public bool isDefending, isAttacking = false;
    [SerializeField] private MultiplayerGameManager gameManager;

    private void Awake()
    {
        //getting references and saving
        networkClient = FindObjectOfType<NetworkClient>();
        networkServer = FindObjectOfType<NetworkServer>();
        animator = GetComponent<Animator>();
        if (networkServer != null)
            isServer = networkServer.isServer;
        if(networkClient != null )
            networkClient.playerMultiplayerController = this;
        if(networkServer != null )
            networkServer.playerMultiplayerController = this;
    }

    //Function to handle Animation according to input
    public void HandleAnimation(string input)
    {
        animator.SetTrigger(input);

        if(input == "Punch" || input == "Kick")
        {
            StartCoroutine(SetAttacking());
        }
    }

    public void Win()
    {
        animator.SetTrigger("Win");
    }

    public void Lose()
    {
        animator.SetTrigger("Lose");
    }

    //coroutine function to set attacking
    private IEnumerator SetAttacking()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    //collision detection and send signal if hitted player
    public void CollisionDetected(GameObject collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (!isDefending && !isAttacking && playerController.isAttacking)
            {
                TakeDamage();
                if (isServer)
                    networkServer.SendUDPMessageToClient("Hit");
                else
                    networkClient.SendUDPMessageToServer("Hit");

            }
        }
    }

    //Function to take damage, triggered according to network message
    public void TakeDamage()
    {
        health -= 5;
        healthSlider.value = health;
        if(health < 0)
        {
            Lose();
            playerController.Win();
            if (isServer)
                networkServer.SendUDPMessageToClient("Lose");
            else
                networkClient.SendUDPMessageToServer("Lose");
            gameManager.GameEnded("Win");

        }
        Debug.Log("Take Damage");
    }

    //return health
    public int GetHealth()
    {
        return health;
    }

    //Verifing position and health with the server or client so the data is synced both devices.
    public void VerficationAndActions(Vector3 position, int health2)
    {
        var playerPosition = this.gameObject.transform.position;
        //if (playerPosition.x != position.x)
        //{
        //    if (playerPosition.x > position.x)
        //    {
        //        HandleAnimation("MoveBackward");
        //    }

        //    if (playerPosition.x < position.x)
        //    {
        //        HandleAnimation("MoveForward");
        //    }
        //}

        if (health2 != health)
        {
            health = health2;
            healthSlider.value = health;
        }
    }
}
