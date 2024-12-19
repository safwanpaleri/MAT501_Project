using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//Script which manages the Menu scene of the game.
public class MenuManager : MonoBehaviour
{
    //Attached to a button
    //when clicked it will change into SinglePlayerScene
    //where player can play with an AI Bot
    public void SelectSinglePlayerFuzzyLogic()
    {
        SceneManager.LoadScene(3);
    }
    public void SelectSinglePlayerBayesianNetwork()
    {
        SceneManager.LoadScene(2);
    }
}
