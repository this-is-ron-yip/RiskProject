using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public List<PlayerScript> players = new List<PlayerScript>();
    public List<Transform> territories = new List<Transform>();
    private DiceRollerScript diceRoller;
    public int[] diceResults;
    public Dictionary<Transform, List<Transform>> adjacencyList = new Dictionary<Transform, List<Transform>>();
    enum ArmyTypes { Infantry, Cavalry, Artillery };

    // Need 6 colors, for a maximum of 6 players
    public static Color[] colorArray = {Color.red, Color.yellow, Color.green,
                Color.blue, Color.magenta, Color.black};

    // Define static variables to avoid bugs:
    // public static String CLAIM_TERRITORIES_STAGE = "CLAIM_TERRITORIES";
    // public static String FINISH_PLACING_ARMIES_STAGE = "FINISH_PLACING_ARMIES";
    // public static String GAME_PLAY_STAGE = "PLAY"; // TODO: add more stages depending on implementation

    private void OnValidate()
    {
        FillTerritoriesList();
        FillAdjList();
    }
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
            players[playerTurn - 1].AllowRollToStart();

            // Wait for the player to take an action
            yield return StartCoroutine(WaitForDieRoll());

            // They didn't roll the die successfully
            if(diceResults[playerTurn - 1] == -1){
                continue;
            }
            
            // Remove permission
            players[playerTurn - 1].PreventRollToStart();
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
        int territories_left = 10; // TODO: change to 42, but for testing, use smaller number
        //Player picks unoccupied country to place 1 infantry, therefore occupying that country
        while(territories_left > 0){
            Debug.Log("Territories left: " + territories_left);
            players[playerTurn - 1].AllowClaimTerritoryAtStart();

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
            players[playerTurn - 1].PreventClaimTerritoryAtStart();

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
            players[playerTurn - 1].AllowPlaceArmyAtStart();
            
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
            players[playerTurn - 1].PreventPlaceArmyAtStart();

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

    private IEnumerator WaitForPlayerToDoMove(PlayerScript player)
    {
        yield return new WaitUntil(() => !player.isTurn);
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

            // Add one to the troops on this territory, since this function is only used at the start 
            // of the game. TODO: Create a similar, but more general function for typical gameplay
            claimed_territory.armyCount++;
            curr_player.infCount--;
            Debug.Log(territory.tag + " is occupied by Player " + player_id + " and has " + claimed_territory.armyCount + " armies");
            // spawn army (this step should always happen last!!!): 
            SpawnArmyPiece(ArmyTypes.Infantry, territory, player_id);
        }
    }

    private void HandleFinishPlacingArmiesAtStart(int player_id, GameObject territory){
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
            // Add one to the troops on this territory, since this function is only used at the start 
            // of the game. TODO: Create a similar, but more general function for typical gameplay
            claimed_territory.armyCount++;
            curr_player.infCount--;
            Debug.Log(claimed_territory.tag + " is occupied by Player " + player_id + " and has " + claimed_territory.armyCount + " armies");
        }
    }

    // TODO: complete this function!
    private IEnumerator EnterGamePlay(){
        Debug.Log("Entered game play!");

        int playerTurn = startingPlayer;

        // TODO: delete this loop later — used for testing only
        for(int i = 0; i < 15; i++){
            // update player and give them the turn
            PlayerScript player = players[playerTurn - 1];
            player.isTurn = true;

            // TODO: set permissions after figuring out game implementaiton. 
            // For now, let's just allow them to draw a card: 
            player.AllowDraw();

            // wait for player to finish turn
            yield return StartCoroutine(WaitForPlayerToDoMove(player));
            
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
            // TODO: DELETE LATER, give less armies for testing
            infCount = 5;
            newPlayerScript.GivePlayerArmies(infCount, 0, 0);

            // Set color for pieces:
            newPlayerScript.color = colorArray[newPlayerScript.playerNumber - 1];

            // Add listener for when player claims or attacks a territory
            newPlayerScript.OnPlayerClaimedTerritoryAtStart += HandleTerritoryClaimedAtStart;
            newPlayerScript.OnPlayerPlacesArmiesAtStart += HandleFinishPlacingArmiesAtStart;
            newPlayerScript.OnRollDiceAtStart += HandleDiceRollAtStart;
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
}
