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
    public int playerTurn;
    public int playerCount = 3;
    public int startingPlayer = -1;
    public List<PlayerScript> players = new List<PlayerScript>();
    public List<Transform> territories = new List<Transform>();
    private DiceRollerScript diceRoller;
    public int[] diceResults;
    public Dictionary<Transform, List<Transform>> adjacencyList = new Dictionary<Transform, List<Transform>>();

    // Define static variables to avoid bugs:
    public static String CLAIM_TERRITORIES_STAGE = "CLAIM_TERRITORIES";
    public static String FINISH_PLACING_ARMIES_STAGE = "FINISH_PLACING_ARMIES";
    public static String GAME_PLAY_STAGE = "PLAY";

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
        diceRoller.OnDiceRolled += HandleDiceRollResult; //assigning the OnDiceRolled event to the HandleDiceResult method 
        diceResults = new int[playerCount];
        
        yield return StartCoroutine(RollDiceForAllPlayers());

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

        // Enetered new game stage
        PlayerScript.gameStage = "CLAIM_TERRITORIES";

        int territories_left = 10; // TODO: change to 42, but for testing, use smaller number
        //Player picks unoccupied country to place 1 infantry, therefore occupying that country
        while(territories_left > 0){
            Debug.Log("Territories left: " + territories_left);
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

        /* Impelemnet last stage of set up:

        "After all 42 territories are claimed, each player in turn places one
        additional army onto any territory he or she already occupies. Continue
        in this way until everyone has run out of armies. There is no limit to the
        number of armies you may place onto a single territory"
        
        */
        PlayerScript.gameStage = FINISH_PLACING_ARMIES_STAGE;
        // Change game stage
        while(players[playerTurn - 1].GetArmyCountTotal() != 0){
            int player_starting_armies = players[playerTurn - 1].GetArmyCountTotal();

            yield return StartCoroutine(InitialiseStartingInfantry());

            // check that the placing army was successful by checking the player's army count
            // if it wasn't, don't update anything and try again
            int player_ending_armies = players[playerTurn - 1].GetArmyCountTotal();
            if(player_ending_armies == player_starting_armies){
                continue;
            }
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


        PlayerScript.gameStage = "PLAY"; // done with set up!

        //TODO
            // Instanciate army object and place it on the territory
            // Update hud
        yield return StartCoroutine(EnterGamePlay());
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

    private IEnumerator RollDiceForAllPlayers()
    {
        for (playerTurn = 1; playerTurn < playerCount+1; playerTurn++)
        {
            diceRoller.AllowRoll(playerTurn);
            yield return StartCoroutine(WaitForDiceRoll());

            // After the roll is complete, store the result
            // Assuming HandleDiceRollResult stores the result in diceResults[currentPlayerIndex]
        }
        // Now, all players have rolled the dice, and you can print the results
        Debug.Log($"Dice Results: {string.Join(", ", diceResults)}");
    }

    private IEnumerator WaitForDiceRoll()
    {
        yield return new WaitUntil(() => diceRoller.isRollComplete);
        diceRoller.isRollComplete = false; // Reset for the next roll
    }

    private void HandleDiceRollResult(int result)
    {
        // Store the result for the current player
        diceResults[playerTurn-1] = result;

        Debug.Log($"Player {playerTurn} rolled: {result}");
    }

    // Update territory members
    private void HandleTerritoryClaimedAtStart(int player_id, string territory_id){
        PlayerScript curr_player = players.Single(player => player.playerNumber == player_id);
        if(PlayerScript.gameStage != "CLAIM_TERRITORIES"){
            // ERROR: this function should only be called in the claime territories stage
            return;
        }

        // Find the territory by territory_id aka tag. If not found, do nothing
        TerritoryScript claimed_territory = GameObject.FindWithTag(territory_id).GetComponent<TerritoryScript>();
        
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
            Debug.Log(territory_id + " is occupied by Player " + player_id + " and has " + claimed_territory.armyCount + " armies");
        }
    }

    private void HandleFinishPlacingArmiesAtStart(int player_id, GameObject territory){
        PlayerScript curr_player = players.Single(player => player.playerNumber == player_id);
        
        if(PlayerScript.gameStage != "FINISH_PLACING_ARMIES"){
            // ERROR: this function should only be called in the finish placing armies stage
            return;
        }

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

            // Add listener for when player claims or attacks a territory
            newPlayerScript.OnPlayerClaimedTerritoryAtStart += HandleTerritoryClaimedAtStart;
            newPlayerScript.OnPlayerPlacesArmiesAtStart += HandleFinishPlacingArmiesAtStart;
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

}
