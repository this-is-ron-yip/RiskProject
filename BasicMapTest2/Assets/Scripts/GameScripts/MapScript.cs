using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapScript : MonoBehaviour
{
    public Transform[] childObjects;
    public GameObject playerPrefab;
    public int playerTurn;
    public int playerCount = 3;
    public List<PlayerScript> players = new List<PlayerScript>();
    public List<Transform> territories = new List<Transform>();
    private DiceRollerScript diceRoller;
    public int[] diceResults;
    public Dictionary<Transform, List<Transform>> adjacencyList = new Dictionary<Transform, List<Transform>>();

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
        //write code here to wait 1 second for all the other classes to finish executing their startup code
        AssignStartTerritories();
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

    private void AssignStartTerritories()
    {
        diceRoller.OnDiceRolled += HandleDiceRollResult;
        diceResults = new int[playerCount];

        StartCoroutine(RollDiceForAllPlayers());

        // The following still needs to be coded:

        //Whoever lands highest gets to start choosing first
        //START LOOP:
        //Player picks unoccupied country to place 1 infantry, therefore occupying that country
        //Instanciate army object and place it on the territory
        //Update hud
        //update "occupiedBy" field in territory class of respective territory
        //update "ownedTerritories field in player class of respective player
        //IF all territories are occupied, endLoop. ELSE:
        //next player's turn to place a piece. restart loop(maybe change the playerTurn bool inside this class and the player class?)
        //END LOOP
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
            newPlayerScript.GivePlayerArmies(infCount, 0, 0);
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
