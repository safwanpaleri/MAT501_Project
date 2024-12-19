using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Script used for AI for the opponent in the SinglePlayer Game scene.
//The technique used in this script is FuzzyLogic
public class PlayerAIController : MonoBehaviour
{
    //Cache variables
    private Animator animator;
    private PlayerController playerController;
    [SerializeField] private Text playerName;
    [SerializeField] private Slider healthSlider;
    [HideInInspector] public bool isDefending = false;
    [HideInInspector] public bool isAttacking = false;

    //variables
    public Transform player;
    public float health = 100f;
    public float punchRange = 1.5f;
    public float kickRange = 2.5f;
    public float lowHealth = 30f;

    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty difficulty = Difficulty.Medium;

    //Logic check variables
    float distance;
    float distanceClose;
    float distanceMedium;
    float distanceFar;
    float healthLow;
    float healthHigh;
    float punchDecision;
    float kickDecision;
    float defendDecision;
    float moveForwardDecision;
    float moveBackwardDecision;
    float rand;

    //difficulty variables
    private float reactionTime = 0.75f;
    private float probabilty = 0.75f;


    private void Start()
    {
        //getting references and saving to cache variables.
        animator = GetComponent<Animator>();
        playerController = player.GetComponent<PlayerController>();
        StartCoroutine(Reaction());
    }

    void Update()
    {
        
        
    }

    //Find appropriate action or reaction as the next movement 
    private IEnumerator Reaction()
    {
        //Check Distance
        distance = Vector3.Distance(transform.position, player.position);
        
        distanceClose = PunchRangeCheck(distance);
        distanceMedium = PunchOrKickCheck(distance);
        distanceFar = KickRangeCheck(distance);

        //Check health
        healthLow = LowHealthCheck(health);
        healthHigh = HighHealthCheck(health);

        #region Rules
        //Rule 1: if player is close and ai has low health, then punch.
        punchDecision = CheckMin(distanceClose, 1 - healthLow);
        //Rule2: if player is not in punch range but in kick range and ai in low health, then kick.
        kickDecision = CheckMin(distanceMedium, 1 - healthLow);
        //Rule 3: if player is attacking, then defend.
        defendDecision = CheckMin(PlayerIsAttacking(), 1);
        //Rule 4: if player is far and have high health,then move towards player.
        moveForwardDecision = CheckMin(distanceFar, healthHigh);
        //Rule 5: if player is close and ai has low health, then move backward.
        moveBackwardDecision = CheckMin(distanceClose, healthLow);
        #endregion

        #region Action Selection
        //probabilty is a difficulty parameter, if the mode is easy less liketly to take decision.,
        //if the mode is hard, then more likely to take decision.
        //if player is attacking, then defend according to difficulty
        if (playerController.isAttacking)
        {
            rand = (UnityEngine.Random.Range(0.0f, 1.0f));
            if (rand > probabilty)
                Defend();
        }
        // if punch decision is greater than all other decisions then do punch.
        if (punchDecision > kickDecision && punchDecision > defendDecision &&
            punchDecision > moveForwardDecision && punchDecision > moveBackwardDecision)
        {
            rand = (UnityEngine.Random.Range(0.0f, 1.0f));
            if(rand > probabilty)
                Punch();
            
        }
        //if kick decision is greater than all other decisions then do punch
        else if (kickDecision > defendDecision && kickDecision > moveForwardDecision &&
                 kickDecision > moveBackwardDecision)
        {
            rand = (UnityEngine.Random.Range(0.0f, 1.0f));
            if (rand > probabilty)
                Kick();
        }
        //if move forward decision is greater than all other decision then move forward
        else if (moveForwardDecision > moveBackwardDecision)
        {
            rand = (UnityEngine.Random.Range(0.0f, 1.0f));
            if (rand > probabilty)
                MoveForward();
        }
        //if move backward decision is greater than all other decisions then move backwards.
        else
        {
            rand = (UnityEngine.Random.Range(0.0f, 1.0f));
            if (rand > probabilty)
                MoveBackward();
        }
        #endregion

        //reaction time is also a difficulty paramater.
        //it is the gap between an action to next. 
        //so if the mode is easier, ai will take longer to take decision,
        //as difficulty increases the reaction time decreases.
        yield return new WaitForSeconds(reactionTime);

        StartCoroutine(Reaction());
    }

    //Logic to check whether the ai is within the punch range.
    //if the ai is within the range it will return 1 and if not it will return 0
    float PunchRangeCheck(float distance)
    {
        return Mathf.Clamp01(1 - distance / punchRange);
    }

    //Logic to determine whether the ai is within the punch range or kick range.
    //if the ai is below the punch range it will return 0 and beyond kick range returns 1.
    float PunchOrKickCheck(float distance)
    {
        return Mathf.Clamp01((distance - punchRange) / (kickRange - punchRange));
    }

    //Logic to check whether the ai is within the kick range.
    //if the ai is below the range it will return 0 and if it is far then it will return 1.
    float KickRangeCheck(float distance)
    {
        return Mathf.Clamp01((distance - kickRange) / kickRange);
    }

    //logic to check whether the ai has low health.
    //if ai has health lower than great health it will return 0 and if it has low health it will return 1
    float LowHealthCheck(float currentHealth)
    {
        return Mathf.Clamp01(1 - currentHealth / lowHealth);
    }

    //logic to check whether the ai has high health
    //if ai has great health return 1 and low health return 0
    float HighHealthCheck(float currentHealth)
    {
        return Mathf.Clamp01(currentHealth / lowHealth);
    }

    //Checking whether the player is attacking or not
    //if attacking returns 1, else 0
    float PlayerIsAttacking()
    {
        return playerController.isAttacking ? 1.0f : 0.0f;
    }

    // Returns minimum or both float.
    float CheckMin(float a, float b)
    {
        return Mathf.Min(a, b);
    }
    
    //Animation functions
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
        //if the collided object is player and the ai is not defending and ai is not the one attacking
        // and player is attacking (not just random collision) then decrease the health.
        // if the health reaches 0 then send a confirmation to player and activate "Lose" Animation.
        if (collision.gameObject.tag == "Player")
        {
            if (!isDefending && !isAttacking && playerController.isAttacking)
            {
                health -= 5;
                healthSlider.value = health;
                if (health <= 0)
                {
                    if (playerController != null)
                        playerController.Win();
                    Lose();
                }
                
            }
           
        }
    }

    //Return next logical move as string,
    //Utility function for other script to decide an action.
    public string GetASingleReaction()
    {
        distance = Vector3.Distance(transform.position, player.position);
        distanceClose = PunchRangeCheck(distance);
        distanceMedium = PunchOrKickCheck(distance);
        distanceFar = KickRangeCheck(distance);

        healthLow = LowHealthCheck(health);
        healthHigh = HighHealthCheck(health);

        punchDecision = CheckMin(distanceClose, 1 - healthLow);
        kickDecision = CheckMin(distanceMedium, 1 - healthLow);
        defendDecision = CheckMin(PlayerIsAttacking(), 1);
        moveForwardDecision = CheckMin(distanceFar, healthHigh);
        moveBackwardDecision = CheckMin(distanceClose, healthLow);

        if (playerController.isAttacking)
        {
            rand = (UnityEngine.Random.Range(0.0f, 1.0f));
            if (rand > probabilty)
                return "Defend";
        }
        else if (punchDecision > kickDecision && punchDecision > defendDecision &&
            punchDecision > moveForwardDecision && punchDecision > moveBackwardDecision)
        {
            rand = (UnityEngine.Random.Range(0.0f, 1.0f));
            if (rand > probabilty)
                return "Punch";

        }
        else if (kickDecision > defendDecision && kickDecision > moveForwardDecision &&
                 kickDecision > moveBackwardDecision)
        {
            rand = (UnityEngine.Random.Range(0.0f, 1.0f));
            if (rand > probabilty)
                return "Kick";
        }
        else if (moveForwardDecision > moveBackwardDecision)
        {
            rand = (UnityEngine.Random.Range(0.0f, 1.0f));
            if (rand > probabilty)
                return "MoveForward";
        }
        else
        {
            rand = (UnityEngine.Random.Range(0.0f, 1.0f));
            if (rand > probabilty)
               return "MoveBackward";
        }

        return "idle";
    }
}
