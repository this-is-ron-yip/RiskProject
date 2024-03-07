using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int infCount, cavCount, artilCount;
    public int playerNumber;
    public bool isTurn = false;
    public GameObject armyPrefab;
    public List<TerritoryScript> territoriesOwned = new List<TerritoryScript>();
    public List<GameObject> armies = new List<GameObject>();
    public List<Card> cardsInHand = new List<Card>();
    public List<Card> cardsPlayed = new List<Card>();
    // Create a set of booleans to dictate legal and illegal actions for this player.
    // MapScript will modify these permissions during game play
    private bool canClaimTerritoryAtStart = false;
    private bool canPlaceArmyAtStart = false;
    private bool canDraw = false;
    private bool canRoll = false; // TODO: probably split this into more specific permissoins
                                // Depending on the game implementation
    private bool canTurnInCards = false;
    private bool canSelectAttackFrom = false;
    private bool canSelectAttackWho = false;

    // Define an event that other scripts can subscribe to, to get the player id
    // The int is the player id, the object is the object they clicked on
    public event Action<int, GameObject> OnPlayerClaimedTerritoryAtStart;
    public event Action<int, GameObject> OnPlayerPlacesArmiesAtStart;
    enum ArmyTypes { Infantry, Cavalry, Artillery }
    

    private void Start()
    {
        isTurn = false;
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
                    OnPlayerPlacesArmiesAtStart?.Invoke(playerNumber, clickedObject);
                }
                else if(canSelectAttackFrom){
                    // TODO: Call a different handle
                }
                else if(canSelectAttackWho){
                    // TODO: Call a different handle
                }
                else{
                    Debug.Log("Illegal click.");
                }
            }
            else if(clickedObject.GetComponent<DeckScript>() != null){
                if(canDraw){
                    // TOOD: Call a different handler. But for now, we can test with calling
                    // directly on draw card
                    clickedObject.GetComponent<DeckScript>().DrawCard();
                }
                else{
                    Debug.Log("Illegal click.");
                }
            }
            else if(clickedObject.GetComponent<DiceRollerScript>() != null){
                if(canRoll){
                    // TODO: Call handler.
                }
                else{
                    Debug.Log("Illegal click.");
                }
            }
            else {
                // replace with other game object possibilities. Like dice, for esample.
                Debug.Log("Illegal click.");
            }
              
            isTurn = false; // Player relinquishes its turn. Map decides whether to give the turn
            // back to the player (in the case that the player's turn isn't actually complete)
        }

        yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
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

    public bool CanClaimTerritoryAtStart(){
        return canClaimTerritoryAtStart;
    }
    public bool CanPlaceArmyAtStart(){
        return canPlaceArmyAtStart;
    }
    public bool CanDraw(){
        return canDraw;
    }
    public bool CanRoll(){
        return canRoll;
    }
    public bool CanTurnInCards(){
        return canTurnInCards;
    }
    public bool CanSelectAttackFrom(){
        return canSelectAttackFrom;
    }

    public bool CanSelectAttackWho(){
        return canSelectAttackWho;
    }
    public void AllowClaimTerritoryAtStart(){
        canClaimTerritoryAtStart = true;
    }
    public void AllowPlaceArmyAtStart(){
        canPlaceArmyAtStart = true;
    }
    public void AllowDraw(){
        canDraw = true;
    }
    public void AllowRoll(){
        canRoll = true;
    }
    public void AllowTurnInCards(){
        canTurnInCards = true;
    }
    public void AllowSelectAttackFrom(){
        canSelectAttackFrom = true;
    }
    public void AllowSelectAttackWho(){
        canSelectAttackWho = true;
    }
    public void PreventClaimTerritoryAtStart(){
        canClaimTerritoryAtStart = false;
    }
    public void PreventPlaceArmyAtStart(){
        canPlaceArmyAtStart = false;
    }
    public void PreventDraw(){
        canDraw = false;
    }
    public void PreventRoll(){
        canRoll = false;
    }
    public void PreventTurnInCards(){
        canTurnInCards = false;
    }
    public void PreventSelectAttackFrom(){
        canSelectAttackFrom = false;
    }
    public void PreventSelectAttackWho(){
        canSelectAttackWho = false;
    }
    public void ResetAllPermissions(){
        canDraw = false;
        canRoll = false;
        canTurnInCards = false;
    }
}