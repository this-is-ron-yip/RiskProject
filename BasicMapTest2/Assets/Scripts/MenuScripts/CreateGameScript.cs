using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateGameScript : MonoBehaviour
{
    [SerializeField] InputField playerCountInput;
    
    /// <summary>
    /// After receiving input on how many players want to play,
    /// create the game by loading the next scene, and handing off the number of players.
    /// Do nothing if the input is invalid.
    /// </summary>
    public void CreateGame()
    {
        string playerCount = playerCountInput.text;

        if (Convert.ToInt32(playerCount) < 2 || Convert.ToInt32(playerCount) > 6)
        {
            Debug.Log("Invalid Player Count");
        }
        else
        {
            Debug.Log("Valid Player Count");
            StaticData.playerCount = Convert.ToInt32(playerCount);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

    }

}
