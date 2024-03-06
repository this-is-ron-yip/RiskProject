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

    public static string gameStage = "SETUP"; // or: PLAY or: FINISHED

    public bool clickHandled = true; // use for handling one click at a time

    // Define an event that other scripts can subscribe to, to get the player id
    // The int is the player id, the string is the territory_id
    public event Action<int, string> OnPlayerClaimedTerritoryAtStart;
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
        // yield return StartCoroutine(WaitForMouseClick());
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

            // the handler will reset clickHandled
            clickHandled = false;

            // Determine which handler to call on 
            if(clickedObject.GetComponent<TerritoryScript>() != null){
                if(gameStage == "SETUP"){
                    // TODO: Spawnarmypiece should be called later, after error checking
                    SpawnArmyPiece(ArmyTypes.Infantry, clickedObject.transform.position);
                    OnPlayerClaimedTerritoryAtStart?.Invoke(playerNumber, clickedTileTag);
                }
                else if(gameStage == "PLAY"){
                    // Call a different handler. Player is choosing a territory to attack
                    // or choosing a territory to attack from.
                    clickHandled = true; // TODO: Move this to the proper handler
                }
            }
            else {
                // replace with other game object possibilities. Like dice, for esample.
                Debug.Log("This is not a territory.");
                clickHandled = true; // TODO: Move this to the proper handler
            }
              
            isTurn = false; // Player relinquishes its turn. Map decides whether to give the turn
            // back to the player (in the case that the player's turn isn't actually complete)
        }

        // wait until the click is handled (updated by handler)
        yield return new WaitUntil(ClickIsHandled);
    }

    // TODO: delete if we don't end up using this elsewhere
    private IEnumerator WaitForMouseClick()
    {
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
    } 

    private bool ClickIsHandled(){
        return clickHandled;
    }
    

    private void SpawnArmyPiece(ArmyTypes armyType, Vector3 position)
    {
        Debug.Log("SPAWNING ARMY PIECE");
        // TODO: place army on the given position
    }

    private void MovePieceUnderCorrectConditions()
    {
        //check if its my turn
        //we want the player to be able to select a country, and select a different country if we want to interract with that other country.
        //If the 2nd selected country is ours, we want to deselect the first country, and select the second country
        //If the second country is not ours, then we want to attack that country (if that coutntry is adj to the first country)
    }

    public void GivePlayerArmies(int _infCount, int _cavCount, int _artilCount)
    {
        infCount += _infCount;
        cavCount += _cavCount;
        artilCount += _artilCount;

        //Update any necessary Huds
    }

    public int GetArmyCountTotal(){
        // TODO: I think it would be clearer to store as the number of peices, rathere than 
        // the number of infantry they represent. We can always just call this function
        // if we want the total number
        return infCount+cavCount+artilCount;
    }
}