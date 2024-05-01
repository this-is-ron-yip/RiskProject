using UnityEngine;

/// <summary>
/// Static data that is used by MapScript to properly instantiate the game.
/// </summary>
public class StaticData : MonoBehaviour
{
    public static int playerCount;

    public static Color[] colorArray = new Color[6] {Color.red, Color.yellow, Color.green,
                                    Color.blue, Color.magenta, Color.cyan};
    private void OnValidate()
    {
    }

    public static void ConfigureColorArray(Color color, int playerNum)
    {
        int playerNumIndex = playerNum - 1;

        Debug.Log(colorArray[0].ToString());
        Debug.Log(colorArray[1].ToString());
        Debug.Log(colorArray[2].ToString());
        Debug.Log(colorArray[3].ToString());
        Debug.Log(colorArray[4].ToString());
        Debug.Log(colorArray[5].ToString());
        colorArray[playerNumIndex-1] = color;
    }
}
