using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateGameScript : MonoBehaviour
{
    [SerializeField] InputField playerCountInput;

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
