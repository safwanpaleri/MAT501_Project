using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script used for detecting collision between player and opponent(AI/Multiplayer)
public class ChildCollisionDetection : MonoBehaviour
{
    //Cache Variables.
    [SerializeField] private PlayerAIController parent;
    [SerializeField] private PlayerAIController2 parent1;
    [SerializeField] private PlayerController parent2;
    [SerializeField] private PlayerController2 parent4;
    [SerializeField] private PlayerMultiplayerController parent3;

    bool sendOnce = false;

    //OnCollision detected
    private void OnCollisionEnter(Collision collision)
    {
        if (!sendOnce)
            StartCoroutine(SendCollision(collision.gameObject));

        //Debug.LogWarning("Collision 0");
    }

    //Send the collision to the parent object and that too only once.
    private IEnumerator SendCollision(GameObject collidedObject)
    {
        sendOnce = true;
        if (parent != null && parent.isActiveAndEnabled)
        {
            parent.CollisionDetected(collidedObject);
        }
        if (parent1 != null && parent1.isActiveAndEnabled)
        {
            parent1.CollisionDetected(collidedObject);
        }
        if (parent2 != null && parent2.isActiveAndEnabled)
        {
            parent2.CollisionDetected(collidedObject);
        }
        if (parent3 != null && parent3.isActiveAndEnabled)
        {
            parent3.CollisionDetected(collidedObject);
        }
        if (parent4 != null && parent4.isActiveAndEnabled)
        {
            parent4.CollisionDetected(collidedObject);
        }
        yield return new WaitForSeconds(1.0f);
        sendOnce = false;
    }
}
