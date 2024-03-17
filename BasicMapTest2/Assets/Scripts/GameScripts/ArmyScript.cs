using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyScript : MonoBehaviour
{
    public bool isMoving = false;
    public float speed = 5f;
    public int ownedByPlayerNum = -1;
    public string armyType = "inf"; //infantry by default
    public Transform currentTerritoryPos;
    public int armyCount = 0; //how many of this armyType does this obj represent
    private TextMesh armyText; // TextMesh to display the army count.

    public void Start()
    {
        // Ensure there's a child GameObject to hold the TextMesh.
        GameObject textObj = new GameObject("ArmyText");
        textObj.transform.SetParent(transform);

        // Position it above the army unit
        textObj.transform.localPosition = new Vector3(0, 1, 0);

        // Add the TextMesh component and configure it.
        armyText = textObj.AddComponent<TextMesh>();
        armyText.characterSize = 0.042f;
        armyText.anchor = TextAnchor.MiddleCenter;
        armyText.alignment = TextAlignment.Center;
        armyText.fontSize = 100;
        armyText.color = Color.black;
        textObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        textObj.transform.localEulerAngles = new Vector3(textObj.transform.localEulerAngles.x, textObj.transform.localEulerAngles.y, 90);

        UpdateNumberDisplay(); // Initialize the display.
    }

    public void GoToStartTerritory() 
    {
        //maybe, in the Map class where this object was instantiated, we could have already set the currentTerritoryPos field to the correct field that it was chosen to start at
    }

    public void Update()
    {
        UpdateNumberDisplay();
    }

    //army 65
    public Transform GetAndUpdateCurrentTerritoryPos()
    {
        // getting the territory that has the same x and z coord as this object
        List<TerritoryScript> _territories = GameObject.Find("Map").GetComponent<MapScript>().players[ownedByPlayerNum - 1].GetComponent<PlayerScript>().territoriesOwned;
        foreach (TerritoryScript terr in _territories)
        {
            if (terr.gameObject.transform.position.x == this.gameObject.transform.position.x && terr.gameObject.transform.position.z == this.gameObject.transform.position.z)
            {
                currentTerritoryPos = terr.transform;
                return currentTerritoryPos;
            }
        }
        Debug.Log("Could not find territory for this army piece..");
        return null;
    }

    private void UpdateNumberDisplay()
    {
        //find the correct armyCount number using territory
        GetAndUpdateCurrentTerritoryPos();
        this.armyCount = currentTerritoryPos.gameObject.GetComponent<TerritoryScript>().armyCount;
        if (armyText != null && armyCount >= 0)
        {
            armyText.text = armyCount.ToString();
        }
    }

    public void MoveIfMouseClicked()
    {
        //code to move the correct army to the correct place under the correct circumstances (does not need to be implemented right now)
    }
}
