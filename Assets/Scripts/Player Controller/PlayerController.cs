using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Script which controlls the player
public class PlayerController : MonoBehaviour
{
    //variables
    [HideInInspector] public bool isMoveForward = false, isMoveBackward = false, isKicking = false, isPunching = false, isDefending = false, isAttacking = false;

    //Cache variables
    private Animator animator;
   
    [SerializeField] private Text playerName;
    [SerializeField] private Slider healthSlider;
    [HideInInspector] public int health = 100;

    //Helper script
    [SerializeField] private PlayerAIController opponentController;
    [SerializeField] private PlayerAIController2 opponentController2;
    [SerializeField] private PlayerMultiplayerController playerMultiplayerController;
    [SerializeField] private SinglePlayerGameManager gameManager;
    private void Awake()
    {
        //Getting and saving reference
        animator = GetComponent<Animator>();
    }
    

    // Update is called once per frame
    void Update()
    {
        //Handle Input and actions accordingly.
        HandleInput();
        HandleAnimation();
    }

    //Function for handling keyboard inputs.
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

    //Functions for handling animations according to inputs.
    void HandleAnimation()
    {
        if (isMoveForward)
        {
            MoveForward();
        }

        if (isMoveBackward)
        {
            MoveBackward();
        }

        if (isKicking)
        {
            Kick();
        }

        if (isPunching)
        {
            Punch();
        }

        if (isDefending)
        {
            Defend();
        }
    }

    //Animation funtions
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
        gameManager.GameEnded("Win");
    }

    public void Lose()
    {
        animator.SetTrigger("Lose");
        gameManager.GameEnded("Lose");
    }

    //Collision Detection
    public void CollisionDetected(GameObject collision)
    {
        if (collision.gameObject.tag == "Opponent")
        {
            if (!isDefending && !isAttacking && (opponentController.isAttacking || opponentController2.isAttacking))
            {
                health -= 10;
                healthSlider.value = health;
                if(health <= 0)
                {
                    if(opponentController != null && opponentController.isActiveAndEnabled)
                        opponentController.Win();
                    if(opponentController2 != null && opponentController2.isActiveAndEnabled)
                        opponentController2.Win();
                    if (playerMultiplayerController != null && playerMultiplayerController.isActiveAndEnabled)
                        playerMultiplayerController.Win();
                    Lose();
                }

            }

        }
       
    }
}
