using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryScript : MonoBehaviour
{
    public int armyCount { get; set; } = 0;
    public int occupiedBy = -1;

    public enum Continents {NorthAmerica, SouthAmerica, Europe, Asia, Africa, Australia};

    // TODO: should be a total of 41 territories
    public enum TerritoryEnum {Canada, EastAmerica, WestAmerica, Brazil, Argentina, UnitedKingdom, 
                Peru, WestEurope, NorthEurope, SouthEurope, Egypt, EastAfrica, NorthAfrica,
                SouthAfrica, WestAustralia, EastAustralia, India, MiddleEast, Kazakhstan, China, Russia };

    public Continents continent; 
    public TerritoryEnum territoryEnum;
    [field: SerializeField] public List<Transform> adjacentCountries { get; set; }

    [field: SerializeField] public List<TerritoryEnum> adjacentCountryEnums { get; set; } // Store id rather than transform
    private TextMesh armyText; // TextMesh to display the army count.

    public const int NUMBER_OF_TERRITORIES = 10; // TODO: change to proper number after testing is done
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
        SetTerritoryEnums();
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
                adjacentCountryEnums.Add(TerritoryEnum.UnitedKingdom);
                adjacentCountries.Add(GameObject.FindWithTag("WestAmerica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestAmerica);
                adjacentCountries.Add(GameObject.FindWithTag("EastAmerica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAmerica);
                break;
            case "EastAmerica":
                adjacentCountries.Add(GameObject.FindWithTag("Canada").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Canada);
                adjacentCountries.Add(GameObject.FindWithTag("WestAmerica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestAmerica);
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Brazil);
                break;
            case "WestAmerica":
                adjacentCountries.Add(GameObject.FindWithTag("Canada").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Canada);
                adjacentCountries.Add(GameObject.FindWithTag("EastAmerica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAmerica);
                adjacentCountries.Add(GameObject.FindWithTag("Peru").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Peru);
                break;
            case "Brazil":
                adjacentCountries.Add(GameObject.FindWithTag("Argentina").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Argentina);
                adjacentCountries.Add(GameObject.FindWithTag("EastAmerica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAmerica);
                adjacentCountries.Add(GameObject.FindWithTag("Peru").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Peru);
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthAfrica);
                break;
            case "Argentina":
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Brazil);
                adjacentCountries.Add(GameObject.FindWithTag("Peru").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Peru);
                break;
            case "UnitedKingdom":
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthEurope);
                adjacentCountries.Add(GameObject.FindWithTag("WestEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestEurope);
                adjacentCountries.Add(GameObject.FindWithTag("Canada").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Canada);
                break;
            case "Peru":
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Brazil);
                adjacentCountries.Add(GameObject.FindWithTag("Argentina").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Argentina);
                adjacentCountries.Add(GameObject.FindWithTag("WestAmerica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestAmerica);
                break;
            case "WestEurope":
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthEurope);
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.SouthEurope);
                adjacentCountries.Add(GameObject.FindWithTag("UnitedKingdom").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.UnitedKingdom);
                break;
            case "NorthEurope":
                adjacentCountries.Add(GameObject.FindWithTag("WestEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestEurope);
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.SouthEurope);
                adjacentCountries.Add(GameObject.FindWithTag("UnitedKingdom").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.UnitedKingdom);
                adjacentCountries.Add(GameObject.FindWithTag("Kazakhstan").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Kazakhstan);
                break;
            case "SouthEurope":
                adjacentCountries.Add(GameObject.FindWithTag("WestEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestEurope);
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthEurope);
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Egypt);
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.MiddleEast);
                break;
            case "Egypt":
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.SouthEurope);
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.MiddleEast);
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthAfrica);
                break;
            case "EastAfrica":
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Egypt);
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("SouthAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.SouthAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("WestAustralia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestAustralia);
                break;
            case "NorthAfrica":
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Egypt);
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Brazil);
                adjacentCountries.Add(GameObject.FindWithTag("SouthAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.SouthAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAfrica);
                break;
            case "SouthAfrica":
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAfrica);
                break;
            case "WestAustralia":
                adjacentCountries.Add(GameObject.FindWithTag("EastAustralia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAustralia);
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("India").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.India);
                break;
            case "EastAustralia":
                adjacentCountries.Add(GameObject.FindWithTag("WestAustralia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestAustralia);
                break;
            case "India":
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.MiddleEast);
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.China);
                adjacentCountries.Add(GameObject.FindWithTag("WestAustralia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestAustralia);
                break;
            case "MiddleEast":
                adjacentCountries.Add(GameObject.FindWithTag("India").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.India);
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Egypt);
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.SouthEurope);
                adjacentCountries.Add(GameObject.FindWithTag("Kazakhstan").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Kazakhstan);
                break;
            case "Kazakhstan":
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthEurope);
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.MiddleEast);
                adjacentCountries.Add(GameObject.FindWithTag("Russia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Russia);
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.China);
                break;
            case "China":
                adjacentCountries.Add(GameObject.FindWithTag("India").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.India);
                adjacentCountries.Add(GameObject.FindWithTag("Russia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Russia);
                adjacentCountries.Add(GameObject.FindWithTag("Kazakhstan").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Kazakhstan);
                break;
            case "Russia":
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.China);
                adjacentCountries.Add(GameObject.FindWithTag("Kazakhstan").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Kazakhstan);
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
    private void SetTerritoryEnums()
    {
        adjacentCountries.Clear();
        switch (this.gameObject.tag)
        {
            case "Canada":
                territoryEnum = TerritoryEnum.Canada;
                break;
            case "EastAmerica":
                territoryEnum = TerritoryEnum.EastAmerica;
                break;
            case "WestAmerica":
                territoryEnum = TerritoryEnum.WestAmerica;
                break;
            case "Brazil":
               territoryEnum = TerritoryEnum.Brazil;
                break;
            case "Argentina":
                territoryEnum = TerritoryEnum.Argentina;
                break;
            case "UnitedKingdom":
               territoryEnum = TerritoryEnum.UnitedKingdom;
                break;
            case "Peru":
                territoryEnum = TerritoryEnum.Peru;
                break;
            case "WestEurope":
               territoryEnum = TerritoryEnum.WestEurope;
                break;
            case "NorthEurope":
               territoryEnum = TerritoryEnum.NorthEurope;
                break;
            case "SouthEurope":
                territoryEnum = TerritoryEnum.SouthEurope;
                break;
            case "Egypt":
                territoryEnum = TerritoryEnum.Egypt;
                break;
            case "EastAfrica":
               territoryEnum = TerritoryEnum.EastAfrica;
                break;
            case "NorthAfrica":
                territoryEnum = TerritoryEnum.NorthAfrica;
                break;
            case "SouthAfrica":
               territoryEnum = TerritoryEnum.SouthAfrica;
                break;
            case "WestAustralia":
                territoryEnum = TerritoryEnum.WestAustralia;
                break;
            case "EastAustralia":
                territoryEnum = TerritoryEnum.EastAustralia;
                break;
            case "India":
                territoryEnum = TerritoryEnum.India;
                break;
            case "MiddleEast":
                territoryEnum = TerritoryEnum.MiddleEast;
                break;
            case "Kazakhstan":
                territoryEnum = TerritoryEnum.Kazakhstan;
                break;
            case "China":
                territoryEnum = TerritoryEnum.China;
                break;
            case "Russia":
                territoryEnum = TerritoryEnum.Russia;
                break;

        }
    }
}
