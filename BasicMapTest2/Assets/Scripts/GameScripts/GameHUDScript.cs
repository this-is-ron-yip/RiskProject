using UnityEngine;
using System.Collections.Generic; // Required for using Lists.
using UnityEngine.UI;
using TMPro;

public class GameHUDScript : MonoBehaviour
{
    public GameObject cardDisplayPrefab;
    public Transform cardDisplayPanel;
    public Transform chooseCardDisplayPanel;
    public PlayerScript currentPlayer;
    private List<Card> selectedCards = new List<Card>();
    private int cardsInDisplay = 0;

    [System.Obsolete]
    private void Start()
    {
        // Make sure the cards display panel is not visible at the start.
        cardDisplayPanel.gameObject.SetActive(false);
    }

    private void CardClicked(Card card, GameObject cardDisplay)
    {
        // Toggle selection of the card
        if (selectedCards.Contains(card))
        {
            // If the card was already selected, deselect it
            selectedCards.Remove(card);
            // Also change the visual state of the card to indicate it's deselected
            cardDisplay.GetComponent<Image>().color = Color.gray;

        }
        else
        {
            // If the card is not selected and less than 3 cards are selected, select it
            if (selectedCards.Count < 3)
            {
                selectedCards.Add(card);
                // Also change the visual state of the card to indicate it's selected
                cardDisplay.GetComponent<Image>().color = Color.green;
                // e.g., change color or toggle a checkbox image, etc.
            }
        }

        Debug.Log("Card clicked: " + card.territory_id + " - " + card.troop_type);
        Debug.Log("Number of Cards Selected: " + selectedCards.Count);

        // If three cards are selected, do something
        if (selectedCards.Count == 3)
        {
            // Call a method in MapScript and pass the selected cards
            MapScript mapScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapScript>();
            if (mapScript != null)
            {
                chooseCardDisplayPanel.gameObject.SetActive(false);
                mapScript.HandleCardTurnIn(selectedCards);
                // Clear the selected cards list
                selectedCards.Clear();
                
            }
        }
    }


    [System.Obsolete]
    public void ShowChooseCardPanel()
    {
        chooseCardDisplayPanel.gameObject.SetActive(true);

        foreach (Transform child in chooseCardDisplayPanel)
        {
            //destroys all the existing cards
            //destroys the button to return without turning in cards if the player has max number of cards (the player must turn in 3 cards)
            if (child.tag == "card" || (child.tag == "BackToGameBtn" && currentPlayer.cardsInHand.Count == 5))
            {
                Destroy(child.gameObject);
            }
        }

        // Reset card count before displaying new cards
        cardsInDisplay = 0;

        // Create a display for each card in the current player's hand
        foreach (Card card in currentPlayer.cardsInHand)
        {
            CreateChooseCardDisplay(card);
        }
    }

    // Call this method when the "View Cards" button is pressed.
    public void OnViewCardsButtonPressed()
    {
        if (currentPlayer != null)
        {
            cardDisplayPanel.gameObject.SetActive(true);

            foreach (Transform child in cardDisplayPanel)
            {
                if (child.tag == "card")
                {
                    Destroy(child.gameObject);
                }

            }

            // Reset card count before displaying new cards
            cardsInDisplay = 0;

            // Create a display for each card in the current player's hand
            foreach (Card card in currentPlayer.cardsInHand)
            {
                CreateCardDisplay(card);
            }
        }
        else
        {
            Debug.Log("Game has not started yet!");
        }
    }

    // Creates a new card display for the given card.
    void CreateCardDisplay(Card card)
    {
        GameObject cardDisplay = Instantiate(cardDisplayPrefab, cardDisplayPanel);

        // Calculate the X position, starting with the initialXOffset.
        float xOffset = -300f + (cardsInDisplay * 150f);
        cardDisplay.GetComponent<RectTransform>().anchoredPosition = new Vector2(xOffset, 0f);

        // Update the text of the card
        string _troopType = card.troop_type;
        string territory_id = card.territory_id;
        TextMeshProUGUI textComponent = cardDisplay.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = territory_id + " : " + _troopType;
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found in card display prefab.");
        }

        // Increment the card count so the next card is positioned further to the right
        cardsInDisplay++;
    }

    public void CreateChooseCardDisplay(Card card)
    {
        Debug.Log("Choose your Cards to turn in!");
        GameObject cardDisplay = Instantiate(cardDisplayPrefab, chooseCardDisplayPanel);

        // Set the position for the card display
        float xOffset = -300f + (cardsInDisplay * 150f); // Adjust as needed
        cardDisplay.GetComponent<RectTransform>().anchoredPosition = new Vector2(xOffset, 0f);

        // Update the text of the card
        TextMeshProUGUI textComponent = cardDisplay.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = card.territory_id + " : " + card.troop_type;
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found in card display prefab.");
        }

        // Add a click event listener to the card
        Button button = cardDisplay.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => CardClicked(card, cardDisplay));
        }
        else
        {
            Debug.LogError("Button component not found in card display prefab.");
        }

        cardsInDisplay++;
    }

    // Closes the Cards display panel and returns to the game
    public void OnBackToGamePressed()
    {
        selectedCards.Clear();
        chooseCardDisplayPanel.gameObject.SetActive(false);
        MapScript mapScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapScript>();
        mapScript.HandleCardTurnIn(null);
        mapScript.players[mapScript.playerTurn - 1].isTurn = false; // Notify board that this sequence is over
    }
}

// TODO: add logic for submitting cards. consider changing implementation to match the rest
// Sicne the on click handlers can't be coroutines, causing out of order debug statements.