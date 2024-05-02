using UnityEngine;
using System.Collections.Generic; // Required for using Lists.
using UnityEngine.UI;

using TMPro;

public class GameHUDScript : MonoBehaviour
{
    /*****************************************************************************
    Game objects for the GameHUD
    ******************************************************************************/
    public GameObject cardDisplayPrefab;
    public Transform cardDisplayPanel;
    public Transform chooseCardDisplayPanel;
    public Transform endOfGamePanel;
    public Transform attackOrForitfyPanel;
    public Transform attackInputPanel;
    public Transform fortifyInputPanel;
    public Button viewCardsBtn;
    public TextMeshProUGUI eventCardTMP;
    public TextMeshProUGUI errorCardTMP;
    public TextMeshProUGUI infoCardTMP;
    public GameObject infoCardsDisplay;
    public SoundEffectsPlayer sfxPlayer;
    /*****************************************************************************
    End of game objects
    ******************************************************************************/

    /*****************************************************************************
    Booleans indicating which (if any) panel is on display, or what action
    the player wants to take.
    ******************************************************************************/
    public bool cardsAreOnDisplay = false;
    public bool attackInputIsOnDisplay = false;
    public bool attackOrFortifyOnDisplay = false;
    public bool fortifyInputOnDisplay = false;
    public bool wantsToReturn = false;
    public bool wantsToAttack = false;
    public bool wantsToFortify = false;
    public bool wantsToEndTurn = false;
    /*****************************************************************************
    End of booleans
    ******************************************************************************/

    /*****************************************************************************
    Data and input fields
    ******************************************************************************/
    public PlayerScript currentPlayer;
    public List<Card> selectedCards = new List<Card>();
    private int cardsInDisplay = 0;
    public int attacker_army_count = -1;
    public int defender_army_count = -1;
    public int fortify_army_count = -1;
    public TextMeshProUGUI playerTurnText;
    [SerializeField] InputField attacker_army_input;
    [SerializeField] InputField defender_army_input;
    [SerializeField] InputField fortify_input;
    /*****************************************************************************
    End of data and input fields
    ******************************************************************************/


    /*****************************************************************************
    Show panel functions.
    Activate the panel and set the proper flag.
    ******************************************************************************/
    /// <summary>
    /// Shows the panel that prompts players to select cards to turn in.
    /// </summary>
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
    private void Start()
    {
        sfxPlayer = GameObject.Find("HUDController").GetComponent<SoundEffectsPlayer>();
    }

    /// <summary>
    /// Called by ShowChooseCardPanel to create and show a given card.
    /// </summary>
    /// <param name="card"></param>
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
    /// <summary>
    /// Called when the player wants to see their hand. Creates a new card display for the given card.
    /// </summary>
    /// <param name="card"></param>
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
       public void ShowEndingPanel(int winnerNum) {
        endOfGamePanel.gameObject.SetActive(true);
        Debug.Log("Player " + winnerNum + " has conquered all the territories and won the game! GAME OVER.");
        sfxPlayer.PlayVictorySound();
    }
    /// <summary>
    /// Displays the panel that asks the player to attack, fortify, or end their turn.
    /// </summary>
    public void ShowAttackOrFortifyPanel() {
        // Reset flags.
        wantsToAttack = false;
        wantsToFortify = false;
        wantsToEndTurn = false;
        // display panel
        attackOrForitfyPanel.gameObject.SetActive(true);
        attackOrFortifyOnDisplay = true;
    }
    /// <summary>
    /// Displays the panel that asks the player how many armies they want to attack with.
    /// </summary>
    public void ShowAttackInputPanel() {
        attackInputIsOnDisplay = true;
        attackInputPanel.gameObject.SetActive(true);
        viewCardsBtn.gameObject.SetActive(false);
        infoCardsDisplay.SetActive(false); // Deactviating all the info cards
    }
    /// <summary>
    /// Disolays the panel that asks the player how many armies they want to move during fortification
    /// </summary>
    public void ShowFortifyInputPanel(){
        fortifyInputPanel.gameObject.SetActive(true);
        fortifyInputOnDisplay = true;
    }
    /*****************************************************************************
    End of show panel functions.
    ******************************************************************************/


    /*****************************************************************************
    On(Event) handlers.
    These functions are called when the player interacts with the gameHUD panels and buttons.
    ******************************************************************************/
    /// <summary>
    /// A listener that handles a card being clicked. Deselects cards that are 
    /// already selected, and selects cards that are not yet selected. If 3
    /// cards are selected, this function automatically attemtps to submit the set.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="cardDisplay"></param>
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
    /// <summary>
    /// This function is called when the "View Cards" button is pressed. It displays
    /// the player's hand.
    /// </summary>
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
            sfxPlayer.PlayErrorSound();

        }
    }
    /// <summary>
    /// This function is called when the player clicks the button to not turn in any cards
    /// and instead return to the game. It simply deactivates the display and resets flags.
    /// </summary>
    public void OnDoNotTurnInCardsPressed()
    {
        selectedCards.Clear();
        chooseCardDisplayPanel.gameObject.SetActive(false);
        cardsAreOnDisplay = false;
        wantsToReturn = true;
        Debug.Log("Player chose to not turn in any cards");
        sfxPlayer.PlayCorrectClickSFX();
    }
    /// <summary>
    /// This function is called when the player clicks the button to return to the game after
    /// viewing their cards. Simply deactivate the display and reset flags.
    /// </summary>
    public void OnBackToGamePressed(){
        cardDisplayPanel.gameObject.SetActive(false);
        cardsAreOnDisplay = false;
    }
    /// <summary>
    /// This function is called when the player attempts to submit the number of armies they
    /// want to fight with. It validates the input, and only deactivates the display on 
    /// valid input. 
    /// </summary>
    public void OnAttackInputSubmitPressed()    
    {
        viewCardsBtn.gameObject.SetActive(false); // Turning this button off so that it looks nicer
        
        // Reset values
        attacker_army_count = -1;
        defender_army_count = -1;

        // Getting the inputs from each input field
        string attackerInput = attacker_army_input.text;
        string defenderInput = defender_army_input.text;

        // If the text in either input field is null or is not a number, output an error
        if (attackerInput == null || defenderInput == null || !int.TryParse(attackerInput, out _) || !int.TryParse(defenderInput, out _))
        {
            Debug.Log("Invalid input.");
            sfxPlayer.PlayErrorSound();
        }
        else // Otherwise, print the output to the debug log
        {
            sfxPlayer.PlaySwordClashingSFX();
            Debug.Log("Attacker is attacking with " + attackerInput + " armies\n " +
                        "Defender is defending with " + defenderInput + " armies");

            attacker_army_count = int.Parse(attacker_army_input.text);
            defender_army_count = int.Parse(defender_army_input.text);

            // Turn off the display once the submit button has been pressed
            attackInputIsOnDisplay = false;
            attackInputPanel.gameObject.SetActive(false);
            viewCardsBtn.gameObject.SetActive(true); 
            infoCardsDisplay.SetActive(true); // Re-activating all the info cards
            //sfxPlayer.PlayCorrectClickSFX();
        }
    }
    /// <summary>
    /// This function is called when the player clicks the attack button that is displayed
    /// by the AttackOrFortifyPanel. It checks if there is a valid way for this player to attack.
    /// If there is no valid way — meaning the player has no territories with more than one army —
    /// it does nothing. Otherwise, it deactivates the AttackOrFOrtifyPanel and resets flags.
    /// </summary>
    public void OnAttackPressed()
    {
        Debug.Log("Attack!");
        // Verify they have an elligible territory to attack from: 
        foreach(TerritoryScript territory in currentPlayer.territoriesOwned){
            if(territory.armyCount > 1){
                territory.FillAdjTerritoriesList();
                // check that there is an adjacent territory that doesn't belong to this player
                foreach(Transform adj in territory.adjacentCountries){
                    if(adj.GetComponent<TerritoryScript>().occupiedBy != currentPlayer.playerNumber){
                        // Valid territory exists.
                        attackOrForitfyPanel.gameObject.SetActive(false);
                        attackOrFortifyOnDisplay = false;
                        wantsToAttack = true;
                        sfxPlayer.PlaySwordClashingSFX();
                        return;
                    }
                }
            }
        }
        Debug.Log("Player has no territories with sufficient armies to attack from");
    }
    /// <summary>
    /// This function is called when the player clicks the fortify button that is displayed
    /// by the AttackOrFortifyPanel. It checks if there is a valid way for this player to fortify.
    /// If there is no valid way — meaning the player has no territories with more than one army
    /// that are adjacent to another territory it occupies — it does nothing. 
    /// Otherwise, it deactivates the AttackOrFOrtifyPanel and resets flags.
    /// </summary>
    public void OnFortifyPressed()
    {
        // Verify that there is a valid way for the player to fortify:
        foreach(TerritoryScript terr in currentPlayer.territoriesOwned){
            if(terr.armyCount > 1){
                terr.FillAdjTerritoriesList(); // For some reason, the list is empty.
                // Check that the player owns an adjacent territory: 
                foreach(Transform adj in terr.adjacentCountries){
                    if(currentPlayer.territoriesOwned.Contains(adj.GetComponent<TerritoryScript>())){
                        // valid option exists!
                        attackOrForitfyPanel.gameObject.SetActive(false);
                        attackOrFortifyOnDisplay = false;
                        wantsToFortify = true;
                        sfxPlayer.PlayFortifySFX();
                        return;
                    }
                }
            }
        }
        Debug.Log("This player has no possible way to fortify.");
    }
    /// <summary>
    /// This function is called when the player clicks the End Turn button that is displayed
    /// by the AttackOrFortifyPanel. It simply deactivates the display and resets flags.
    /// </summary>
    public void OnEndTurnPressed()
    {
        attackOrForitfyPanel.gameObject.SetActive(false);
        attackOrFortifyOnDisplay = false;
        wantsToEndTurn = true;
    }
    /// <summary>
    /// This function is called when the player submits the number of armies they want to 
    /// fortify with. If the input is valid, it passes the input to the player and deactivates
    /// the display. Otherwise, it does nothing.
    /// </summary>
    public void OnSubmitFortifyInputPressed()
    {
        // Reset value
        fortify_army_count = -1;

        // Getting the inputs from each input field
        string fortifyInput = fortify_input.text;

        // If the text in either input field is null or is not a number, output an error
        if (fortifyInput == null || !int.TryParse(fortifyInput, out _))
        {
            // TODO: check that there are sufficient armies on the source territory
            Debug.Log("Invalid input.");
        }
        else // Otherwise, print the output to the debug log
        {
            fortify_army_count = int.Parse(fortify_input.text);
            // Check that there are sufficient armies on the territory
            if(currentPlayer.TerritoryMoveFrom.armyCount <= fortify_army_count){
                Debug.Log("Unable to move "  + fortify_army_count + " armies. Too many. Try again.");
                return;
            }
            else if(fortify_army_count < 0){
                 Debug.Log("Negative input is invalid. Try again.");
                 return;
            }

            // Otherwise, input is vald
            Debug.Log("Moving " + fortifyInput + " armies\n ");
            sfxPlayer.PlayFortifySFX();
            // Turn off the display once the submit button has been pressed
            fortifyInputPanel.gameObject.SetActive(false);
            fortifyInputOnDisplay = false;
        }
    }
    /// <summary>
    /// This function is called when the player chooses to quit the game. It kills the application.
    /// </summary>
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
    /*****************************************************************************
    End of On(Event) handlers.
    ******************************************************************************/
}