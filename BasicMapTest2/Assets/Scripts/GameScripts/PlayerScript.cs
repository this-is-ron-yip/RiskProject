using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int infCount, cavCount, artilCount, armyCountTotal;
    public int playerNumber;
    public bool isTurn = false;
    public GameObject armyPrefab;
    public List<TerritoryScript> territoriesOwned = new List<TerritoryScript>();
    public List<GameObject> armies = new List<GameObject>();
    enum ArmyTypes { Infantry, Cavalry, Artillery }
    

    private void Start()
    {
        isTurn = false;
    }

    private void Update()
    {
        if (isTurn)
        {
            StartCoroutine(CheckIfTerritoryClickedOn());
        }
    }

    private IEnumerator CheckIfTerritoryClickedOn()
    {
        //ERROR: after mouse is clicked, this method keeps looping for 999+ times for some reason
        //dont forget to set territory as occupied once its been clicked on

        yield return StartCoroutine(WaitForMouseClick());
        // Create a ray from the camera to the mouse cursor
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Debug.Log("TARGET HIT");

        // Perform the raycast
        if (Physics.Raycast(ray, out hit))
        {
            // Check if the raycast hit a tile
            GameObject clickedObject = hit.transform.gameObject;
            string clickedTileTag = clickedObject.tag;
            Debug.Log("Clicked on tile with tag: " + clickedTileTag);
            SpawnArmyPiece(ArmyTypes.Infantry, clickedObject.transform.position);
            isTurn = false;
        }
    }

    private IEnumerator WaitForMouseClick()
    {
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
    } 
    

    private void SpawnArmyPiece(ArmyTypes armyType, Vector3 position)
    {
        Debug.Log("SPAWNING ARMY PIECE");
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
        armyCountTotal = infCount+cavCount+artilCount;

        //Update any necessary Huds
    }
}
