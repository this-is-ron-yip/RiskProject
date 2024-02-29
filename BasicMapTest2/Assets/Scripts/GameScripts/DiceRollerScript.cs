using UnityEngine;
using System;

public class DiceRollerScript : MonoBehaviour
{
    public bool canRoll = false; // Flag to control when rolling is allowed
    public bool isRollComplete = false; // Flag to indicate that a roll has finished

    // Define an event that other scripts can subscribe to, to get the dice roll result
    public event Action<int> OnDiceRolled;

    // Method to be called when we want to roll the dice
    public void RollDice()
    {
        if (canRoll)
        {
            int diceResult = UnityEngine.Random.Range(1, 7); // Random dice roll between 1 and 6

            // Implement dice roll animation here if needed

            // Trigger the event with the dice roll result
            OnDiceRolled?.Invoke(diceResult);

            // Indicate that the roll is complete
            isRollComplete = true;

            // Reset canRoll to ensure dice can't be rolled again until explicitly allowed
            canRoll = false;
        }
    }

    // Method to allow dice rolling, could be called with a player number if needed
    public void AllowRoll(int playerNumber) // default parameter in case you don't want to specify
    {
        canRoll = true;
        isRollComplete = false; // Ensure the roll is set to not complete when allowing a new roll
        Debug.Log($"Player {playerNumber}, press Spacebar to roll dice");
    }

    // Method to prevent dice rolling, if needed
    public void PreventRoll()
    {
        canRoll = false;
    }

    // Update is used to check for input to roll the dice
    private void Update()
    {
        // Check if the space bar is pressed and if rolling is allowed
        if (Input.GetKeyDown(KeyCode.Space) && canRoll)
        {
            RollDice();
        }
    }
}
