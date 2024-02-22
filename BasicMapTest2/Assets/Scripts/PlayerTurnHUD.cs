using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerTurnHUD : MonoBehaviour
{   
    public TMP_Text playerTurnText;

    void OnValidate()
    {
        UpdatePlayerTurnUI(1); // Example starting player
    }

    // Call this method when the player's turn changes
    public void UpdatePlayerTurnUI(int currentPlayer)
    {
        playerTurnText.text = $"Player's Turn: {currentPlayer}";
    }
}
