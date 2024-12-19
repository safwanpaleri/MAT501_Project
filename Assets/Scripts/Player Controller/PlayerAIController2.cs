using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Script used for AI for the opponent in the SinglePlayer Game scene.
//The technique used in this script is Bayesian Network
public class PlayerAIController2 : MonoBehaviour
{
    //Cache Variables
    private Animator animator;
    [SerializeField] private Text playerName;
    [SerializeField] private Slider healthSlider;
    private PlayerController playerController;

    //variables
    public Transform player;
    public float health = 100f;
    public float punchRange = 1.5f;
    public float kickRange = 2.5f;
    public float lowHealth = 30f;


    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty difficulty = Difficulty.Medium;

    //difficulty variables(changes according to difficulty)
    private float reactionTime = 0.75f;
    private float reactionprobability = 0.75f;
    

    //Logic check variables
    private string healthStatus;
    private string distance;
    private string playerStatus;
    private float punchProbability;
    private float kickProbability;
    private float defendProbability;
    private float moveForwardProbability;
    private float moveBackwardProbability;

    private bool isDefending = false;
    [HideInInspector] public bool isAttacking = false;

    private void Start()
    {
        //getting references and saving to cache variables.
        animator = GetComponent<Animator>();
        playerController = player.GetComponent<PlayerController>();
        StartCoroutine(Reaction());
    }

    //Find appropriate action or reaction as the next movement 
    private IEnumerator Reaction()
    {
        // Get status
        healthStatus = GetHealthStatus();
        distance = GetDistanceBetweenPlayerAndAI();
        playerStatus = GetPlayerStatus();

        // Compute probabilities for each action
        punchProbability = PunchingProbability(healthStatus, distance);
        kickProbability = KickingProbability(healthStatus, distance);
        defendProbability = DefendingProbability(healthStatus, distance, playerController.isAttacking);
        moveForwardProbability = MoveForwardProbability(healthStatus, distance);
        moveBackwardProbability = MoveBackwardProbability(healthStatus, distance);

        // Choose the action with the highest probability
        float maxProbability = Mathf.Max(punchProbability, kickProbability, defendProbability, moveForwardProbability, moveBackwardProbability);
        float actionProbability = UnityEngine.Random.Range(0.0f,1.0f);

        if (actionProbability > reactionprobability)
        {
            if (maxProbability == punchProbability)
                Punch();
            else if (maxProbability == kickProbability)
                Kick();
            else if (maxProbability == moveForwardProbability)
                MoveForward();
            else
                MoveBackward();
        }

        if(playerController.isAttacking)
        {
            if ( actionProbability > defendProbability)
                Defend();
        }

        yield return new WaitForSeconds(reactionTime);
        StartCoroutine(Reaction());
    }

    void Update()
    {
        
    }

    //Get health status of ai
    string GetHealthStatus()
    {
        if (health <= lowHealth) 
            return "Low";
        else if (health <= lowHealth * 2) 
            return "Medium";
        else 
            return "High";
    }

    //Get distance between player and ai
    string GetDistanceBetweenPlayerAndAI()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= punchRange) 
            return "Close";
        else if (distance <= kickRange) 
            return "Medium";
        else 
            return "Far";
    }

    //get player current action/status.
    string GetPlayerStatus()
    {
        if (playerController.isAttacking)
            return "Attacking";
        else if (playerController.isDefending)
            return "Defending";
        else
            return "Idle";
    }

    //give the probabilty for a punch according to distance and health
    //if distance is close(in punching range) it will give probability according to health,
    //else just the smallest value 0.1.
    float PunchingProbability(string health, string distance)
    {
        if (distance == "Close")
            return health == "High" ? 0.8f : health == "Medium" ? 0.5f : 0.3f;
        return 0.1f;
    }

    //gives the probability for a kick according to distance and health
    //if distance is medium (in kicking range) it will give probabilty according to health,
    //else just the smallest number 0.1;
    float KickingProbability(string health, string distance)
    {
        if (distance == "Medium")
            return health == "High" ? 0.7f : health == "Medium" ? 0.4f : 0.2f;
        return 0.1f;
    }

    //gives the probability for defenfing according to distance, health and if player is attacking
    //if player is attacking then it will give probabilty according to health
    //else just the smallest number 0.2;
    float DefendingProbability(string health, string distance, bool isPlayerAttacking)
    {
        if (isPlayerAttacking)
            return health == "Low" ? 0.8f : health == "Medium" ? 0.6f : 0.4f;
        return 0.2f;
    }

    //gives the probability for moving forward according to distance and health
    //if distance is far  it will give probabilty according to health,
    //else just the smallest number 0.2;
    float MoveForwardProbability(string health, string distance)
    {
        if (distance == "Far")
            return health == "High" ? 0.8f : health == "Medium" ? 0.5f : 0.3f;
        return 0.2f;
    }

    //gives the probability for moving backward according to distance and health
    //if distance is close and health is low it will give max value.
    //else just the smallest number 0.2;
    float MoveBackwardProbability(string health, string distance)
    {
        if (health == "Low" && distance == "Close")
            return 0.9f;
        return 0.2f;
    }

   
    //Animations Functions
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
        isAttacking = true;
        animator.SetTrigger("Kick");
        StartCoroutine(StopAttacking());
    }

    public void Punch()
    {
        isAttacking = true;
        animator.SetTrigger("Punch");
        StartCoroutine(StopAttacking());
    }

    public void Defend()
    {
        isDefending = true;
        animator.SetTrigger("Defend");
        StartCoroutine(StopDefending());
    }

    public void Win()
    {
        animator.SetTrigger("Win");
    }

    public void Lose()
    {
        animator.SetTrigger("Lose");
    }

    //bool resetting coroutines
    private IEnumerator StopAttacking()
    {
        yield return new WaitForSeconds(0.25f);
        isAttacking = false;
    }

    private IEnumerator StopDefending()
    {
        yield return new WaitForSeconds(0.18f);
        isDefending = false;
    }

    //Collision detection
    public void CollisionDetected(GameObject collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (!isDefending && !isAttacking && playerController.isAttacking)
            {
                health -= 5;
                healthSlider.value = health;
                if (health <= 0)
                {
                    if(playerController != null)
                        playerController.Win();
                    Lose();
                }
            }
        }
    }
}
