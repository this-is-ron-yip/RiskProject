using UnityEngine;

/// <summary>
/// The script that is tied to the main menu controller on the opening game scene.
/// </summary>
public class MainMenuScript : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
