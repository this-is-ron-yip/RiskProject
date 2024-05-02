using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public class MapScript : MonoBehaviour
{
    /*****************************************************************************
    Game object data members
    ******************************************************************************/
    public Transform[] childObjects;
    public GameObject playerPrefab;
    public GameObject infantryPrefab;
    public GameObject cavalryPrefab;
    public GameObject artillaryPrefab;
    private GameHUDScript gameHUDScript;
    public SoundEffectsPlayer sfxPlayer;
    public List<PlayerScript> players = new List<PlayerScript>(); // List of players in the game
    public List<Transform> territories = new List<Transform>(); // List of territories on the map
    /*****************************************************************************
    End of game object data members
    ******************************************************************************/


    /*****************************************************************************
    Game data and trackers
    ******************************************************************************/
    public int playerTurn = -1; // Whose turn it is. 1 indexed just like the PlayerScript's playerNumber
    public int playerCount; // Number of players in the game
    public int startingPlayer = -1; // Which player starts, based on die rolls.
    public int sets_turned_in = 0; // Track the number of card sets turned in. 
    public int[] diceResults; // Stores the initial die rolls to determine play order
    public bool gameOver = false; // Flag to signal the end of the game.
    public string infoCardText; // Re-use when printing a message to both the debug log and HUD
    public event Action<int> OnPlayerConqueredAllTerritories; // An event that shoudl be invoked to end the game
    /*****************************************************************************
    End of game data and trackers
    ******************************************************************************/


    /*****************************************************************************
    Look up tables and enums
    ******************************************************************************/
    enum ArmyTypes { Infantry, Cavalry, Artillery };
    public Color[] colorArray;

    // How many armies to grant based on how many sets have been turned in.
    public static int[] ArmiesGrantedForCardSet = {4, 6, 8, 10, 12, 15}; 

    public Dictionary<Transform, List<Transform>> adjacencyList = // Tracks which territories are adjacent to each other
                            new Dictionary<Transform, List<Transform>>();
    
    // How many terrritories correspond to each continent
    public static Dictionary<TerritoryScript.Continents, int> ContinentTerritoryCounts =  
        new Dictionary<TerritoryScript.Continents, int> (){
            {TerritoryScript.Continents.NorthAmerica, 9}, {TerritoryScript.Continents.SouthAmerica, 4},
            {TerritoryScript.Continents.Europe, 7}, {TerritoryScript.Continents.Asia, 12}, 
            {TerritoryScript.Continents.Africa, 6}, {TerritoryScript.Continents.Australia, 4}
    };
    
    // How many armies to grant for controlling a certain continent
    public static Dictionary<TerritoryScript.Continents, int> ArmiesGrantedForContinent =  
        new Dictionary<TerritoryScript.Continents, int> (){
            {TerritoryScript.Continents.NorthAmerica, 5}, {TerritoryScript.Continents.SouthAmerica, 2},
            {TerritoryScript.Continents.Europe, 5}, {TerritoryScript.Continents.Asia, 7}, 
            {TerritoryScript.Continents.Africa, 3}, {TerritoryScript.Continents.Australia, 2}
    };
    /*****************************************************************************
    End of look up tables and enums
    ******************************************************************************/


    /*****************************************************************************
    Unity functions. 
    Called by the unity game engine, but implemented here.
    ******************************************************************************/
    /// <summary>
    /// Called by unity engine. Initialises the territory objects in the scene.
    /// </summary>
    private void OnValidate()
    {
        gameHUDScript = GameObject.FindWithTag("GameHUD").GetComponent<GameHUDScript>();
        FillTerritoriesList();
        FillAdjList();
    }
    /// <summary>
    /// Called by unity engine at the start of the scene. Initialises the player objects
    /// and starts the game.
    /// </summary>
    [Obsolete]
    private void Start()
    {
        SetColorKey(); // Set colors and gamehud key
        sfxPlayer = GameObject.Find("HUDController").GetComponent<SoundEffectsPlayer>();
        CreatePlayers();
        OnPlayerConqueredAllTerritories += HandlePlayerWonGame;
        StartCoroutine(AssignStartTerritories()); // Will in turn execute rest of the program.
    }
    /*****************************************************************************
    End of Unity functions.
    ******************************************************************************/


    /*****************************************************************************
    Initialisation functions. 
    Set up the objects in the scene. 
    ******************************************************************************/
    /// <summary>
    /// Instantiates playerScripts, based on the player count that was provided by the 
    /// previous scene. Provides handlers for the player's events, sets their data members, 
    /// and stores the created PlayerScripts in an array.
    /// </summary>
    private void CreatePlayers()
    {
        playerCount = StaticData.playerCount; //StaticData is a class I made inside the MainMenu Scene (used for transferring data between scenes)

        //Create players and instantiate them and give them respective playernumbers and infantry (after creating a player, call it's GivePlayerArmies() function)
        for (int i = 0; i < playerCount; i++)
        {
            GameObject newPlayer = Instantiate(playerPrefab, new Vector3(0, 0, 50), Quaternion.identity);
            newPlayer.name = $"Player {i+1}";
            PlayerScript newPlayerScript = newPlayer.GetComponent<PlayerScript>();
            newPlayerScript.playerNumber = i+1;
            int infCount = 50 - (playerCount * 5);
            // TODO: DELETE THE LINE BELOW LATER, give less armies for testing
            infCount = 3;
            newPlayerScript.infCount = infCount;

            // Set color for pieces:
            newPlayerScript.color = colorArray[newPlayerScript.playerNumber - 1];

            // Add listener for when player claims or attacks a territory
            newPlayerScript.OnPlayerClaimedTerritoryAtStart += HandleTerritoryClaimedAtStart;
            newPlayerScript.OnPlayerPlacesAnArmyAtStart += HandlePlacingAnArmy;
            newPlayerScript.OnPlayerPlacesAnArmyInGame += HandlePlacingAnArmy;
            newPlayerScript.OnPlayerSelectAttackFrom += HandleTerritoryToAttackFrom;
            newPlayerScript.OnPlayerSelectAttackOn += HandleTerritoryToAttackOn;
            newPlayerScript.OnPlayerSelectAttackOn += HandleTerritoryToAttackOn;
            newPlayerScript.OnPlayerSelectMoveFrom += HandleTerritoryToMoveFrom;
            newPlayerScript.OnPlayerSelectMoveTo += HandleTerritoryToMoveTo;
            newPlayerScript.OnRollDiceAtStart += HandleDiceRollAtStart;
            newPlayerScript.OnRollToBattle += HandleRollToBattle;
            newPlayerScript.OnPlayerDrawsCard += HandleDrawCard;
            players.Add(newPlayerScript);
        }

    }
    /// <summary>
    /// Fills adjacency lists (meaning adjacent territories) for each territory object in the scene.
    /// </summary>
    private void FillAdjList()
    {
        adjacencyList.Clear();
        foreach (Transform territory in territories)
        {
            adjacencyList.Add(territory, GameObject.FindWithTag(territory.gameObject.tag).GetComponent<TerritoryScript>().adjacentCountries);
        }
    }
    /// <summary>
    /// Fills the MapScript's transform list for all territories in the scene.
    /// </summary>
    private void FillTerritoriesList()
    {
        territories.Clear();
        childObjects = GetComponentsInChildren<Transform>();

        foreach (Transform territory in childObjects)
        {
            if (territory != this.transform)
            {
                territories.Add(territory);
            }
        }
    }
    /// <summary>
    /// Set the color array based on options inputs from the prevoius scene. 
    /// Also update the game hud player key to display the proper colors
    /// </summary>
    private void SetColorKey(){
        colorArray = StaticData.colorArray; // Set the array

        // After color array is set, gray out the unused players:
        for(int i =  StaticData.playerCount; i < 6; i++){
            colorArray[i] = Color.gray;
        }

        if(GameObject.Find("Player1Key") != null){
             GameObject.Find("Player1Key").GetComponent<RawImage>().color = colorArray[0];
        }
        else{
            Debug.Log("Player1Key is null");
        }

        if(GameObject.Find("Player2Key") != null){
             GameObject.Find("Player2Key").GetComponent<RawImage>().color = colorArray[1];
        }
        else{
            Debug.Log("Player2Key is null");
        }

        if(GameObject.Find("Player3Key") != null){
             GameObject.Find("Player3Key").GetComponent<RawImage>().color = colorArray[2];
        }
        else{
            Debug.Log("Player3Key is null");
        }

        if(GameObject.Find("Player4Key") != null){
             GameObject.Find("Player4Key").GetComponent<RawImage>().color = colorArray[3];
        }
        else{
            Debug.Log("Player4Key is null");
        }

        if(GameObject.Find("Player5Key") != null){
             GameObject.Find("Player5Key").GetComponent<RawImage>().color = colorArray[4];
        }
        else{
            Debug.Log("Player5Key is null");
        }

        if(GameObject.Find("Player6Key") != null){
             GameObject.Find("Player6Key").GetComponent<RawImage>().color = colorArray[5];
        }
        else{
            Debug.Log("Player6Key is null");
        }
    }
    /*****************************************************************************
    End of initialization functions 
    ******************************************************************************/


    /*****************************************************************************
    Player Turn functions.
    Handle the main logic for executing certain steps of a player's turn.
    Call on other, smaller functions to achieve this.
    ******************************************************************************/
    /// <summary>
    /// Handles the game setup by determining the play order and requiring players to
    /// place all of their initial armies, according to the game rules. Then calls
    /// the EnterGamePlay() function
    /// </summary>
    /// <returns> private IEnumerator </returns>
    [Obsolete]
    private IEnumerator AssignStartTerritories()
    {
        // Step one: get dice results for all players to determine who starts
        diceResults = new int[playerCount];
        for(int i = 0; i < playerCount; i++){
            // initialize to -1 for later reference.
            diceResults[i] = -1;
        }
        
        int completed_roll = 0;
        playerTurn = 1; // start from 1st player
        gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
        while(completed_roll < playerCount){
            // permit this player to roll
            players[playerTurn - 1].canRollToStart = true;

            // Wait for the player to take an action
            yield return StartCoroutine(WaitForDieRoll());

            // They didn't roll the die successfully
            if(diceResults[playerTurn - 1] == -1){
                continue;
            }
            
            // Remove permission
            players[playerTurn - 1].canRollToStart = false;
            // update player turn: 
            if(playerTurn == playerCount){
                // cycle back to start of player list
                playerTurn = 1;
                gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
            }
            else{
                // otherwise, simply move to the next player
                playerTurn++;
                gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
            }
            completed_roll++; // success!
        }

        //Whoever lands highest gets to start choosing first
        int highestNumIndex = 0;
        for (int i = 1; i < diceResults.Length; i++)
        {
            if (diceResults[i] > diceResults[highestNumIndex])
            {
                highestNumIndex = i;
            }
        }
        playerTurn = highestNumIndex + 1;
        gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
        startingPlayer = playerTurn; // for future reference

        // Step two: Allow players to claim territories to start
        //Player picks unoccupied country to place 1 infantry, therefore occupying that country
        int territories_left = TerritoryScript.NUMBER_OF_TERRITORIES;
        int irrelevantCounter = 0;
        while (territories_left > 0){

            // Displaying information in the info card correctly
            if (irrelevantCounter == 0)
            {
                gameHUDScript.infoCardTMP.text += "\nTotal territories left to be placed: " + territories_left;
                irrelevantCounter = 1;
            } 
            else
            {
                gameHUDScript.infoCardTMP.text = "Total territories left to be placed: " + territories_left;
            }
            players[playerTurn - 1].canClaimTerritoryAtStart = true;

            // store in temp variable for later reference
            int player_starting_territories = players[playerTurn - 1].territoriesOwned.Count;

            yield return StartCoroutine(InitialiseStartingInfantry());

            // check that the claiming was successful by checking the player's territory count
            // if it wasn't, don't update anything and try again
            int player_ending_territories = players[playerTurn - 1].territoriesOwned.Count;
            if(player_ending_territories == player_starting_territories){
                continue;
            }

            // update remanaining territories
            territories_left--;

            // Revoke permission
            players[playerTurn - 1].canClaimTerritoryAtStart = false;

            // update next player's turn: 
            if(playerTurn == playerCount){
                // cycle back to start of player list
                playerTurn = 1;
                gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
            }
            else{
                // otherwise, simply move to the next player
                playerTurn++;
                gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
            }
        }

        // Step three: players place remaining pieces on their claimed territories
        while(players[playerTurn - 1].GetArmyCountTotal() != 0){
            players[playerTurn - 1].canPlaceArmyAtStart = true; 

            // Store in temp variable for later reference
            int player_starting_armies = players[playerTurn - 1].GetArmyCountTotal();

            yield return StartCoroutine(InitialiseStartingInfantry());

            // check that the placing army was successful by checking the player's army count
            // if it wasn't, don't update anything and try again
            // else, instantiate the infantry object and make the territory the colour of the player
            int player_ending_armies = players[playerTurn - 1].GetArmyCountTotal();
            if(player_ending_armies == player_starting_armies){
                continue;
            }

            // Revoke permission
            players[playerTurn - 1].canPlaceArmyAtStart = false;

            // update next player's turn: 
            if(playerTurn == playerCount){
                // cycle back to start of player list
                playerTurn = 1;
                gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
            }
            else{
                // otherwise, simply move to the next player
                playerTurn++;
                gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
            }
        }
            
        // Completed game set up!
        yield return StartCoroutine(EnterGamePlay());
    }
    /// <summary>
    /// This function is called after the game set up has been completed. It handles
    /// rotating through player turns, prompting them for the appropriate action or input.
    /// Once this function returns, the game is over.
    /// </summary>
    /// <returns></returns>
    [Obsolete]
    private IEnumerator EnterGamePlay(){
        gameHUDScript.infoCardTMP.text = "Entered game play!";
        int playerTurn = startingPlayer;
        gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";

        while (!gameOver){
            // update player and give them the turn
            PlayerScript player = players[playerTurn - 1];
            // Prevent all permissions at start of each loop
            player.ResetAllPermissions();
            // Update gamehud's player
            FindAnyObjectByType<GameHUDScript>().currentPlayer = players[playerTurn-1];

            // Check if this player has been eliminated before proceeding: 
            if(player.eliminated){
                Debug.Log("Player " + playerTurn + " is out of the game. Skipping their turn.");
                gameHUDScript.infoCardTMP.text = "Player " + playerTurn + " is out of the game. Skipping their turn.";
                // update the player
                if (playerTurn == playerCount){
                    // cycle back to start of player list
                    playerTurn = 1;
                    gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
                }
                else{
                    // otherwise, simply move to the next player
                    playerTurn++;
                    gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
                }
                continue;
            }

            // Step one: getting and placing armies
            /// calculate the number of armies this player should receive based on territories
            GrantArmiesFromTerritories(playerTurn);

            // Require player to place the new armies
            while(player.GetArmyCountTotal() != 0){
                player.canPlaceArmyInGame = true;
                yield return StartCoroutine(InitialiseStartOfTurnInfantry(playerTurn));
                Debug.Log("Player " + playerTurn + " has " + player.GetArmyCountTotal() + 
                    " armies left");
                gameHUDScript.infoCardTMP.text = "Player " + playerTurn + " has " + player.GetArmyCountTotal() +
                    " armies left";

                // Revoke permission
                player.canPlaceArmyInGame = false;
            }


            // calculate and grant armies from controlling continents
            GrantArmiesFromContinents(playerTurn);
            // Require player to place their armies
            while(player.GetArmyCountTotal() != 0){
                player.canPlaceArmyInGame = true;
                yield return StartCoroutine(InitialiseStartOfTurnInfantry(playerTurn));
                Debug.Log("Player " + playerTurn + " has " + player.GetArmyCountTotal() + 
                    " armies left");
                gameHUDScript.infoCardTMP.text = ("Player " + playerTurn + " has " + player.GetArmyCountTotal() +
                    " armies left");

                // Revoke permission
                player.canPlaceArmyInGame = false;
            }

            // Step two: allow player to turn in sets of cards. give additional armies accordingly
            if(player.cardsInHand.Count >= 3)
            {
                // Allow player to turn in cards
                infoCardText = "Player: " + GameObject.FindAnyObjectByType<GameHUDScript>().currentPlayer.playerNumber +
                         " Select cards to turn in.";
                Debug.Log(infoCardText);
                gameHUDScript.eventCardTMP.text = infoCardText;
                gameHUDScript.ShowChooseCardPanel();
                // Once display is inactive, assume we are done with this phase.
                yield return StartCoroutine(WaitForChooseCardDisplayInactive());
            }
            
            // Require players to turn in until they have less than 5 cards.
            while (player.cardsInHand.Count >= 5)
            {
                // Allow player to turn in cards
                infoCardText = "Player: " + GameObject.FindAnyObjectByType<GameHUDScript>().currentPlayer.playerNumber +
                         " Still has more than four cards. Select cards to turn in.";
                Debug.Log(infoCardText);
                gameHUDScript.eventCardTMP.text = infoCardText;
                gameHUDScript.ShowChooseCardPanel();
                // Once display is inactive, assume we are done with this phase.
                yield return StartCoroutine(WaitForChooseCardDisplayInactive());
            }

            // Allow them to turn in more cards as long as they have at least three
            while (player.cardsInHand.Count >= 3 && !gameHUDScript.wantsToReturn)
            {
                gameHUDScript.ShowChooseCardPanel();
                // Once display is inactive, assume we are done with this phase.
                yield return StartCoroutine(WaitForChooseCardDisplayInactive());
            }

            // Require player to place armies earned from card sets
            while(player.GetArmyCountTotal() != 0){
                player.canPlaceArmyInGame = true;
                yield return StartCoroutine(InitialiseStartOfTurnInfantry(playerTurn));
                Debug.Log("Player " + playerTurn + " has " + player.GetArmyCountTotal() + 
                    " armies left");
                gameHUDScript.infoCardTMP.text = "Player " + playerTurn + " has " + player.GetArmyCountTotal() +
                    " armies left";

                // Revoke permission
                player.canPlaceArmyInGame = false;
            }

            // Step three: let the player attack multiple times
            gameHUDScript.wantsToEndTurn = false;
            while(!gameHUDScript.wantsToEndTurn){
                gameHUDScript.ShowAttackOrFortifyPanel();
                yield return WaitForAttackOrFortifyInactive();
            
                if(gameHUDScript.wantsToAttack){
                    yield return LaunchAnAttack(playerTurn);
                }
                else if(gameHUDScript.wantsToFortify){
                    yield return FortifyPosition(playerTurn);
                    break; // After one fortification, end the turn. 
                }
                // Otherwise, do nothing, aka exit the while loop
            }
            
            // If the player has claimed at least one territory during their turn
            // Prompt and allow/require them to draw a card from the deck
            bool player_must_draw = player.wonTerritory;
            while(player_must_draw){
                Debug.Log("Player " + playerTurn + " won a territory this round. Draw a card from the deck");
                gameHUDScript.eventCardTMP.text = "Player " + playerTurn + " won a territory this round. Draw a card from the deck";
                int cardsBeforeDraw = player.cardsInHand.Count;
                player.clickExpected = true; // figure out why we need this line (should be handled by waitforplayer)
                player.canDraw = true;
                yield return StartCoroutine(WaitForPlayerToDoMove(player));
                int cardsAfterDraw = player.cardsInHand.Count;
                if(cardsBeforeDraw != cardsAfterDraw){
                    // Task accomplished
                    player.canDraw = false;
                    player_must_draw = false;
                }
                else{
                    Debug.Log("Must draw a card. Try again.");
                    gameHUDScript.errorCardTMP.text = "Must draw a card. Try again.";
                    sfxPlayer.PlayErrorSound();
                }
            }

            Debug.Log("End of Player " + playerTurn + "'s turn");
            gameHUDScript.infoCardTMP.text = "End of Player " + playerTurn + "'s turn";

            // update the player
            if (playerTurn == playerCount){
                // cycle back to start of player list
                playerTurn = 1;
                gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
            }
            else{
                // otherwise, simply move to the next player
                playerTurn++;
                gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
            }
        }
    }
    /// <summary>
    /// This function is called when a player wishes to launch an attack. It requests and extracts
    /// wehre the player would like to attack from, where they would like to attack on, and with 
    /// how many armies they would like to attack. It then executes the attack and updates all 
    /// appropriate data based on the results.
    /// </summary>
    /// <param name="player_id"></param>
    /// <returns></returns>
    private IEnumerator LaunchAnAttack(int player_id)
    {
        Debug.Log("Player: " + player_id + ", launch an attack.");
        gameHUDScript.eventCardTMP.text = "Player: " + player_id + ", launch an attack.";
        PlayerScript player = players[player_id - 1];
        // Clear past values:
        player.TerritoryAttackingFrom = null;
        player.TerritoryAttackingOn = null;

        // Part one: pick an owned territory to attack from
        player.canSelectAttackFrom = true;
        while(player.TerritoryAttackingFrom == null)
        {
            playerTurn = player_id; // We need to reset the playerTurn
            gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
            yield return StartCoroutine(WaitForAttackFromTerritory());
        }
        player.canSelectAttackFrom = false;

        player.canSelectAttackOn = true;
        while(player.TerritoryAttackingOn == null) { // Until valid input is received
            playerTurn = player_id; // Override in case playerTurn has changed unpredictably
            gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
            yield return StartCoroutine(WaitForAttackOnTerritory());
        }
        player.canSelectAttackOn = false;

        // Part three: roll the dice
        // Ask the defender and attacker how many armies they would like to use
        int attacker_army_count = -1;
        int defender_army_count = -1;
        bool valid_input = false;
        while(!valid_input){
            gameHUDScript.ShowAttackInputPanel();
            yield return WaitForAttackInputPanelInactive(); // Get input from panel
            attacker_army_count = gameHUDScript.attacker_army_count;
            defender_army_count = gameHUDScript.defender_army_count;
            // Check defender input: 
            if(defender_army_count != 1 && defender_army_count != 2){
                Debug.Log("Defender must choose to fight with one or two armies.");
                gameHUDScript.errorCardTMP.text = "Defender must choose to fight with one or two armies.";
                sfxPlayer.PlayErrorSound();
            }
            else if(player.TerritoryAttackingOn.armyCount < defender_army_count){
                Debug.Log("Defender does not have sufficient armies for this input.");
                gameHUDScript.errorCardTMP.text = "Defender does not have sufficient armies for this input.";
                sfxPlayer.PlayErrorSound();
            } 
            else if(attacker_army_count !=1 && attacker_army_count != 2 && attacker_army_count != 3)  {
                Debug.Log("Attacker must choose to fight with one, two, or three armies.");
                gameHUDScript.errorCardTMP.text = "Attacker must choose to fight with one, two, or three armies.";
                sfxPlayer.PlayErrorSound();
            }
            else if(player.TerritoryAttackingFrom.armyCount <= attacker_army_count){
                Debug.Log("Attacker has selected too many armies. Must leave one army " + 
                    " to occupy its starting territory.");
                gameHUDScript.errorCardTMP.text = "Attacker has selected too many armies. Must leave one army " +
                    " to occupy its starting territory.";
                sfxPlayer.PlayErrorSound();
            }
            else{
                valid_input = true;
            }
        }

        // Part four: evaluate the outcome of the attack 
        yield return EvaluateAttack(player_id, attacker_army_count, defender_army_count);

        /* From the game rules, after eliminating an opponent:
        You must immediately trade in enough sets to reduce your hand to 4 or fewer cards, 
        but once your hand is reduced to 4,3, or 2 cards, you must stop trading.
        But if winning them gives you fewer than 6, you must wait until the
        beginning of your next turn to trade in a set.
        */
        bool firstRep = true;
        while(player.cardsInHand.Count >= 6) // Will only happen if they elimintaed opponent
        {
            Debug.Log((firstRep? "After eliminating your opponenent, you have more than 6 cards." :
                "You still have more than 6 cards.") +
            " Select which cards you would like to turn in.");
            // Allow player to turn in cards
            gameHUDScript.ShowChooseCardPanel();
            // Once display is inactive, assume we are done with this phase.
            yield return WaitForChooseCardDisplayInactive();
            firstRep = false;
        }
    }
    /// <summary>
    /// Given the amount of armies both sides wish to battle with, this function generates
    /// the die rolls and determines who won each match up. It then modifies the number of armies
    /// on each territory, and checks to see if the attacker has claimed the territory. In this case,
    /// it transfers the territory from the defender to the attacker.
    /// </summary>
    /// <param name="attacker_id"></param>
    /// <param name="attacker_dice"></param>
    /// <param name="defender_dice"></param>
    /// <returns>IEnumerator</returns>
    private IEnumerator EvaluateAttack(int attacker_id, int attacker_dice, int defender_dice)
    {
        PlayerScript player = players[attacker_id - 1];

        TerritoryScript PlayerTerritory = player.TerritoryAttackingFrom;
        TerritoryScript EnemyTerritory = player.TerritoryAttackingOn;
        PlayerScript defender = players[EnemyTerritory.occupiedBy - 1];

        // Generate the die rolls.
        List<int> attacker_rolls = new List<int>();
        List<int> defender_rolls = new List<int>();

        for(int i = 0; i < attacker_dice; i++){
            // Reset die result
            player.battleDiceResults = -1;
            // permit this player to roll
            player.canRollToBattle = true;
            // Wait for the player to take an action
            while(player.battleDiceResults == -1){
                yield return StartCoroutine(WaitForDieRoll());
            }
            // Remove permission
            player.canRollToBattle = false;
            // Set the result array
            attacker_rolls.Add(player.battleDiceResults);
        }
        for(int i = 0; i < defender_dice; i++){
            // Reset die roll arrays
            defender.battleDiceResults = -1;
            // permit this player to roll
            defender.canRollToBattle = true;
            // Wait for the player to take an action
            while(defender.battleDiceResults == -1){
                Debug.Log($"Player {defender.playerNumber}, click the dice to roll.");
                gameHUDScript.eventCardTMP.text = $"Player {defender.playerNumber}, click the dice to roll.";
                defender.clickExpected = true;
                yield return StartCoroutine(WaitForPlayerToDoMove(defender));
            }
            // Remove permission
            defender.canRollToBattle = false;
            // Set the result array
            defender_rolls.Add(player.battleDiceResults);
        }

        // Sort from highest to lowest. Sorts in ascending order by default, so reverse
        attacker_rolls.Sort();
        attacker_rolls.Reverse();
        defender_rolls.Sort();
        defender_rolls.Reverse();

        // Determine winner
        int one_v_ones = Math.Min(attacker_dice, defender_dice);
        infoCardText = "Evaluating...\n";
        int remaining_attackers = attacker_dice;
        for(int i = 0; i < one_v_ones; i++){
            infoCardText += attacker_rolls[i] +
                    " vs " + defender_rolls[i];
            if (attacker_rolls[i] > defender_rolls[i]){
                // Defender loses one army on the territory being attacked.
                Debug.Log("Attacker wins this match up!");
                infoCardText += ": W\n";
                EnemyTerritory.armyCount--;
            }
            else{ // Defender wins on a tie
                // Attacker loses one of the armies they sent to attack
                Debug.Log("Defender wins this match up!");
                infoCardText += ": L\n";
                PlayerTerritory.armyCount--;
                remaining_attackers--;
            }
        }
        gameHUDScript.infoCardTMP.text = infoCardText;

        // Check if the attacker claimed the territory
        if(EnemyTerritory.armyCount <= 0){ // should not be negative, but just in case
            player.wonTerritory = true;
            EnemyTerritory.occupiedBy = attacker_id; // update territories variables
            player.territoriesOwned.Add(EnemyTerritory);
            player.territoryCountsPerContinent[EnemyTerritory.continent] += 1;
            defender.territoriesOwned.Remove(EnemyTerritory);
            defender.territoryCountsPerContinent[EnemyTerritory.continent] -= 1;

            // Attacker must leave surviving armies on the territory they won
            EnemyTerritory.armyCount = remaining_attackers;

            if(PlayerTerritory.armyCount > 1){
                infoCardText = "How many additional armies do you wish to move from the attacking territory to the " +
                "territory you just conquered? You may select 0.";
                Debug.Log(infoCardText);
                gameHUDScript.eventCardTMP.text = infoCardText;

                // Ask how many armies they would like to move.
                int fortify_army_count = -1;
                player.TerritoryMoveFrom = PlayerTerritory;
                player.TerritoryMoveTo = EnemyTerritory;
                while(fortify_army_count == -1){
                    gameHUDScript.ShowFortifyInputPanel();
                    yield return WaitForFortifyInputInactive(); // Get input from panel
                    fortify_army_count = gameHUDScript.fortify_army_count; // Should be validated
                }

                PlayerTerritory.armyCount -= fortify_army_count;
                EnemyTerritory.armyCount += fortify_army_count;

                // Reset
                player.TerritoryMoveFrom = null;
                player.TerritoryMoveTo = null;
            }
        }

        // Reset member variables
        player.TerritoryAttackingFrom = null;
        player.TerritoryAttackingOn = null;

        // Check if the attacker has won the game
        if(player.territoriesOwned.Count == TerritoryScript.NUMBER_OF_TERRITORIES){
            OnPlayerConqueredAllTerritories?.Invoke(player.playerNumber);
        }

        // Check if the defendant has been eliminated (no territories left)
        if(defender.territoriesOwned.Count == 0){
            defender.eliminated = true;
            // Give defender's cards to the attacker:
            player.cardsInHand.AddRange<Card>(defender.cardsInHand);
            // Reset their color on the game hud: 
            GameObject.Find("Player" + defender.playerNumber + "Key").GetComponent<RawImage>().color = Color.gray;

        }

        // TODO: delete later. for testing the final game screen only
        // OnPlayerConqueredAllTerritories?.Invoke(player.playerNumber);
    }
    /// <summary>
    /// This function is called when a player is fortifying at the end of their turn. It 
    /// requests and extracts which territory to move from, which territory to move to, and 
    /// how many armies to move. It then modifies the proper data based on these inputs. The 
    /// player may only fortify once at the end of their turn.
    /// </summary>
    /// <param name="player_id"></param>
    /// <returns></returns>
    private IEnumerator FortifyPosition(int player_id){
        PlayerScript player = players[player_id - 1];
        // Clear past values:
        player.TerritoryMoveFrom = null;
        player.TerritoryMoveTo = null;

        // Part one: pick an owned territory to move from
        player.canSelectMoveFrom = true;
        while(player.TerritoryMoveFrom == null)
        {
            playerTurn = player_id; // We need to reset the playerTurn, which is being updated when it shouldn't be
            gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
            yield return StartCoroutine(WaitForMoveFromTerritory());
        }
        player.canSelectMoveFrom = false;
        gameHUDScript.infoCardTMP.text = "Player chose to move from : " + player.TerritoryMoveFrom;
        Debug.Log("Player chose to move from : " + player.TerritoryMoveFrom);

        player.canSelectMoveTo = true;
        while(player.TerritoryMoveTo == null) { // Until valid input is received
            playerTurn = player_id; // Override in case playerTurn has changed unpredictably
            gameHUDScript.playerTurnText.text = $"Player turn: {playerTurn}";
            yield return StartCoroutine(WaitForMoveToTerritory());
        }
        player.canSelectMoveTo = false;
        Debug.Log("Player chose to move to : " + player.TerritoryMoveTo);
        gameHUDScript.infoCardTMP.text = "Player chose to move to : " + player.TerritoryMoveFrom;

        // Ask how many armies they would like to move.
        int fortify_army_count = -1;
        while(fortify_army_count == -1){
            gameHUDScript.ShowFortifyInputPanel();
            yield return WaitForFortifyInputInactive(); // Get input from panel
            fortify_army_count = gameHUDScript.fortify_army_count; // Should be validated
        }

        player.TerritoryMoveFrom.armyCount -= fortify_army_count;
        player.TerritoryMoveTo.armyCount += fortify_army_count;
    }
    /*****************************************************************************
    End of player turn functions.
    ******************************************************************************/


    /*****************************************************************************
    WaitFor (player to do some action) functions. 
    Set the player permissions according to the expected event and await a valid click. 
    ******************************************************************************/
    /// <summary>
    /// Waits for a player to reset its clickExpected field, meaning the PlayerScript
    /// has detected a click. Used by many other functions.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitForPlayerToDoMove(PlayerScript player)
    {
        yield return new WaitUntil(() => !player.clickExpected);
    }
    /// <summary>
    /// Waits for a click on the dice.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitForDieRoll(){
        Debug.Log($"Player {playerTurn}, click the dice to roll.");
        gameHUDScript.eventCardTMP.text = $"Player {playerTurn}, click the dice to roll.";
        PlayerScript player = players[playerTurn - 1];
        player.clickExpected = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }
    /// <summary>
    /// Waits for a click on a territory. Permissions are set before this function is called
    /// rather than in the function itself, becuase this function is used during two set up steps:
    /// claiming territories at the start, and placing remaining armies on already claimed territories.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator InitialiseStartingInfantry()
    {
        Debug.Log($"Player {playerTurn}, choose a territory to place 1 infantry on.");
        gameHUDScript.eventCardTMP.text = $"Player {playerTurn}, choose a territory to place 1 infantry on.";
        PlayerScript player = players[playerTurn - 1];
        player.clickExpected = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }
    /// <summary>
    /// Waits for player to place one of the armies they were granted at the start of their turn.
    /// </summary>
    /// <param name="player_id"></param>
    /// <returns></returns>
    private IEnumerator InitialiseStartOfTurnInfantry(int player_id){
        PlayerScript player = players[player_id - 1];
        gameHUDScript.eventCardTMP.text = $"Player {player_id}, place your infantries. You have {player.GetArmyCountTotal()} left";
        Debug.Log($"Player {player_id}, where would you like to place one infantry?");
        player.clickExpected = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }
    /// <summary>
    /// Waits for a click on a territory that the current player can attack from.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitForAttackFromTerritory()
    {
        gameHUDScript.eventCardTMP.text = $"Player {playerTurn}, click an owned territory to attack from.";
        PlayerScript player = players[playerTurn - 1];
        player.clickExpected = true;
        player.canSelectAttackFrom = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }
    /// <summary>
    /// Waits for a click on a territory that the current player can attack on.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitForAttackOnTerritory()
    {
        gameHUDScript.eventCardTMP.text = $"Player {playerTurn}, click an enemy territory to attack on.";
        Debug.Log($"Player {playerTurn}, click an enemy territory to attack on.");

        PlayerScript player = players[playerTurn - 1];
        player.clickExpected = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }
    /// <summary>
    /// Waits for a click on a territory that the current player move armies from.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitForMoveFromTerritory()
    {
        gameHUDScript.eventCardTMP.text = $"Player {playerTurn}, click an owned territory to move from.";
        PlayerScript player = players[playerTurn - 1];
        player.clickExpected = true;
        player.canSelectMoveFrom = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }
    /// <summary>
    /// Waits for a click on a territory that the current player move armies to.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitForMoveToTerritory()
    {
        gameHUDScript.eventCardTMP.text = $"Player {playerTurn}, click an owned territory to move to.";
        PlayerScript player = players[playerTurn - 1];
        player.clickExpected = true;
        player.canSelectMoveTo = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }
    /*****************************************************************************
    End of WaitFor (player to do some action) functions. 
    ******************************************************************************/


    /*****************************************************************************
    WaitFor (gamehud displays to become inactive) functions. 
    If the display is inactive, we know we've received some input and can 
    move onto the next part of the game.
    ******************************************************************************/
    /// <summary>
    /// Waits for cardsAreOnDisplay to be false
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitForChooseCardDisplayInactive()
    {
        yield return new WaitUntil(() => 
                !gameHUDScript.cardsAreOnDisplay);
    }
    /// <summary>
    /// Waits for fortifyInputOnDisplay to be false, meaning the player has entered
    /// a valid number of armies to fortify with.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitForFortifyInputInactive()
    {
        yield return new WaitUntil(() => 
                !gameHUDScript.fortifyInputOnDisplay);
    }
    /// <summary>
    /// Waits for attackOrFortifyOnDisplay to be false, meaning the player has
    /// chosen to attack, fortify, or end their turn.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitForAttackOrFortifyInactive()
    {
        yield return new WaitUntil(() => 
                !gameHUDScript.attackOrFortifyOnDisplay);
    }
    /// <summary>
    /// Waits for attackInputIsOnDisplay to be false, meaning the player has
    /// entered a valid number of armies to attack with.
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitForAttackInputPanelInactive()
    {
        yield return new WaitUntil(() => 
                !gameHUDScript.attackInputIsOnDisplay);
    }
    /*****************************************************************************
    End of WaitFor (gamehud displays to become inactive) functions. 
    ******************************************************************************/


    /*****************************************************************************
    Grant army functions.
    ******************************************************************************/
    /// <summary>
    /// Calculates and gratns armies based on the terrirotries owned by the current player.
    /// Called at the start of every turn.
    /// </summary>
    /// <param name="player_id"></param>
    private void GrantArmiesFromTerritories(int player_id){
        PlayerScript player = players[player_id - 1];

        // Count the number of territories you occupy, divide by three and round down. 
        player.infCount += Math.Max(player.territoriesOwned.Count / 3, 3);

        gameHUDScript.infoCardTMP.text = "Player " + player_id + " occupies " + player.territoriesOwned.Count +
        " territories and has been granted " +
                player.GetArmyCountTotal() + " armies";
        Debug.Log("Player " + player_id + " occupies " + player.territoriesOwned.Count + 
        " territories and has been granted " +
                player.GetArmyCountTotal() + " armies");
    }
    /// <summary>
    /// Calculates and gratns armies based on the continents owned by the current player.
    /// Called at the start of every turn.
    /// </summary>
    /// <param name="player_id"></param>
    private void GrantArmiesFromContinents(int player_id){
        int infantry = 0;
        PlayerScript player = players[player_id - 1];

        // Add armies for each territory owned
        foreach(KeyValuePair<TerritoryScript.Continents, int> count in player.territoryCountsPerContinent){
            // This player controls the continent
            if(count.Value == ContinentTerritoryCounts[count.Key] ){
                // Add proper number of armies
                infantry += ArmiesGrantedForContinent[count.Key];
                Debug.Log("Player " + player_id + " controls " + count.Key + " and has been granted" + 
                    ArmiesGrantedForContinent[count.Key] + " additional armies");
            }
        }
    }
    /*****************************************************************************
    End of grant army functions.
    ******************************************************************************/


    /*****************************************************************************
    Handle functions.
    The PlayerScript invokes these functions on events, like clicks.
    The GameHUDScript invokes these functions on events, like button presses.
    ******************************************************************************/
    /// <summary>
    /// On a dice click at the very start of the game (to determine play order), 
    /// roll the die and store the results in the diceResults array.
    /// </summary>
    /// <param name="player_id"></param>
    /// <param name="die"></param>
    private void HandleDiceRollAtStart(int player_id, GameObject dice){
        DiceRollerScript dice_rolled = dice.GetComponent<DiceRollerScript>();
        int result = dice_rolled.RollDice();
        diceResults[player_id - 1] = result;
        Debug.Log("Player " + player_id + " rolled a " + result);
        gameHUDScript.infoCardTMP.text = "Player " + player_id + " rolled a " + result;
    }
    /// <summary>
    /// On a dice click during at attack, set the player's data member for dice result
    /// </summary>
    /// <param name="player_id"></param>
    /// <param name="dice"></param>
    private void HandleRollToBattle(int player_id, GameObject dice){
        DiceRollerScript dice_rolled = dice.GetComponent<DiceRollerScript>();
        int result = dice_rolled.RollDice();
        players[player_id - 1].battleDiceResults = result;
        Debug.Log("Player " + player_id + " rolled a " + result);
        gameHUDScript.infoCardTMP.text = "Player " + player_id + " rolled a " + result;
    }
    /// <summary>
    /// Verify that the given territory is unclaimed, then update the player's data members
    /// to reflect that they have claimed this territory. To be called during game set up.
    /// </summary>
    /// <param name="player_id"></param>
    /// <param name="territory"></param>
    private void HandleTerritoryClaimedAtStart(int player_id, GameObject territory){
        PlayerScript curr_player = players.Single(player => player.playerNumber == player_id);

        // Find the territory by name aka tag. If not found, do nothing
        TerritoryScript claimed_territory = territory.GetComponent<TerritoryScript>();
        
        // Update the territory's owner
        if(claimed_territory == null){
            Debug.Log("Tag does not match a known territory");
            gameHUDScript.errorCardTMP.text = "Tag does not match a known territory";
            sfxPlayer.PlayErrorSound();
            return;
        }
        if(claimed_territory.occupiedBy != -1){
            Debug.Log("Territory already claimed. Occupied by Player " + claimed_territory.occupiedBy);
            gameHUDScript.errorCardTMP.text = "Territory already claimed. Occupied by Player " + claimed_territory.occupiedBy;
            sfxPlayer.PlayErrorSound();
            // someone else is occupying, do nothing
            return;
        }
        else{
            // add territory to player list:
            claimed_territory.occupiedBy = player_id;
            curr_player.territoriesOwned.Add(claimed_territory);
            curr_player.territoryCountsPerContinent[claimed_territory.continent] += 1;

            // Add one to the troops on this territory, since this function is only used at the start
            claimed_territory.armyCount++;
            curr_player.infCount--;
            infoCardText = territory.tag + " is occupied by Player " + player_id + " and has " + claimed_territory.armyCount + " armies." +
               " Player " + player_id + " now has " + curr_player.territoryCountsPerContinent[claimed_territory.continent] + " on " +
               " territories on the continent of " + claimed_territory.continent + ". Player " + player_id + " has " +
                curr_player.infCount + " infantry remaining.";
            Debug.Log(infoCardText);
            gameHUDScript.eventCardTMP.text = infoCardText;
            // spawn army (this step should always happen last!!!): 
            // SpawnArmyPiece(ArmyTypes.Infantry, territory, player_id);
        }
    }
    /// <summary>
    /// Given a player and territory, place an army on that territory if it is owned
    /// by that player. To be called after a player has received new armies at the start
    /// of their turn.
    /// </summary>
    /// <param name="player_id"></param>
    /// <param name="territory"></param>
    private void HandlePlacingAnArmy(int player_id, GameObject territory){
        PlayerScript curr_player = players.Single(player => player.playerNumber == player_id);

        // Find the territory by name aka tag. If not found, do nothing
        TerritoryScript claimed_territory = territory.GetComponent<TerritoryScript>();
        
        // Update the territory's owner
        if(claimed_territory == null){
            infoCardText = "Tag does not match a known territory";
            Debug.Log(infoCardText);
            gameHUDScript.errorCardTMP.text = infoCardText;
            sfxPlayer.PlayErrorSound();
            return;
        }
        if(claimed_territory.occupiedBy != player_id){
            infoCardText = claimed_territory.tag + " is not claimed by this player.";
            Debug.Log(infoCardText);
            gameHUDScript.errorCardTMP.text = infoCardText;
            sfxPlayer.PlayErrorSound();
            // only allowed to add to already claimed territories
            return;
        }
        else{
            //update the territory's armyCount
            claimed_territory.armyCount++;
            curr_player.infCount--;
            Debug.Log(claimed_territory.tag + " is occupied by Player " + player_id + " and has " + claimed_territory.armyCount + " armies");
            gameHUDScript.infoCardTMP.text = claimed_territory.tag + " is occupied by Player " + player_id + " and has " + claimed_territory.armyCount + " armies";
        }
    }
    /// <summary>
    /// Given a player id and deck, draw a card from the deck and add it to the player's hand
    /// </summary>
    /// <param name="player_id"></param>
    /// <param name="deck"></param>
    private void HandleDrawCard(int player_id, GameObject deck){
        Card drawn = deck.GetComponent<DeckScript>().DrawCard();
        players[player_id - 1].cardsInHand.Add(drawn);
    }
    /// <summary>
    /// Verify that the given territory can be attacked from by the given player, then set
    /// the player's TerritoryAttackingFrom field.
    /// </summary>
    /// <param name="player_id"></param>
    /// <param name="territory"></param>
    private void HandleTerritoryToAttackFrom(int player_id, GameObject territory)
    {
        PlayerScript curr_player = players.Single(player => player.playerNumber == player_id);

        // Find the territory by name aka tag. If not found, do nothing
        TerritoryScript claimed_territory = territory.GetComponent<TerritoryScript>();

        // Update the territory's owner
        if (claimed_territory == null)
        {
            Debug.Log("Tag does not match a known territory");
            gameHUDScript.errorCardTMP.text = "Tag does not match a known territory";
            sfxPlayer.PlayErrorSound();
            return;
        }
        if (claimed_territory.occupiedBy == player_id)
        {
            if(claimed_territory.armyCount < 2){
                Debug.Log("Must attack from a territory with at least two armies.");
                return;
            }
            gameHUDScript.infoCardTMP.text = $"{curr_player} is attacking from " + claimed_territory.name;
            Debug.Log($"{curr_player} is attacking from " + claimed_territory.name);
            curr_player.TerritoryAttackingFrom = claimed_territory;
            return;
        }
        else
        {
            Debug.Log("Player " + player_id + " does not own " + claimed_territory.name);
            gameHUDScript.errorCardTMP.text = "Player " + player_id + " does not own " + claimed_territory.name;
        }
    }
    // Select a territory to attack on
    /// <summary>
    /// Verify that the given territory can be attacked on by the given player, then set
    /// the player's TerritoryAttackingOn field.
    /// </summary>
    /// <param name="player_id"></param>
    /// <param name="territory"></param>
    private void HandleTerritoryToAttackOn(int player_id, GameObject territory)
    {
        PlayerScript curr_player = players.Single(player => player.playerNumber == player_id);

        // Find the territory by name aka tag. If not found, do nothing
        TerritoryScript selected_territory = territory.GetComponent<TerritoryScript>();

        if (selected_territory != null)
        {
            if (selected_territory.occupiedBy != player_id)
            {
                Debug.Log("Player " + player_id + " is launching an attack on " + selected_territory.name);
                Debug.Log(curr_player.TerritoryAttackingFrom.name);
                gameHUDScript.infoCardTMP.text = "Player " + player_id + " is launching an attack on " + selected_territory.name;

                // Check if territory is adjacent
                if (AreAdjacent(selected_territory, curr_player.TerritoryAttackingFrom))
                {
                    curr_player.TerritoryAttackingOn = selected_territory;
                    return;
                }
                else{
                    infoCardText = "The selected territory: " + selected_territory.name +
                            " is not adjacent to the territory you are attacking from: " + curr_player.TerritoryAttackingFrom.name;
                    Debug.Log(infoCardText);
                    gameHUDScript.errorCardTMP.text = infoCardText;
                    sfxPlayer.PlayErrorSound();
                }
            }
            else
            {
                infoCardText = "Attack cannot be launched on " + selected_territory.name + ": Player owns the territory!";
                Debug.Log(infoCardText);
                gameHUDScript.errorCardTMP.text = infoCardText;
                sfxPlayer.PlayErrorSound();
            }
        }
        else
        {
            Debug.Log("Tag does not match a known territory");
            gameHUDScript.errorCardTMP.text = "Tag does not match a known territory";
            sfxPlayer.PlayErrorSound();
            return;
        }
    }
    /// <summary>
    /// Verify that the player can move armies from the given territory, then set
    /// the player's TerritoryMoveFrom field.
    /// </summary>
    /// <param name="player_id"></param>
    /// <param name="territory"></param>
    private void HandleTerritoryToMoveFrom(int player_id, GameObject territory)
    {
        PlayerScript curr_player = players.Single(player => player.playerNumber == player_id);

        // Find the territory by name aka tag. If not found, do nothing
        TerritoryScript selected_territory = territory.GetComponent<TerritoryScript>();

        if (selected_territory != null)
        {
            if (selected_territory.occupiedBy == player_id)
            {
                // Check if the territory has more than one army:
                if(selected_territory.armyCount <= 1){
                    // invalid
                    Debug.Log("Selected territory has insufficient armies.");
                    gameHUDScript.errorCardTMP.text = "Selected territory has insufficient armies.";
                    sfxPlayer.PlayErrorSound();
                    return;
                }
                
                selected_territory.FillAdjTerritoriesList();
                foreach(Transform adj_transform in selected_territory.adjacentCountries){
                    if(adj_transform.GetComponent<TerritoryScript>().occupiedBy == player_id){
                        // There is a valid adjacent territory owned by this player
                        curr_player.TerritoryMoveFrom = selected_territory;
                        return;
                    }
                }

                // Otherwise, there are no adjacent territories owned by this player
                infoCardText = "Selected territory has no neighbors owned by this player. Choose another territory";
                Debug.Log(infoCardText);
                gameHUDScript.errorCardTMP.text = infoCardText;
                sfxPlayer.PlayErrorSound();
                return;
            }
            else
            {
                infoCardText = selected_territory.name + " does not belong to this player";
                Debug.Log(infoCardText);
                gameHUDScript.errorCardTMP.text = infoCardText;
                sfxPlayer.PlayErrorSound();
            }
        }
        else
        {
            infoCardText = "Tag does not match a known territory";
            Debug.Log(infoCardText);
            gameHUDScript.errorCardTMP.text = infoCardText;
            sfxPlayer.PlayErrorSound();
            return;
        }
    }
    /// <summary>
    /// Verify that the player can move armies to the given territory, then set
    /// the player's TerritoryMoveTo field.
    /// </summary>
    /// <param name="player_id"></param>
    /// <param name="territory"></param>
    private void HandleTerritoryToMoveTo(int player_id, GameObject territory)
    {
        PlayerScript curr_player = players.Single(player => player.playerNumber == player_id);

        // Find the territory by name aka tag. If not found, do nothing
        TerritoryScript selected_territory = territory.GetComponent<TerritoryScript>();

        if (selected_territory != null)
        {
            if (selected_territory.occupiedBy == player_id)
            {
                // Check if territory is adjacent
                selected_territory.FillAdjTerritoriesList();
                selected_territory.FillAdjTerritoriesList();
                if(AreAdjacent(selected_territory, curr_player.TerritoryMoveFrom))
                {
                    curr_player.TerritoryMoveTo = selected_territory;
                    return;
                }
                else{
                    infoCardText = "The selected territory: " + selected_territory.name +
                            " is not adjacent to the territory you are moving from: " + curr_player.TerritoryAttackingFrom.name;
                    Debug.Log(infoCardText);
                    gameHUDScript.errorCardTMP.text = infoCardText;
                    sfxPlayer.PlayErrorSound();
                }
            }
            else
            {
                infoCardText = selected_territory.name + " does not belong to this player.";
                Debug.Log(infoCardText);
                gameHUDScript.errorCardTMP.text = infoCardText;
                sfxPlayer.PlayErrorSound();
            }
        }
        else
        {
            infoCardText = "Tag does not match a known territory";
            Debug.Log(infoCardText);
            gameHUDScript.errorCardTMP.text = infoCardText;
            sfxPlayer.PlayErrorSound();
            return;
        }
    }
    /// <summary>
    /// On the event that one player owns all the territories, this function 
    /// will end the game and show the ending screen.
    /// </summary>
    /// <param name="playerNumber"></param>
    public void HandlePlayerWonGame(int playerNumber){
        // Show ending screen
        gameHUDScript.ShowEndingPanel(playerNumber);
        gameOver = true;
    }
    /// <summary>
    /// When a player attempts to turn in a set of cards, this function verifies that the set 
    /// is valid, then updates the player's army count and card hand appropriately.
    /// </summary>
    /// <param name="selectedCards"></param>
    /// <param name="curr_player"></param>
    public void HandleCardTurnIn(List<Card> selectedCards, PlayerScript curr_player)
    {
        if (selectedCards == null)
        {
            return;
        }

        // Otherwise, must have selected 3 to turn in. Verify the match is valid. If not, return early.
        if(selectedCards.Count != 3){
            Debug.Log("Insufficient cards");
            gameHUDScript.errorCardTMP.text = "Insufficient cards";
            sfxPlayer.PlayErrorSound();
            return; // Should never be reached, but just in case.
        }

        // Check for a valid set
        if(selectedCards[0].troop_type != "WILD" && selectedCards[1].troop_type != "WILD"
            && selectedCards[2].troop_type != "WILD"){ 
            // All three match:
            if(selectedCards[0].troop_type == selectedCards[1].troop_type && 
                selectedCards[1].troop_type == selectedCards[2].troop_type){
                // valid set! allow cards to be turned in.
            }
            else if(selectedCards[0].troop_type != selectedCards[1].troop_type &&
                selectedCards[0].troop_type != selectedCards[2].troop_type &&
                selectedCards[1].troop_type != selectedCards[2].troop_type){
                // valid set! allow cards to be turned in.
            }
            else{
                Debug.Log("Set is invalid.");
                gameHUDScript.errorCardTMP.text = "Set is invalid.";
                sfxPlayer.PlayErrorSound();
                return;
            }

        }
        // If one card is a wild, then it must be a valid set. Allow the set to be turned in.
        // Grant the proper amount of armies:
        int armies_granted;
        if(sets_turned_in < ArmiesGrantedForCardSet.Length){
            armies_granted = ArmiesGrantedForCardSet[sets_turned_in];
        }
        else{
            armies_granted = 15 + 5*(sets_turned_in - 5);
        }
        sets_turned_in++;

        // Does the player own the territory of any of the cards? grant 2 extra armies
        foreach(Card card in selectedCards){
            if(card.territory_id == "WILD"){
                continue;
            }
            if(curr_player.territoriesOwned.
                Contains(GameObject.FindWithTag(card.territory_id).GetComponent<TerritoryScript>())){
                Debug.Log("Player " + curr_player.playerNumber + " owns " + card.territory_id);
                gameHUDScript.infoCardTMP.text = "Player " + curr_player.playerNumber + " owns " + card.territory_id;
                armies_granted += 2;
                break; // only grant up to 2, regardless of how many territories match
            }
        }
        curr_player.infCount += armies_granted;
        Debug.Log("Set is valid. Player has been granted " + armies_granted + " armies");
        gameHUDScript.infoCardTMP.text = "Set is valid. Player has been granted " + armies_granted + " armies";

        //removing the selected cards from the players hand:
        List<Card> updatedHand = new List<Card>();
        foreach(Card card in curr_player.cardsInHand){
            if (!selectedCards.Contains(card)){
                updatedHand.Add(card);
            }
        }
        // players[playerTurn - 1].cardsInHand = updatedHand;
        curr_player.cardsInHand = updatedHand; // For some reason, the above line doesn't update the player hand. We need this line.

        // Add selected cards to the discard pile:
        DeckScript ds = FindAnyObjectByType<DeckScript>().GetComponent<DeckScript>();
        if(ds != null){
            ds.discard.AddRange(selectedCards);
        }
    }
    /*****************************************************************************
    End of handle functions.
    ******************************************************************************/


    /// <summary>
    /// Utility function. Returns whether two territories are adjacent.
    /// </summary>
    /// <param name="terr1"></param>
    /// <param name="terr2"></param>
    /// <returns></returns>
    private bool AreAdjacent(TerritoryScript terr1, TerritoryScript terr2){
        return terr1.adjacentCountryEnums.Contains(terr2.territoryEnum);
    }



    /* This function would have been used if we had more time. It could be implemented in future updates when 
     * implementing the actual army pieces into the game
     */
    private void SpawnArmyPiece(ArmyTypes armyType, GameObject territory, int player_id)
    {
        /* If a player owns a territory, it will either have an inf object, a cav object or an artil object.
         * These objects will each have a number attached to them to say how many of its type it represents.
         * E.g if Player1 owns Peru and has 5 infantry on it, Peru will have 1 infantry Gameobject
         * on it, and that gameobjcet will have the number 5 attached to it. (to avoid having loads of gameobjects
         * on a single territory)
         * 
         * As a result of this, this method should handle spawning an army differently depending on what situation we're
         * spawning it in. Refer to the pinned messages in the "game-developments" channel to see these situations
        */

        Debug.Log("SPAWNING ARMY PIECE");

        //SITUATION 1 - Initial Army Placement:

        //setting the position for the army to be in when its spwaned
        Vector3 armyPos = territory.transform.position;
        armyPos.Set(armyPos.x, (float)0.4044418, armyPos.z);
        //creating the correct army piece on the territory that was clicked on for the player who clicked on it
        switch (armyType)
        {
            case ArmyTypes.Infantry:
                players[playerTurn - 1].CreateArmy(infantryPrefab, armyPos);
                break;
            case ArmyTypes.Cavalry:
                players[playerTurn - 1].CreateArmy(artillaryPrefab, armyPos);
                break;
            case ArmyTypes.Artillery:
                players[playerTurn - 1].CreateArmy(cavalryPrefab, armyPos);
                break;
        }
    }

}
