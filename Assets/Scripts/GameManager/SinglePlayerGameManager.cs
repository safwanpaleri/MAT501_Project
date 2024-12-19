using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Script that Manages Single Player Game Scene
public class SinglePlayerGameManager : MonoBehaviour
{
    //Cache Variables
    [SerializeField] private Text timer;
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private GameObject loseCanvas;
    [SerializeField] private GameObject drawCanvas;
    [SerializeField] private GameObject exitCanvas;

    //variables
    int seconds = 5;
    int minute = 0;
    bool stopTimer = false;

    // Start is called before the first frame update
    void Start()
    {
        //Start the game timer.
        StartCoroutine(TimerCoroutine());
    }

    private void Update()
    {
        //if the player presses Escape button, and exit pop up will be activated
        //where player can choose either to continue or to exit the game
        if (Input.GetKeyDown(KeyCode.Escape))
            exitCanvas.SetActive(true);
    }

    //Coroutine function of timer.
    private IEnumerator TimerCoroutine()
    {
        seconds--;
        if(seconds < 0)
        {
            seconds = 59;
            minute--;
        }
        if (minute < 0)
            stopTimer = true;
        if(minute >= 0)
            timer.text = string.Format("{0:00}:{1:00}", minute, seconds);
        yield return new WaitForSeconds(1f);
        if(!stopTimer)
            StartCoroutine(TimerCoroutine());
    }

    //Function to be called when the game ends,
    //Ui canvas will be activated according to result.
    public void GameEnded(string result)
    {
        if (result == "Win")
            winCanvas.SetActive(true);
        else if(result == "Lose")
            loseCanvas.SetActive(true);
        else
            drawCanvas.SetActive(true);
    }

    //A fucntion which will take us to the Menu Screen
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
