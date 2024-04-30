using UnityEngine;
using System.Collections.Generic; // Required for using Lists.
using UnityEngine.UI;

using TMPro;
using System;

public class GameHUDScript : MonoBehaviour
{
    public GameObject cardDisplayPrefab;
    public Transform cardDisplayPanel;
    public Transform chooseCardDisplayPanel;
    public Transform endOfGamePanel;
    public Button viewCardsBtn;
    public TextMeshProUGUI eventCardTMP;
    public TextMeshProUGUI errorCardTMP;
    public TextMeshProUGUI infoCardTMP;

    public Transform attackInputPanel;
    public bool cardsAreOnDisplay;
    public bool attackInputIsOnDisplay;
    public bool wantsToReturn = false;
    public PlayerScript currentPlayer;
    public List<Card> selectedCards = new List<Card>();
    private int cardsInDisplay = 0;
    [SerializeField] InputField attacker_army_input;
    [SerializeField] InputField defender_army_input;

    [System.Obsolete]
    private void Start()
    {
        // Make sure the display panels are not visible at the start.
        cardDisplayPanel.gameObject.SetActive(false);
        endOfGamePanel.gameObject.SetActive(false);
        attackInputPanel.gameObject.SetActive(false);
        cardsAreOnDisplay = false;
        attackInputIsOnDisplay = false;
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
                cardsAreOnDisplay = false;
                selectedCards.Clear();  
            }
        }
    }


    [System.Obsolete]
    public void ShowChooseCardPanel()
    {
        chooseCardDisplayPanel.gameObject.SetActive(true);
        cardsAreOnDisplay = true;
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
            cardsAreOnDisplay = true;

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
        cardsAreOnDisplay = false;
        wantsToReturn = true;
        MapScript mapScript = GameObject.FindGameObjectWithTag("Map").GetComponent<MapScript>();
        mapScript.HandleCardTurnIn(null, currentPlayer);
    }

    // From the view cards panel
    public void OnBackToGamePressed(){
        cardDisplayPanel.gameObject.SetActive(false);
        cardsAreOnDisplay = false;
    }

    public void ShowEndingPanel(int winnerNum) {
        endOfGamePanel.gameObject.SetActive(true);
        Debug.Log("Player " + winnerNum + " has conquered all the territories and won the game! GAME OVER.");
    }

    public void ShowAttackInputPanel() {
        attackInputIsOnDisplay = true;
        attackInputPanel.gameObject.SetActive(true);
        viewCardsBtn.gameObject.SetActive(false);
        infoCardTMP.GetComponentInParent<RawImage>().gameObject.GetComponentInParent<Canvas>().gameObject.SetActive(false); // Deactviating all the info cards
    }

    public void OnAttackInputSubmitPressed()    
    {
        viewCardsBtn.gameObject.SetActive(false); // Turning this button off so that it looks nicer

        // Getting the inputs from each input field
        string attackerInput = attacker_army_input.text;
        string defenderInput = defender_army_input.text;

        // If the text in either input field is null or is not a number, output an error
        if (attackerInput == null || defenderInput == null || !int.TryParse(attackerInput, out _) || !int.TryParse(defenderInput, out _))
        {
            Debug.Log("Invalid input.");
        }
        else // Otherwise, print the output to the debug log
        {
            Debug.Log("Attacker is attacking with " + attackerInput + " armies\n " +
                        "Defender is defending with " + defenderInput + " armies");


            // I left your code here Sophia, in case you wanted to use it again when you came back to this

            /*
            // TODO: Set member fields.
            string attacker_army_count = attackerInput;
            string defender_army_count = defenderInput;
            Debug.Log("reached submit. attacker army count: " +
            attacker_army_count + " defender army count: " + defender_army_count); // TODO: delete later
            */

            // Turn off the display once the submit button has been pressed
            attackInputIsOnDisplay = false;
            attackInputPanel.gameObject.SetActive(false);
            viewCardsBtn.gameObject.SetActive(true);
            infoCardTMP.GetComponentInParent<RawImage>().gameObject.GetComponentInParent<Canvas>().gameObject.SetActive(true); // Re-activating all the info cards
        }
    }

    public void OnQuitGamePressed()
    {
        Debug.Log("Quitting Game...");
         #if UNITY_STANDALONE
            Application.Quit();
        #endif
         #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}