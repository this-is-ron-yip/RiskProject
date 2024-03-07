using UnityEngine;
using System;

public class DiceRollerScript : MonoBehaviour
{

    // Method to be called when we want to roll the dice
    public int RollDice()
    {
        return UnityEngine.Random.Range(1, 7); // Random dice roll between 1 and 6
    }

    // Update is used to check for input to roll the dice
    private void Update()
    {
    }
}
