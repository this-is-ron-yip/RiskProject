using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyScript : MonoBehaviour
{
    public bool isMoving = false;
    public float speed = 5f;
    public int ownedByPlayerNum;
    public string armyType;
    private Transform target;
    private Transform currentTerritoryPos;

    public void Start()
    {
        //initialise fields: ownedByPlayerNum, armyType
    }

    public void GoToStartTerritory() 
    {
        //maybe, in the Map class where this object was instantiated, we could have already set the currentTerritoryPos field to the correct field that it was chosen to start at
    }

    public void Update()
    {
        //In previous versions, this update function included code to move the army object. This code is now being handled in the player object
    }


    public void MoveIfMouseClicked()
    {
        //code to move the correct army to the correct place under the correct circumstances (does not need to be implemented right now)
    }
}
