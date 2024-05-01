using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int infCount; // How many armies they have.
    public int playerNumber; // 1 indexed. Used as this player's id. 
    public bool clickExpected = false; // If true, the game is waiting for this player to click.
    public bool eliminated = false; // flag to be checked by MapScript
    public bool wonTerritory = false; // Flag to be checked by MapScript
    public Color color = new Color(0, 0, 0);
    public List<TerritoryScript> territoriesOwned = new List<TerritoryScript>();
    public SoundEffectsPlayer sfxPlayer;
    public List<Card> cardsInHand = new List<Card>();
    public TerritoryScript TerritoryAttackingFrom = null;
    public TerritoryScript TerritoryAttackingOn = null;
    public TerritoryScript TerritoryMoveFrom = null;
    public TerritoryScript TerritoryMoveTo = null;
    public Dictionary<TerritoryScript.Continents, int> territoryCountsPerContinent =  
        new Dictionary<TerritoryScript.Continents, int> (){
            {TerritoryScript.Continents.NorthAmerica, 0}, {TerritoryScript.Continents.SouthAmerica, 0},
            {TerritoryScript.Continents.Europe, 0}, {TerritoryScript.Continents.Asia, 0}, 
            {TerritoryScript.Continents.Africa, 0}, {TerritoryScript.Continents.Australia, 0}
    };
    enum ArmyTypes { Infantry, Cavalry, Artillery };

    /*****************************************************************************
    Create a set of booleans to dictate legal and illegal actions for this player.
    MapScript will modify these permissions during game play.
    ******************************************************************************/
    public bool canClaimTerritoryAtStart = false; // During game set up, when selecting new territories to claim.
    public bool canPlaceArmyAtStart = false; // During game set up, when placing armies on claimned territories
    public bool canDraw = false; // At the end of each turn, the player may draw a card from the deck.
    public bool canRollToStart = false; // At the start of the game, when determining player order.
    public bool canTurnInCards = false; // During the player's turn, when they are asked or requried to turn in cards.
    public bool canSelectAttackFrom = false; // During an attack sequence, when selecting where to attack from
    public bool canSelectAttackOn = false; // During an attack sequence, when selecting where to attack on
    public bool canSelectMoveFrom = false; // During fortification, when selecting where to move armies from
    public bool canSelectMoveTo = false; // During fortification, when selecting where to move armies to
    public bool canPlaceArmyInGame = false; // During player's turn, when selecting where to place granted armies
    /*****************************************************************************
    End of permissions
    ******************************************************************************/


    /*****************************************************************************
    Define an event that the MapScript can subscribe to, then call the appropriate
    handler. The int is the player id, the object is the object they clicked on.
    The events will be invoked when a certain click occurs.
    ******************************************************************************/
    public event Action<int, GameObject> OnPlayerClaimedTerritoryAtStart;
    public event Action<int, GameObject> OnPlayerPlacesAnArmyAtStart;
    public event Action<int, GameObject> OnPlayerPlacesAnArmyInGame;
    public event Action<int, GameObject> OnPlayerSelectAttackFrom;
    public event Action<int, GameObject> OnPlayerSelectAttackOn;
    public event Action<int, GameObject> OnPlayerSelectMoveFrom;
    public event Action<int, GameObject> OnPlayerSelectMoveTo;
    public event Action<int, GameObject> OnRollDiceAtStart;
    public event Action<int, GameObject> OnPlayerDrawsCard;
    /*****************************************************************************
    End of events.
    ******************************************************************************/

   /// <summary>
   /// Called by unity engine. Initialise the player's game object and data
   /// </summary>
    private void Start()
    {
        sfxPlayer = GameObject.Find("HUDController").GetComponent<SoundEffectsPlayer>();
        gameObject.tag = "player";
        clickExpected = false;
    }

    /// <summary>
    /// On every frame, check whether a click occurred and whether this player was expected
    /// to click. If so, process the click by calling CheckWhatWasClickedOn();
    /// </summary>
    private void Update()
    {
        if (clickExpected && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(CheckWhatWasClickedOn());
        }
    }

    /// <summary>
    /// Based on this player's permissions and the object that was clicked on, 
    /// invoke the proper event. Once this event is invoked, the MapScript will
    /// finish hanlding the click. Do not invoke any event if the click is invalid.
    /// </summary>
    /// <returns></returns>
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

            // Clicked on a territory object
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
                else if(canSelectMoveFrom){
                    OnPlayerSelectMoveFrom?.Invoke(playerNumber, clickedObject);
                }
                else if(canSelectMoveTo){
                    OnPlayerSelectMoveTo?.Invoke(playerNumber, clickedObject);
                }
                else
                {
                    GameObject.FindWithTag("GameHUD").GetComponent<GameHUDScript>().errorCardTMP.text = "Error: Illegal Click";
                    sfxPlayer.PlayErrorSound();
                }
            }
            // Clicked on the deck
            else if(clickedObject.GetComponent<DeckScript>() != null){
                if(canDraw){
                    OnPlayerDrawsCard?.Invoke(playerNumber, clickedObject);
                }
                else{
                    Debug.Log("Illegal click on deck.");
                    sfxPlayer.PlayErrorSound();
                }
            }
            // Clicked on the dice
            else if(clickedObject.GetComponent<DiceRollerScript>() != null){
                if(canRollToStart){
                    // Call handler.
                    OnRollDiceAtStart?.Invoke(playerNumber, clickedObject);
                }
                else{
                    Debug.Log("Illegal click on dice.");
                    sfxPlayer.PlayErrorSound();
                }
            }
            // Clicked on the game hud
            else if(clickedObject.GetComponent<GameHUDScript>() != null){
                // Do nothing. The game hud will detect clicks on buttons.
            }
            else {
                // replace with other game object possibilities. Like dice, for esample.
                Debug.Log("Illegal click.");
                sfxPlayer.PlayErrorSound();
            }
              
            clickExpected = false; // Player resets clickExpected. The MapScript decides when to 
            // await another click from this player. 
        }

        yield return null;
    }
    /// <summary>
    /// Return the number of armies owned by this player.
    /// </summary>
    /// <returns></returns>
    public int GetArmyCountTotal(){
        // We only store armies in terms of infantry, so simply return infCount
        return infCount;
    }

    /// <summary>
    /// Create an army game object, and assign it the proper player and color.
    /// </summary>
    /// <param name="armyPrefab"></param>
    /// <param name="position"></param>
    /// TODO: maybe delete this function if it isn't being used.
    public void CreateArmy(GameObject armyPrefab, Vector3 position)
    {
        GameObject obj = Instantiate(armyPrefab, position, Quaternion.identity);

        obj.GetComponent<Renderer>().material.color = color;
        obj.GetComponent<ArmyScript>().ownedByPlayerNum = playerNumber;
        obj.GetComponent<ArmyScript>().armyCount = 1;
    }
 
    /// <summary>
    /// Set all permission booleans to false. Called by MapScript at the start of every turn.
    /// </summary>
    public void ResetAllPermissions(){
        canClaimTerritoryAtStart = false;
        canPlaceArmyAtStart = false;
        canDraw = false;
        canRollToStart = false;
        canTurnInCards = false;
        canSelectAttackFrom = false;
        canSelectAttackOn = false;
        canPlaceArmyInGame = false;
        wonTerritory = false;
        canSelectMoveFrom = false;
        canSelectMoveTo = false;
    }
}