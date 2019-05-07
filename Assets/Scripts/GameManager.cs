using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public GameObject playerChar;

    #region Unity_functions
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region scene_transitions
    public void StartGame()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void StartLevel()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void NextLevel()
    {
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            SceneManager.LoadScene("WinScene");
        }
    }
    public void WinGame()
    {
        SceneManager.LoadScene("WinScene");
    }

    public void LoseGame()
    {
        SceneManager.LoadScene("LoseScene");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    #endregion

    #region player_death
    public void PlayerDied()
    {
        StartCoroutine("DieDelay");
    }

    IEnumerator DieDelay()
    {
        yield return new WaitForSeconds(1f);

        //Trigger anything we need to end the game, find game manager and lose game
        if (playerChar == null)
        {
            GameObject gm = GameObject.FindWithTag("GameController");
            gm.GetComponent<GameManager>().LoseGame();
        }
    }
    #endregion
}
