using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class SubmitOption : MonoBehaviour
{
    // Values assigned in editor
    public int playerNum;

    public void HandleInputData(int option)
    {
        switch (option)
        {
            case 0:
                StaticData.ConfigureColorArray(Color.red, playerNum);
                break;
            case 1:
                StaticData.ConfigureColorArray(Color.yellow, playerNum);
                break;
            case 2:
                StaticData.ConfigureColorArray(Color.green, playerNum);
                break;
            case 3:
                StaticData.ConfigureColorArray(Color.blue, playerNum);
                break;
            case 4:
                StaticData.ConfigureColorArray(Color.magenta, playerNum);
                break;
            case 5:
                StaticData.ConfigureColorArray(Color.cyan, playerNum);
                break;
        }
    }
}
