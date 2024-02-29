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

    private void Update()
    {
        MovePieceUnderCorrectConditions();
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
