using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryScript : MonoBehaviour
{
    public int armyCount { get; set; } = 0;
    public int occupiedBy = -1;

    public enum Continents {NorthAmerica, SouthAmerica, Europe, Asia, Africa, Australia};

    public Continents continent; 
    [field: SerializeField] public List<Transform> adjacentCountries { get; set; }

    [field: SerializeField] public List<string> adjacentCountryIDs { get; set; } // Store id rather than transform
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
        textObj.transform.localScale = new Vector3(1, 0.7f, 1);
        textObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        textObj.transform.localEulerAngles = new Vector3(textObj.transform.localEulerAngles.x, textObj.transform.localEulerAngles.y, 90);
    }

    private void Update()
    {
        // if (armyText != null && armyCount >= 0)
        // {
            armyText.text = armyCount.ToString();

        // }
        if (occupiedBy != -1)
        {
            foreach (PlayerScript player in FindObjectsOfType<PlayerScript>())
            {
                if (player.playerNumber == occupiedBy)
                {
                    gameObject.GetComponent<Renderer>().material.color = player.color;
                    break;
                }
            }
        }
    }
    private void OnValidate()
    {
        FillAdjTerritoriesList();
        SetContinents();
    }

    private void FillAdjTerritoriesList()
    {
        adjacentCountries.Clear();
        switch (this.gameObject.tag)
        {
            case "Canada":
                adjacentCountries.Add(GameObject.FindWithTag("UnitedKingdom").GetComponent<Transform>());
                adjacentCountryIDs.Add("UnitedKingdom");
                adjacentCountries.Add(GameObject.FindWithTag("WestAmerica").GetComponent<Transform>());
                adjacentCountryIDs.Add("WestAmerica");
                adjacentCountries.Add(GameObject.FindWithTag("EastAmerica").GetComponent<Transform>());
                adjacentCountryIDs.Add("EastAmerica");
                break;
            case "EastAmerica":
                adjacentCountries.Add(GameObject.FindWithTag("Canada").GetComponent<Transform>());
                adjacentCountryIDs.Add("Canada");
                adjacentCountries.Add(GameObject.FindWithTag("WestAmerica").GetComponent<Transform>());
                adjacentCountryIDs.Add("WestAmerica");
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountryIDs.Add("Brazil");
                break;
            case "WestAmerica":
                adjacentCountries.Add(GameObject.FindWithTag("Canada").GetComponent<Transform>());
                adjacentCountryIDs.Add("Canada");
                adjacentCountries.Add(GameObject.FindWithTag("EastAmerica").GetComponent<Transform>());
                adjacentCountryIDs.Add("EastAmerica");
                adjacentCountries.Add(GameObject.FindWithTag("Peru").GetComponent<Transform>());
                adjacentCountryIDs.Add("Peru");
                break;
            case "Brazil":
                adjacentCountries.Add(GameObject.FindWithTag("Argentina").GetComponent<Transform>());
                adjacentCountryIDs.Add("Argentina");
                adjacentCountries.Add(GameObject.FindWithTag("EastAmerica").GetComponent<Transform>());
                adjacentCountryIDs.Add("EastAmerica");
                adjacentCountries.Add(GameObject.FindWithTag("Peru").GetComponent<Transform>());
                adjacentCountryIDs.Add("Peru");
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                adjacentCountryIDs.Add("NorthAfrica");
                break;
            case "Argentina":
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountryIDs.Add("Brazil");
                adjacentCountries.Add(GameObject.FindWithTag("Peru").GetComponent<Transform>());
                adjacentCountryIDs.Add("Peru");
                break;
            case "UnitedKingdom":
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountryIDs.Add("NorthEurope");
                adjacentCountries.Add(GameObject.FindWithTag("WestEurope").GetComponent<Transform>());
                adjacentCountryIDs.Add("WestEurope");
                adjacentCountries.Add(GameObject.FindWithTag("Canada").GetComponent<Transform>());
                adjacentCountryIDs.Add("Canada");
                break;
            case "Peru":
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountryIDs.Add("Brazil");
                adjacentCountries.Add(GameObject.FindWithTag("Argentina").GetComponent<Transform>());
                adjacentCountryIDs.Add("Argentina");
                adjacentCountries.Add(GameObject.FindWithTag("WestAmerica").GetComponent<Transform>());
                adjacentCountryIDs.Add("WestAmerica");
                break;
            case "WestEurope":
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountryIDs.Add("NorthEurope");
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountryIDs.Add("SouthEurope");
                adjacentCountries.Add(GameObject.FindWithTag("UnitedKingdom").GetComponent<Transform>());
                adjacentCountryIDs.Add("UnitedKingdom");
                break;
            case "NorthEurope":
                adjacentCountries.Add(GameObject.FindWithTag("WestEurope").GetComponent<Transform>());
                adjacentCountryIDs.Add("WestEurope");
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountryIDs.Add("SouthEurope");
                adjacentCountries.Add(GameObject.FindWithTag("UnitedKingdom").GetComponent<Transform>());
                adjacentCountryIDs.Add("UnitedKingdom");
                adjacentCountries.Add(GameObject.FindWithTag("Kazakhstan").GetComponent<Transform>());
                adjacentCountryIDs.Add("Kazakhstan");
                break;
            case "SouthEurope":
                adjacentCountries.Add(GameObject.FindWithTag("WestEurope").GetComponent<Transform>());
                adjacentCountryIDs.Add("WestEurope");
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountryIDs.Add("NorthEurope");
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountryIDs.Add("Egypt");
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountryIDs.Add("MiddleEast");
                break;
            case "Egypt":
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountryIDs.Add("SouthEurope");
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountryIDs.Add("EastAfrica");
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountryIDs.Add("MiddeEast");
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                adjacentCountryIDs.Add("NorthAfrica");
                break;
            case "EastAfrica":
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountryIDs.Add("Egypt");
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                adjacentCountryIDs.Add("NorthAfrica");
                adjacentCountries.Add(GameObject.FindWithTag("SouthAfrica").GetComponent<Transform>());
                adjacentCountryIDs.Add("SouthAfrica");
                adjacentCountries.Add(GameObject.FindWithTag("WestAustralia").GetComponent<Transform>());
                adjacentCountryIDs.Add("WestAustralia");
                break;
            case "NorthAfrica":
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountryIDs.Add("Egypt");
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountryIDs.Add("Brazil");
                adjacentCountries.Add(GameObject.FindWithTag("SouthAfrica").GetComponent<Transform>());
                adjacentCountryIDs.Add("SouthAfrica");
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountryIDs.Add("EastAfrica");
                break;
            case "SouthAfrica":
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                adjacentCountryIDs.Add("NorthAfrica");
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountryIDs.Add("EastAfrica");
                break;
            case "WestAustralia":
                adjacentCountries.Add(GameObject.FindWithTag("EastAustralia").GetComponent<Transform>());
                adjacentCountryIDs.Add("EastAustralia");
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountryIDs.Add("EastAfrica");
                adjacentCountries.Add(GameObject.FindWithTag("India").GetComponent<Transform>());
                adjacentCountryIDs.Add("India");
                break;
            case "EastAustralia":
                adjacentCountries.Add(GameObject.FindWithTag("WestAustralia").GetComponent<Transform>());
                adjacentCountryIDs.Add("WestAustralia");
                break;
            case "India":
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountryIDs.Add("MiddleEast");
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryIDs.Add("China");
                adjacentCountries.Add(GameObject.FindWithTag("WestAustralia").GetComponent<Transform>());
                adjacentCountryIDs.Add("WestAustralia");
                break;
            case "MiddleEast":
                adjacentCountries.Add(GameObject.FindWithTag("India").GetComponent<Transform>());
                adjacentCountryIDs.Add("India");
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountryIDs.Add("Egypt");
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountryIDs.Add("SouthEurope");
                adjacentCountries.Add(GameObject.FindWithTag("Kazakhstan").GetComponent<Transform>());
                adjacentCountryIDs.Add("Kazakhstan");
                break;
            case "Kazakhstan":
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountryIDs.Add("NorthEurope");
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountryIDs.Add("MiddleEast");
                adjacentCountries.Add(GameObject.FindWithTag("Russia").GetComponent<Transform>());
                adjacentCountryIDs.Add("Russia");
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryIDs.Add("China");
                break;
            case "China":
                adjacentCountries.Add(GameObject.FindWithTag("India").GetComponent<Transform>());
                adjacentCountryIDs.Add("India");
                adjacentCountries.Add(GameObject.FindWithTag("Russia").GetComponent<Transform>());
                adjacentCountryIDs.Add("Russia");
                adjacentCountries.Add(GameObject.FindWithTag("Kazakhstan").GetComponent<Transform>());
                adjacentCountryIDs.Add("Kazakhstan");
                break;
            case "Russia":
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryIDs.Add("China");
                adjacentCountries.Add(GameObject.FindWithTag("Kazakhstan").GetComponent<Transform>());
                adjacentCountryIDs.Add("Kazakhstan");
                break;

        }
    }

    private void SetContinents()
    {
        adjacentCountries.Clear();
        switch (this.gameObject.tag)
        {
            case "Canada":
                continent = Continents.NorthAmerica;
                break;
            case "EastAmerica":
                continent = Continents.NorthAmerica;
                break;
            case "WestAmerica":
                continent = Continents.NorthAmerica;
                break;
            case "Brazil":
               continent = Continents.SouthAmerica;
                break;
            case "Argentina":
                continent = Continents.SouthAmerica;
                break;
            case "UnitedKingdom":
               continent = Continents.Europe;
                break;
            case "Peru":
                continent = Continents.SouthAmerica;
                break;
            case "WestEurope":
               continent = Continents.Europe;
                break;
            case "NorthEurope":
               continent = Continents.Europe;
                break;
            case "SouthEurope":
                continent = Continents.Europe;
                break;
            case "Egypt":
                continent = Continents.Africa;
                break;
            case "EastAfrica":
               continent = Continents.Africa;
                break;
            case "NorthAfrica":
                continent = Continents.Africa;
                break;
            case "SouthAfrica":
               continent = Continents.Africa;
                break;
            case "WestAustralia":
                continent = Continents.Australia;
                break;
            case "EastAustralia":
                continent = Continents.Australia;
                break;
            case "India":
                continent = Continents.Asia;
                break;
            case "MiddleEast":
                continent = Continents.Asia;
                break;
            case "Kazakhstan":
                continent = Continents.Asia;
                break;
            case "China":
                continent = Continents.Asia;
                break;
            case "Russia": // TODO: Russia is in two continents, and isn't a territory in risk
                continent = Continents.Asia;
                break;

        }
    }
}
