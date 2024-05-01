using UnityEngine;


/// <summary>
/// Utility class to create die rolls. 
/// </summary>
public class DiceRollerScript : MonoBehaviour
{

    // Method to be called when we want to roll the dice
    public int RollDice()
    {
        return Random.Range(1, 7); // Random dice roll between 1 and 6
    }
}
