using UnityEngine;

/// <summary>
/// Static data that is used by MapScript to properly instantiate the game.
/// </summary>
public class StaticData : MonoBehaviour
{
    public static int playerCount;
    public static Color[] colorArray = new Color[6] {Color.red, Color.yellow, Color.green,
                                    Color.blue, Color.magenta, Color.cyan};
    
/// <summary>
/// Assign the given player the given number.
/// </summary>
/// <param name="color"></param>
/// <param name="playerNum"></param>
    public static void ConfigureColorArray(Color color, int playerNum)
    {
        colorArray[playerNum-1] = color;
    }
}
