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
    public Transform[] childObjects;
    public GameObject playerPrefab;
    public GameObject infantryPrefab;
    public GameObject cavalryPrefab;
    public GameObject artillaryPrefab;
    public int playerTurn = 1;
    public int playerCount = 3;
    public int startingPlayer = -1;
    private GameHUDScript gameHUDScript;

    public bool gameOver = false;
    public List<PlayerScript> players = new List<PlayerScript>();
    public List<Transform> territories = new List<Transform>();
    private DiceRollerScript diceRoller;
    public int[] diceResults;
    public Dictionary<Transform, List<Transform>> adjacencyList = new Dictionary<Transform, List<Transform>>();
    public event Action<int> OnPlayerConqueredAllTerritories;
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

    // Determine how many armies to grant based on how many sets have been turned in.
    // After six sets, grant 5 more armies: armies = 15 + (turned_in - 6)*5
    public static int[] ArmiesGrantedForCardSet = {4, 6, 8, 10, 12, 15}; 

    public int sets_turned_in = 0;

    private void OnValidate()
    {
        gameHUDScript = GameObject.FindWithTag("GameHUD").GetComponent<GameHUDScript>();
        FillTerritoriesList();
        FillAdjList();
    }

    [Obsolete]
    private void Start()
    {
        diceRoller = GetComponent<DiceRollerScript>(); //initialising the diceRoller
        CreatePlayers();
        UpdateHud();
        OnPlayerConqueredAllTerritories += HandlePlayerWonGame;

        StartCoroutine(AssignStartTerritories()); // Will in turn execute rest of the program.
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

        //TODO: Instantiate army object and place it on the territory OR simply don't spawn
            
        // Completed game set up!
        yield return StartCoroutine(EnterGamePlay());
    }

    private IEnumerator WaitForDieRoll(){
        // Debug.Log($"Player {playerTurn}, click the dice to roll.");
        gameHUDScript.eventCardTMP.text = $"Player {playerTurn}, click the dice to roll.";
        PlayerScript player = players[playerTurn - 1];
        player.clickExpected = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }

    private IEnumerator WaitForAttackFromTerritory()
    {
        gameHUDScript.eventCardTMP.text = $"Player {playerTurn}, click an owned territory to attack from.";
        PlayerScript player = players[playerTurn - 1];
        player.clickExpected = true;
        player.canSelectAttackFrom = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }

    private IEnumerator WaitForAttackOnTerritory()
    {
        gameHUDScript.eventCardTMP.text = $"Player {playerTurn}, click an enemy territory to attack on.";
        Debug.Log($"Player {playerTurn}, click an enemy territory to attack on.");
        PlayerScript player = players[playerTurn - 1];
        player.clickExpected = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }

    private IEnumerator InitialiseStartingInfantry()
    {
        //Debug.Log($"Player {playerTurn}, choose a territory to place 1 infantry on.");
        gameHUDScript.eventCardTMP.text = $"Player {playerTurn}, choose a territory to place 1 infantry on.";
        PlayerScript player = players[playerTurn - 1];
        player.clickExpected = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }

    private IEnumerator InitialiseStartOfTurnInfantry(int player_id){
        PlayerScript player = players[playerTurn - 1];
        gameHUDScript.eventCardTMP.text = $"Player {player_id}, place your infantries. You have {player.GetArmyCountTotal()} left";
        //Debug.Log($"Player {player_id}, where would you like to place one infantry?");
        player.clickExpected = true;
        yield return StartCoroutine(WaitForPlayerToDoMove(player));
    }

    private IEnumerator WaitForPlayerToDoMove(PlayerScript player)
    {
        yield return new WaitUntil(() => !player.clickExpected);
    }

    private IEnumerator WaitForChooseCardDisplayInactive()
    {
        yield return new WaitUntil(() => 
                !gameHUDScript.cardsAreOnDisplay);
    }

    // TODO: change isOnDisplay to be more specific
    private IEnumerator WaitForAttackInputPanelInactive()
    {
        yield return new WaitUntil(() => 
                !gameHUDScript.attackInputIsOnDisplay);
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
    }

    private IEnumerator LaunchAnAttack(int player_id)
    {
        Debug.Log("Phase two. Player: " + player_id + ", launch an attack.");
        PlayerScript player = players[player_id - 1];
        // Clear past values:
        player.TerritoryAttackingFrom = null;
        player.TerritoryAttackingOn = null;

        // Part one: pick an owned territory to attack from
        player.canSelectAttackFrom = true;
        while(player.TerritoryAttackingFrom == null)
        {
            playerTurn = player_id; // We need to reset the playerTurn, which is being updated when it shouldn't be
            // TODO: figure out why the behavior explained above is occuring.
            yield return StartCoroutine(WaitForAttackFromTerritory());
        }
        player.canSelectAttackFrom = false;

        player.canSelectAttackOn = true;
        while(player.TerritoryAttackingOn == null) { // Until valid input is received
            playerTurn = player_id; // Override in case playerTurn has changed unpredictably
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
            }
            else if(player.TerritoryAttackingOn.armyCount < defender_army_count){
                Debug.Log("Defender does not have sufficient armies for this input.");
            } 
            else if(attacker_army_count !=1 && attacker_army_count != 2 && attacker_army_count != 3)  {
                Debug.Log("Attacker must choose to fight with one, two, or three armies.");
            }
            else if(player.TerritoryAttackingFrom.armyCount <= attacker_army_count){
                Debug.Log("Attacker has selected too many armies. Must leave one army " + 
                    " to occupy its starting territory.");
            }
            else{
                valid_input = true;
            }
        }

        // Part four: evaluate the outcome of the attack 
        EvaluateAttack(player_id, attacker_army_count, defender_army_count);

        /* From the game rules:
        If winning them gives you 6 or more cards, you must immediately trade
        in enough sets to reduce your hand to 4 or fewer cards, but once your
        hand is reduced to 4,3, or 2 cards, you must stop trading.
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

    private void HandleDiceRollAtStart(int player_id, GameObject die){
        DiceRollerScript die_rolled = die.GetComponent<DiceRollerScript>();
        int result = die_rolled.RollDice();
        diceResults[player_id - 1] = result;
        //Debug.Log("Player " + player_id + " rolled a " + result);
        gameHUDScript.infoCardTMP.text = "Player " + player_id + " rolled a " + result;
        // TODO: add animation here.
    }
        
    // Update territory members
    private void HandleTerritoryClaimedAtStart(int player_id, GameObject territory){
        PlayerScript curr_player = players.Single(player => player.playerNumber == player_id);

        // Find the territory by name aka tag. If not found, do nothing
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
            claimed_territory.armyCount++;
            curr_player.infCount--;
            Debug.Log(territory.tag + " is occupied by Player " + player_id + " and has " + claimed_territory.armyCount + " armies." +
               " Player " + player_id + " now has " + curr_player.territoryCountsPerContinent[claimed_territory.continent] + " on " + 
               " territories on the continent of " + claimed_territory.continent + ". Player " + player_id + " has " + 
                curr_player.infCount + " infantry remaining.");
            // spawn army (this step should always happen last!!!): 
            // SpawnArmyPiece(ArmyTypes.Infantry, territory, player_id);
        }
    }

    // Select a territory to attack from
    private void HandleTerritoryToAttackFrom(int player_id, GameObject territory)
    {
        PlayerScript curr_player = players.Single(player => player.playerNumber == player_id);

        // Find the territory by name aka tag. If not found, do nothing
        TerritoryScript claimed_territory = territory.GetComponent<TerritoryScript>();

        // Update the territory's owner
        if (claimed_territory == null)
        {
            Debug.Log("Tag does not match a known territory");
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
        }
    }

    // Select a territory to attack on
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
                
                // Check if territory is adjacent
                if(AreAdjacent(selected_territory, curr_player.TerritoryAttackingFrom))
                {
                    curr_player.TerritoryAttackingOn = selected_territory;
                    return;
                }
                else{
                    Debug.Log("The selected territory: " + selected_territory.name + 
                            " is not adjacent to the territory you are attacking from: " + curr_player.TerritoryAttackingFrom.name);
                }
            }
            else
            {
                Debug.Log("Attack cannot be launched on " + selected_territory.name + ": Player owns the territory!");
            }
        }
        else
        {
            Debug.Log("Tag does not match a known territory");
            return;
        }
    }

   public void EvaluateAttack(int attacker_id, int attacker_dice, int defender_dice)
    {
        PlayerScript player = players[attacker_id - 1];

        TerritoryScript PlayerTerritory = player.TerritoryAttackingFrom;
        TerritoryScript EnemyTerritory = player.TerritoryAttackingOn;
        PlayerScript defender = players[EnemyTerritory.occupiedBy - 1];

        // Generate the die rolls.
        List<int> attacker_rolls = new List<int>();
        List<int> defender_rolls = new List<int>();
        for(int i = 0; i < attacker_dice; i++){
            attacker_rolls.Add(UnityEngine.Random.Range(1, 7));
            Debug.Log("Attacker Roll " + i + ": " + attacker_rolls[i]);
            
        }
        for(int i = 0; i < defender_dice; i++){
            defender_rolls.Add(UnityEngine.Random.Range(1, 7));
            Debug.Log("Defender Roll " + i + ": " + defender_rolls[i]);
        }

        // Sort from highest to lowest. Sorts in ascending order by default, so reverse
        attacker_rolls.Sort();
        attacker_rolls.Reverse();
        defender_rolls.Sort();
        defender_rolls.Reverse();

        // Determine winner
        int one_v_ones = Math.Min(attacker_dice, defender_dice);
        int remaining_attackers = attacker_dice;
        for(int i = 0; i < one_v_ones; i++){
            Debug.Log("Battling... Attacker: " +attacker_rolls[i] +
                    " vs Defender: " + defender_rolls[i]);
            if(attacker_rolls[i] > defender_rolls[i]){
                // Defender loses one army on the territory being attacked.
                Debug.Log("Attacker wins this match up!");
                EnemyTerritory.armyCount--;
            }
            else{ // Defender wins on a tie
                // Attacker loses one of the armies they sent to attack
                Debug.Log("Defender wins this match up!");
                PlayerTerritory.armyCount--;
                remaining_attackers--;
            }
        }

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
        }

        /* TODO: prompt attacker to fortify position. Game rules state: 
            As soon as you defeat the last opposing army on
            a territory, you capture that territory and must occupy it immediately. To
            do so, move in at least as many armies as the number of dice you rolled in
            your last battle. Remember you must always leave at least
            one army behind on the territory you attacked from. During the game,
            every territory must always be occupied by at least one army
        */

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
        }

        // TODO: delete later. for testing the final game screen only
        // OnPlayerConqueredAllTerritories?.Invoke(player.playerNumber);

        return;
    }
    private void HandlePlacingAnArmy(int player_id, GameObject territory){
        PlayerScript curr_player = players.Single(player => player.playerNumber == player_id);

        // Find the territory by name aka tag. If not found, do nothing
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
            // foreach (GameObject army in curr_player.armies)
            // {
            //     if (army.GetComponent<ArmyScript>().currentTerritoryPos = territory.transform)
            //     {
            //         army.GetComponent<ArmyScript>().armyCount = claimed_territory.armyCount;
            //     }
            // }
            curr_player.infCount--;
            Debug.Log(claimed_territory.tag + " is occupied by Player " + player_id + " and has " + claimed_territory.armyCount + " armies");
        }
    }

    private void HandleDrawCard(int player_id, GameObject deck){
        Card drawn = deck.GetComponent<DeckScript>().DrawCard();
        players[player_id - 1].cardsInHand.Add(drawn);
    }

    [Obsolete]
    private IEnumerator EnterGamePlay(){
        gameHUDScript.infoCardTMP.text = "Entered game play!";
        int playerTurn = startingPlayer;

        while(!gameOver){
            // update player and give them the turn
            PlayerScript player = players[playerTurn - 1];
            // Prevent all permissions at start of each loop
            player.ResetAllPermissions();
            // Update gamehud's player
            FindAnyObjectByType<GameHUDScript>().currentPlayer = players[playerTurn-1];

            // Check if this player has been eliminated before proceeding: 
            if(player.eliminated){
                Debug.Log("Player " + playerTurn + " is out of the game. Skipping their turn.");
                // update the player
                if(playerTurn == playerCount){
                    // cycle back to start of player list
                    playerTurn = 1;
                }
                else{
                    // otherwise, simply move to the next player
                    playerTurn++;
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
            if(player.cardsInHand.Count >= 3)
            {
                // Allow player to turn in cards
                Debug.Log("Player: " +  GameObject.FindAnyObjectByType<GameHUDScript>().currentPlayer.playerNumber +
                         " Select cards to turn in.");
                gameHUDScript.ShowChooseCardPanel();
                // Once display is inactive, assume we are done with this phase.
                yield return StartCoroutine(WaitForChooseCardDisplayInactive());
            }
            
            // Require players to turn in until they have less than 5 cards.
            while (player.cardsInHand.Count >= 5)
            {
                // Allow player to turn in cards
                Debug.Log("Player: " +  GameObject.FindAnyObjectByType<GameHUDScript>().currentPlayer.playerNumber +
                         " Still has more than four cards. Select cards to turn in.");
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

                // Revoke permission
                player.canPlaceArmyInGame = false;
            }

            // Step three: let the player attack
            // TODO: let them attack multiple times, until they have no territories that are eligibe to attack from
            // We can probably just lmake this a button instead of computing if there are elligible territories every time
            yield return LaunchAnAttack(playerTurn);

            // Step four: if the player has claimed at least one territory during their turn
            // Prompt and allow/require them to draw a card from the deck
            bool player_must_draw = player.wonTerritory;
            while(player_must_draw){
                Debug.Log("Player " + playerTurn + " won a territory this round. Draw a card from the deck");
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
                }
            }

            // TODO: Step five: Fortify position
            /*
            To fortify your position, move as many armies as you’d like from one (and
            only one) of your territories into one (and only one) of your adjacent
            territories. 
            */
            

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
            // TODO: DELETE THE LINE BELOW LATER, give less armies for testing
            infCount = 3;
            newPlayerScript.GivePlayerArmies(infCount, 0, 0);

            // Set color for pieces:
            newPlayerScript.color = colorArray[newPlayerScript.playerNumber - 1];

            // Add listener for when player claims or attacks a territory
            newPlayerScript.OnPlayerClaimedTerritoryAtStart += HandleTerritoryClaimedAtStart;
            newPlayerScript.OnPlayerPlacesAnArmyAtStart += HandlePlacingAnArmy;
            newPlayerScript.OnPlayerPlacesAnArmyInGame += HandlePlacingAnArmy;
            newPlayerScript.OnPlayerSelectAttackFrom += HandleTerritoryToAttackFrom;
            newPlayerScript.OnPlayerSelectAttackOn += HandleTerritoryToAttackOn;
            newPlayerScript.OnPlayerSelectAttackOn += HandleTerritoryToAttackOn;
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

    // Returns whether or not the territories are adjacent. Simply check adjacency list
    private bool AreAdjacent(TerritoryScript terr1, TerritoryScript terr2){
        return terr1.adjacentCountryEnums.Contains(terr2.territoryEnum);
    }

    // TODO: Delete this function? This function is only responsible for creating the army piece and visually placing it
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
    }

    public void HandlePlayerWonGame(int playerNumber){
        // Show ending screen
        gameHUDScript.ShowEndingPanel(playerNumber);
        gameOver = true;
    }
    public void HandleCardTurnIn(List<Card> selectedCards, PlayerScript curr_player)
    {
        if (selectedCards == null)
        {
            Debug.Log("Player chose to not turn in any cards");
            return;
        }

        // Otherwise, must have selected 3 to turn in. Verify the match is valid. If not, return early.
        if(selectedCards.Count != 3){
            Debug.Log("Insufficient cards");
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
                armies_granted += 2;
                break; // only grant up to 2, regardless of how many territories match
            }
        }
        curr_player.infCount += armies_granted;
        Debug.Log("Set is valid. Player has been granted " + armies_granted + " armies");

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
        
}
