using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MapScript : MonoBehaviour
{
    public Transform[] childObjects;
    public GameObject playerPrefab;
    public GameObject infantryPrefab;
    public GameObject cavalryPrefab;
    public GameObject artillaryPrefab;
    public int playerTurn = 1;
    public int playerCount = 3;
    public int startingPlayer = -1;

    public bool gameOver = false;
    public List<PlayerScript> players = new List<PlayerScript>();
    public List<Transform> territories = new List<Transform>();
    private DiceRollerScript diceRoller;
    public int[] diceResults;
    public Dictionary<Transform, List<Transform>> adjacencyList = new Dictionary<Transform, List<Transform>>();
    enum ArmyTypes { Infantry, Cavalry, Artillery };

    // Need 6 colors, for a maximum of 6 players
    public static Color[] colorArray = {Color.red, Color.yellow, Color.green,
                Color.blue, Color.magenta, Color.black};
    
    // Map for how many terrritories correspond to each continent
    public static Dictionary<TerritoryScript.Continents, int> ContinentTerritoryCounts =  
        new Dictionary<TerritoryScript.Continents, int> (){
            {TerritoryScript.Continents.NorthAmerica, 9}, {TerritoryScript.Continents.SouthAmerica, 4},
            {TerritoryScript.Continents.Europe, 7}, {TerritoryScript.Continents.Asia, 12}, 
            {TerritoryScript.Continents.Africa, 6}, {TerritoryScript.Continents.Australia, 4}
    };
    
    // Map for how many armies to grant for controlling a continent
    public static Dictionary<TerritoryScript.Continents, int> ArmiesGrantedForContinent =  
        new Dictionary<TerritoryScript.Continents, int> (){
            {TerritoryScript.Continents.NorthAmerica, 5}, {TerritoryScript.Continents.SouthAmerica, 2},
            {TerritoryScript.Continents.Europe, 5}, {TerritoryScript.Continents.Asia, 7}, 
            {TerritoryScript.Continents.Africa, 3}, {TerritoryScript.Continents.Australia, 2}
    };

    private void OnValidate()
    {
        FillTerritoriesList();
        FillAdjList();
    }

    [Obsolete]
    private void Start()
    {
        diceRoller = GetComponent<DiceRollerScript>(); //initialising the diceRoller
        CreatePlayers();
        UpdateHud();

        StartCoroutine(AssignStartTerritories());
    }

    private void Update()
    {

    }

    public void OnDrawGizmos()
    {
        //using this function because it executes for every frame inside the unity editor.
        //this function will draw the lines between the countries (which will indicate the routes the piece will take) using the adjacency list
        Gizmos.color = Color.green;

        foreach (var pair in adjacencyList)
        {
            Vector3 startPos = pair.Key.position;

            foreach (var adjTerr in pair.Value)
            {
                Vector3 endPos = adjTerr.position;
                Gizmos.DrawLine(startPos, endPos);
            }
        }
    }

    //idk if this works, will check later
    public List<Transform> GetAdjTerritories(Transform territory)
    {
        List<Transform> listOfAdjTerrs = new List<Transform>();
        foreach (var pair in adjacencyList)
        {
            if (pair.Key.gameObject.tag == territory.gameObject.tag)
            {
                listOfAdjTerrs = adjacencyList[pair.Key];
            }
        }
        listOfAdjTerrs.Add(null);
        return listOfAdjTerrs;
    }

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
            }
            else{
                // otherwise, simply move to the next player
                playerTurn++;
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
        startingPlayer = playerTurn; // for future reference

        //for testing
        GameObject.FindAnyObjectByType<GameHUDScript>().currentPlayer = players[playerTurn-1];

        // Step two: Allow players to claim territories to start
        int territories_left = 10; // TODO: change to 42, but for testing, use smaller number
        //Player picks unoccupied country to place 1 infantry, therefore occupying that country
        while(territories_left > 0){
            Debug.Log("Territories left: " + territories_left);
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
            }
            else{
                // otherwise, simply move to the next player
                playerTurn++;
            }
        }

        /* Step three: players place remaining pieces on their claimed territories

        "After all 42 territories are claimed, each player in turn places one
        additional army onto any territory he or she already occupies. Continue
        in this way until everyone has run out of armies. There is no limit to the
        number of armies you may place onto a single territory"
        
        */
    
        // Entered next phase of the game
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
            }
            else{
                // otherwise, simply move to the next player
                playerTurn++;
            }
        }

        //TODO
            // Instanciate army object and place it on the territory
            // Update hud
        // Completed game set up!
        yield return StartCoroutine(EnterGamePlay());
    }

    private IEnumerator WaitForDieRoll(){
        Debug.Log($"Player {playerTurn}, click the dice to roll.");
        PlayerScript player = players[playerTurn - 1];
        player.isTurn = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }

    private IEnumerator InitialiseStartingInfantry()
    {
        Debug.Log($"Player {playerTurn}, choose a territory to place 1 infantry on.");
        PlayerScript player = players[playerTurn - 1];
        player.isTurn = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }

    private IEnumerator InitialiseStartOfTurnInfantry(int player_id){
        Debug.Log($"Player {player_id}, where would you like to place one infantry?");
        PlayerScript player = players[player_id - 1];
        player.isTurn = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }

    private IEnumerator WaitForPlayerToDoMove(PlayerScript player)
    {
        yield return new WaitUntil(() => !player.isTurn);
    }


    // Responsible for calculating and granting armies based on territories,
    // At the start of each turn.
    // This function is called by a coroutine, so it must be a coroutine
    private void GrantArmiesFromTerritories(int player_id){
        PlayerScript player = players[player_id - 1];

        // Count the number of territories you occupy, divide by three and round down. 
        player.infCount += Math.Max(player.territoriesOwned.Count / 3, 3);

        Debug.Log("Player " + player_id + " occupies " + player.territoriesOwned.Count + 
        " territories and has been granted " +
                player.GetArmyCountTotal() + " armies");
    }

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
        Debug.Log("Player " + player_id + " now has " + player.GetArmyCountTotal() + " armies");
    }

    private void HandleDiceRollAtStart(int player_id, GameObject die){
        DiceRollerScript die_rolled = die.GetComponent<DiceRollerScript>();
        int result = die_rolled.RollDice();
        diceResults[player_id - 1] = result;
        Debug.Log("Player " + player_id + " rolled a " + result);
        // TODO: add animation here.
    }

    // Update territory members
    private void HandleTerritoryClaimedAtStart(int player_id, GameObject territory){
        PlayerScript curr_player = players.Single(player => player.playerNumber == player_id);

        // Find the territory by territory_id aka tag. If not found, do nothing
        TerritoryScript claimed_territory = territory.GetComponent<TerritoryScript>();
        
        // Update the territory's owner
        if(claimed_territory == null){
            Debug.Log("Tag does not match a known territory");
            return;
        }
        if(claimed_territory.occupiedBy != -1){
            Debug.Log("Territory already claimed. Occupied by Player " + claimed_territory.occupiedBy + ". Returning from handler");
            // someone else is occupying, do nothing
            return;
        }
        else{
            // add territory to player list:
            claimed_territory.occupiedBy = player_id;
            curr_player.territoriesOwned.Add(claimed_territory);
            curr_player.territoryCountsPerContinent[claimed_territory.continent] += 1;

            // Add one to the troops on this territory, since this function is only used at the start 
            // of the game. TODO: Create a similar, but more general function for typical gameplay
            claimed_territory.armyCount++;
            curr_player.infCount--;
            Debug.Log(territory.tag + " is occupied by Player " + player_id + " and has " + claimed_territory.armyCount + " armies." +
               " Player " + player_id + " now has " + curr_player.territoryCountsPerContinent[claimed_territory.continent] + " on " + 
               " territories on the continent of " + claimed_territory.continent);
            // spawn army (this step should always happen last!!!): 
            SpawnArmyPiece(ArmyTypes.Infantry, territory, player_id);
        }
    }

    private void HandlePlacingAnArmy(int player_id, GameObject territory){
        PlayerScript curr_player = players.Single(player => player.playerNumber == player_id);

        // Find the territory by territory_id aka tag. If not found, do nothing
        TerritoryScript claimed_territory = territory.GetComponent<TerritoryScript>();
        
        // Update the territory's owner
        if(claimed_territory == null){
            Debug.Log("Tag does not match a known territory");
            return;
        }
        if(claimed_territory.occupiedBy != player_id){
            Debug.Log(claimed_territory.tag + " is not claimed by this player.");
            // only allowed to add to already claimed territories
            return;
        }
        else{
            //update the territory's armyCount
            claimed_territory.armyCount++;
            //update the armyCount of the Army that is on the territory so it can display a new number
            foreach (GameObject army in curr_player.armies)
            {
                if (army.GetComponent<ArmyScript>().currentTerritoryPos = territory.transform)
                {
                    army.GetComponent<ArmyScript>().armyCount = claimed_territory.armyCount;
                }
            }
            curr_player.infCount--;
            Debug.Log(claimed_territory.tag + " is occupied by Player " + player_id + " and has " + claimed_territory.armyCount + " armies");
        }
    }

    private void HandleDrawCard(int player_id, GameObject deck){
        Card drawn = deck.GetComponent<DeckScript>().DrawCard();
        players[player_id - 1].cardsInHand.Add(drawn);
    }

    // TODO: complete this function!
    [Obsolete]
    private IEnumerator EnterGamePlay(){
        Debug.Log("Entered game play!");
        int playerTurn = startingPlayer;
        int testNumberOfTurns = 10;

        while(!gameOver){
            // update player and give them the turn
            PlayerScript player = players[playerTurn - 1];
            // Prevent all permissions at start of each loop
            player.ResetAllPermissions();

            // Step one: getting and placing armies
            /// calculate the number of armies this player should receive based on territories
            GrantArmiesFromTerritories(playerTurn);

            // Require player to place the new armies
            while(player.GetArmyCountTotal() != 0){
                player.canPlaceArmyInGame = true;
                yield return StartCoroutine(InitialiseStartOfTurnInfantry(playerTurn));
                Debug.Log("Player " + playerTurn + " has " + player.GetArmyCountTotal() + 
                    " armies left");

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

                // Revoke permission
                player.canPlaceArmyInGame = false;
            }

            // Step two: allow player to turn in sets of cards. give additional armies accordingly
            // Allow player to turn in cards
            // TODO: this requires figuring out how the HUD will work, how to prompt user to 
            // Submit sets of cards. Fill in this step once that is complete
            if (players[playerTurn-1].cardsInHand.Count >= 3)
            {
                GameObject.FindWithTag("GameHUD").GetComponent<GameHUDScript>().ShowChooseCardPanel();
                // TODO: add code to wait for the player to choose a card
            }
            else
            {
                Debug.Log("You don't have enough cards to turn in (3 minimum required)...");
            }

            // Require player to place armies earned from card sets

            // Step three: as long as they keep winning, prompt and allow the player to attack
            // TODO: maybe make this a member variable
            bool playerMustDraw = false; // For whether they can draw a RISK card at the end

            // Step four: if the player has claimed at least one territory during their turn
            // Prompt and allow/require them to draw a card from the deck
            // TODO: what if the deck is empty? 


            /*
            playerMustDraw = true; // TODO: delete later, for testing only: 
            while(playerMustDraw){
                Debug.Log("Player " + playerTurn + " won a territory this round. Draw a card from the deck");
                int cardsBeforeDraw = player.cardsInHand.Count;
                player.canDraw = true;
                player.isTurn = true;
                yield return StartCoroutine(WaitForPlayerToDoMove(player));
                int cardsAfterDraw = player.cardsInHand.Count;
                if(cardsBeforeDraw != cardsAfterDraw){
                    // Task accomplished
                    player.canDraw = false;
                    playerMustDraw = false;
                }
                else{
                    Debug.Log("Must draw a card. Try again.");
                }
            }
            

            Debug.Log("End of Player " + playerTurn + "'s turn");
            
            // update the player
            if(playerTurn == playerCount){
                // cycle back to start of player list
                playerTurn = 1;
            }
            else{
                // otherwise, simply move to the next player
                playerTurn++;
            }

            // TODO: delete later. For testing only
            testNumberOfTurns--;
            if(testNumberOfTurns == 0){
                gameOver = true;
            }
            */
        }
    }

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
            // TODO: DELETE LATER, give less armies for testing
            infCount = 5;
            newPlayerScript.GivePlayerArmies(infCount, 0, 0);

            // Set color for pieces:
            newPlayerScript.color = colorArray[newPlayerScript.playerNumber - 1];

            // Add listener for when player claims or attacks a territory
            newPlayerScript.OnPlayerClaimedTerritoryAtStart += HandleTerritoryClaimedAtStart;
            newPlayerScript.OnPlayerPlacesAnArmyAtStart += HandlePlacingAnArmy;
            newPlayerScript.OnPlayerPlacesAnArmyInGame += HandlePlacingAnArmy;
            newPlayerScript.OnRollDiceAtStart += HandleDiceRollAtStart;
            newPlayerScript.OnPlayerDrawsCard += HandleDrawCard;
            players.Add(newPlayerScript);
        }

    }

    private void UpdateHud()
    {
        //Update all the huds
    }

    private void FillAdjList()
    {
        adjacencyList.Clear();
        foreach (Transform territory in territories)
        {
            adjacencyList.Add(territory, GameObject.FindWithTag(territory.gameObject.tag).GetComponent<TerritoryScript>().adjacentCountries);
        }
    }

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


    // This function is only responsible for creating the army piece and visually placing it
    // in the proper position. Any other functional logic (i.e. number of armies a player has
    // left, or who owns the territory) is handled outside of this function.
    // As such, this function should always be called last, since it may rely on data members
    // that should be modified before it is called. 
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
        
        //TODO for SITUATION 1: check any other variables that need to be updated when spawning the Army object
            //(e.g occupiedBy field in the Territory script etc..)

        //SITUATION 2 - Getting and Placing New Armies at the Beginning of Each Turn:

        //SITUATION 3 - During Gameplay:

        // TODO: place army on the given position
        // TODO: figure out who owns the army objects. Should we store a list
        // of army game objects in the player, which would include their position? 
        // TODO: figure out how to represent armies on a territory (maybe just 1 piece?)
        // And maybe clicking on the piece tells us how many armies it represents.
    }

    internal void HandleCardTurnIn(List<Card> selectedCards)
    {
        //removing the selected cards from the players hand:
        List<Card> playerCards = players[playerTurn - 1].GetComponent<PlayerScript>().cardsInHand;
        if (selectedCards != null)
        {
            for (int i = playerCards.Count - 1; i >= 0; i--)
            {
                if (selectedCards.Contains(playerCards[i]))
                {
                    playerCards.RemoveAt(i);
                }
            }
        }
        

        //Can remove all of the following code. It was just to show that the card selection works correctly

        //if selected cards is a null object, it means that the player decided not to turn in any cards
        if (selectedCards == null)
        {
            Debug.Log("Player chose to not turn in any cards");
        }
        else
        {
            string debugString = "Selected Cards: ";
            foreach (Card card in selectedCards)
            {
                debugString += "(" + $"{card.territory_id} : {card.troop_type}" + ") ";
            }
            Debug.Log(debugString);
        }

        // showing the cards that are left in the players hand
        string debugString2 = "Cards remaining in player hand : ";
        foreach (Card card in playerCards)
        {
            debugString2 += "(" + $"{card.territory_id} : {card.troop_type}" + ") ";
        }
        Debug.Log(debugString2);
    }
        
}
