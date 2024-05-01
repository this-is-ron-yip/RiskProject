using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsBackBtnPressed : MonoBehaviour
{

    public GameObject optionsMenu;
    public GameObject mainMenu;

    public void BackBtnPressed()
    {
        // Check if none of the player colours are the same before closing the options menu:

        Color[] colors = StaticData.colorArray;
        bool areAllUnique = AreAllUnique(colors);
        if (areAllUnique)
        {
            optionsMenu.SetActive(false);
            mainMenu.SetActive(true);
            
        }
        else
        {
            Debug.Log("All player colours must be unique");
        }
    }

    public static bool AreAllUnique(Color[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            for (int j = i + 1; j < array.Length; j++)
            {
                if (array[i].Equals(array[j]))
                {
                    return false;
                }
            }
        }
        // If no matches are found, all elements are unique
        return true;
    }
}
