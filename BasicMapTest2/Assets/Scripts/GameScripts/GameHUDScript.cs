using UnityEngine;
using System.Collections.Generic; // Required for using Lists.
using UnityEngine.UI;
using TMPro;

public class GameHUDScript : MonoBehaviour
{
    public GameObject cardDisplayPrefab;
    public Transform cardDisplayPanel;
    public Transform chooseCardDisplayPanel;
    public Transform endOfGamePanel;
    public bool isOnDisplay;
    public bool wantsToReturn = false;
    public PlayerScript currentPlayer;
    public List<Card> selectedCards = new List<Card>();
    private int cardsInDisplay = 0;

    [System.Obsolete]
    private void Start()
    {
        // Make sure the display panels are not visible at the start.
        cardDisplayPanel.gameObject.SetActive(false);
        endOfGamePanel.gameObject.SetActive(false);
        isOnDisplay = false;
        wantsToReturn = false;
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

        // If three cards are selected, do something
        if (selectedCards.Count == 3)
        {
            // Call a method in MapScript and pass the selected cards
            MapScript mapScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapScript>();
            if (mapScript != null)
            {
                mapScript.HandleCardTurnIn(selectedCards, currentPlayer);
                chooseCardDisplayPanel.gameObject.SetActive(false);
                isOnDisplay = false;
                selectedCards.Clear();  
            }
        }
    }


    [System.Obsolete]
    public void ShowChooseCardPanel()
    {
        chooseCardDisplayPanel.gameObject.SetActive(true);
        isOnDisplay = true;
        wantsToReturn = false;

        foreach (Transform child in chooseCardDisplayPanel)
        {
            //destroys the button to return without turning in cards if the player has max number of cards (the player must turn in 3 cards)
            if (child.tag == "card")
            {
                Destroy(child.gameObject);
            }
        }

        // Reset card count before displaying new cards
        cardsInDisplay = 0;

        // Create a display for each card in the current player's hand
        Debug.Log("Choose your cards to turn in!");
        Debug.Log("Cards remaining in player hand: " + currentPlayer.cardsInHand.Count);
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
            isOnDisplay = true;

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
    public void OnDoNotTurnInCardsPressed()
    {
        selectedCards.Clear();
        chooseCardDisplayPanel.gameObject.SetActive(false);
        isOnDisplay = false;
        wantsToReturn = true;
        MapScript mapScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapScript>();
        mapScript.HandleCardTurnIn(null, currentPlayer);
    }

    // From the view cards panel
    public void OnBackToGamePressed(){
        cardDisplayPanel.gameObject.SetActive(false);
        isOnDisplay = false;
    }

    public void ShowEndingPanel(int winnerNum) {
        endOfGamePanel.gameObject.SetActive(true);
        Debug.Log("Player " + winnerNum + " has conquered all the territories and won the game! GAME OVER.");
    }

    public void OnQuitGamePressed()
    {
        // TODO: add code to quit/kill the game.
        Debug.Log("Quit game pressed.");
    }
}