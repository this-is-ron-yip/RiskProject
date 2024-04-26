using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int infCount, cavCount, artilCount;
    public int playerNumber;
    public bool isTurn = false;

    public Color color = new Color(0, 0, 0);
    public List<TerritoryScript> territoriesOwned = new List<TerritoryScript>();
    // public List<GameObject> armies = new List<GameObject>();
    public List<Card> cardsInHand = new List<Card>();
    public List<Card> cardsPlayed = new List<Card>();
    public Dictionary<TerritoryScript.Continents, int> territoryCountsPerContinent =  
        new Dictionary<TerritoryScript.Continents, int> (){
            {TerritoryScript.Continents.NorthAmerica, 0}, {TerritoryScript.Continents.SouthAmerica, 0},
            {TerritoryScript.Continents.Europe, 0}, {TerritoryScript.Continents.Asia, 0}, 
            {TerritoryScript.Continents.Africa, 0}, {TerritoryScript.Continents.Australia, 0}
    };

    // Create a set of booleans to dictate legal and illegal actions for this player.
    // MapScript will modify these permissions during game play
    public bool canClaimTerritoryAtStart = false;
    public bool canPlaceArmyAtStart = false;
    public bool canDraw = false;
    public bool canRollToStart = false;
    public bool canTurnInCards = false;
    public bool canSelectAttackFrom = false;
    public bool canSelectAttackOn = false;    public bool canPlaceArmyInGame = false;

    // TODO: add more permissoins for different actions

    // Define an event that other scripts can subscribe to, to get the player id
    // The int is the player id, the object is the object they clicked on
    public event Action<int, GameObject> OnPlayerClaimedTerritoryAtStart;
    public event Action<int, GameObject> OnPlayerPlacesAnArmyAtStart;
    public event Action<int, GameObject> OnPlayerPlacesAnArmyInGame;
    public event Action<int, GameObject> OnPlayerSelectAttackFrom;
    public event Action<int, GameObject> OnPlayerSelectAttackOn;
    public event Action<int, GameObject> OnRollDiceAtStart;
    public event Action<int, GameObject> OnPlayerDrawsCard;
    enum ArmyTypes { Infantry, Cavalry, Artillery };
    public TerritoryScript TerritoryAttackingFrom = null, TerritoryAttackingOn = null;

    private void Start()
    {
        this.gameObject.tag = "player";
        isTurn = false;

        //just to test to see if the "choose armies" screen will come up if a player has 3 army cards (can delete after done with testing)
        // TODO: change later
        Card card1 = new Card();
        Card card2 = new Card();
        Card card3 = new Card();
        card1.territory_id = "China";
        card1.status = "IN_HAND";
        card1.troop_type = "Infantry";
        card2.territory_id = "Peru";
        card2.status = "IN_HAND";
        card2.troop_type = "Infantry";
        card3.territory_id = "Russia";
        card3.status = "IN_HAND";
        card3.troop_type = "Infantry";
        cardsInHand.Add(card1);
        cardsInHand.Add(card2);
        cardsInHand.Add(card3);
    }

    private void Update()
    {
        if (isTurn && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(CheckWhatWasClickedOn());
        }
    }

    private IEnumerator CheckWhatWasClickedOn()
    {
        // Create a ray from the camera to the mouse cursor
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform the raycast, to see if something was hit
        if (Physics.Raycast(ray, out hit))
        {
            // Check if the raycast hit a tile
            GameObject clickedObject = hit.transform.gameObject;
            string clickedTileTag = clickedObject.tag;
            Debug.Log("Clicked on tile with tag: " + clickedTileTag);

            // Determine which handler to call on 
            if(clickedObject.GetComponent<TerritoryScript>() != null){
                if(canClaimTerritoryAtStart){
                    OnPlayerClaimedTerritoryAtStart?.Invoke(playerNumber, clickedObject);
                }
                else if(canPlaceArmyAtStart){
                    OnPlayerPlacesAnArmyAtStart?.Invoke(playerNumber, clickedObject);
                }
                else if(canPlaceArmyInGame){
                    OnPlayerPlacesAnArmyInGame?.Invoke(playerNumber, clickedObject);
                }
                else if(canSelectAttackFrom){
                    OnPlayerSelectAttackFrom?.Invoke(playerNumber, clickedObject);
                }
                else if(canSelectAttackOn){
                    OnPlayerSelectAttackOn?.Invoke(playerNumber, clickedObject);
                }
                else
                {
                    Debug.Log("Illegal click on territory.");
                }
            }
            else if(clickedObject.GetComponent<DeckScript>() != null){
                if(canDraw){
                    OnPlayerDrawsCard?.Invoke(playerNumber, clickedObject);
                }
                else{
                    Debug.Log("Illegal click on deck.");
                }
            }
            else if(clickedObject.GetComponent<DiceRollerScript>() != null){
                if(canRollToStart){
                    // Call handler.
                    OnRollDiceAtStart?.Invoke(playerNumber, clickedObject);
                }
                else{
                    Debug.Log("Illegal click on dice.");
                }
            }
            else if(clickedObject.GetComponent<GameHUDScript>() != null){
                // For now, do nothing. This is handled by button on click
            }
            else {
                // replace with other game object possibilities. Like dice, for esample.
                Debug.Log("Illegal click.");
            }
              
            isTurn = false; // Player relinquishes its turn. Map decides whether to give the turn
            // back to the player (in the case that the player's turn isn't actually complete)
        }

        yield return null;
    }

    private void MovePieceUnderCorrectConditions()
    {
        //check if its my turn
        //we want the player to be able to select a country, and select a different country if we want to interract with that other country.
        //If the 2nd selected country is ours, we want to deselect the first country, and select the second country
        //If the second country is not ours, then we want to attack that country (if that coutntry is adj to the first country)
        // Note: the map contains the territories, which contain armies. Meaning we should probably handle
        // Moving armies across the board in the map script.
    }

    public void GivePlayerArmies(int _infCount, int _cavCount, int _artilCount)
    {
        infCount += _infCount;
        cavCount += _cavCount;
        artilCount += _artilCount;

        //Update any necessary Huds
    }

    public int GetArmyCountTotal(){
        // TODO: I think it would be clearer to store as the number of pieces, rather than 
        // the number of infantry they represent. We can always just call this function
        // if we want the total number
        return infCount+cavCount+artilCount;
    }

    public void CreateArmy(GameObject armyPrefab, Vector3 position)
    {
        GameObject obj = Instantiate(armyPrefab, position, Quaternion.identity);

        obj.GetComponent<Renderer>().material.color = color;
        obj.GetComponent<ArmyScript>().ownedByPlayerNum = this.playerNumber;
        obj.GetComponent<ArmyScript>().armyCount = 1;
    }
 
    public void ResetAllPermissions(){
        canClaimTerritoryAtStart = false;
        canPlaceArmyAtStart = false;
        canDraw = false;
        canRollToStart = false;
        canTurnInCards = false;
        canSelectAttackFrom = false;
        canSelectAttackOn = false;
        canPlaceArmyInGame = false;
    }
}