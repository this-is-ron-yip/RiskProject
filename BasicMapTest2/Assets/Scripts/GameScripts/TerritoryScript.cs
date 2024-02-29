using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryScript : MonoBehaviour
{
    public string name { get; set; }
    public int armyCount { get; set; }
    public string occupiedBy;
    [field: SerializeField] public List<Transform> adjacentCountries { get; set; }

    private void OnValidate()
    {
        FillAdjTerritoriesList();
    }

    private void FillAdjTerritoriesList()
    {
        adjacentCountries.Clear();
        int count = 0;
        switch (this.gameObject.tag)
        {
            case "Canada":
                adjacentCountries.Add(GameObject.FindWithTag("UnitedKingdom").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("WestAmerica").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("EastAmerica").GetComponent<Transform>());
                break;
            case "EastAmerica":
                adjacentCountries.Add(GameObject.FindWithTag("Canada").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("WestAmerica").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                break;
            case "WestAmerica":
                adjacentCountries.Add(GameObject.FindWithTag("Canada").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("EastAmerica").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Peru").GetComponent<Transform>());
                break;
            case "Brazil":
                adjacentCountries.Add(GameObject.FindWithTag("Argentina").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("EastAmerica").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Peru").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                break;
            case "Argentina":
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Peru").GetComponent<Transform>());
                break;
            case "UnitedKingdom":
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("WestEurope").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Canada").GetComponent<Transform>());
                break;
            case "Peru":
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Argentina").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("WestAmerica").GetComponent<Transform>());
                break;
            case "WestEurope":
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("UnitedKingdom").GetComponent<Transform>());
                break;
            case "NorthEurope":
                adjacentCountries.Add(GameObject.FindWithTag("WestEurope").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("UnitedKingdom").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Kazakhstan").GetComponent<Transform>());
                break;
            case "SouthEurope":
                adjacentCountries.Add(GameObject.FindWithTag("WestEurope").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                break;
            case "Egypt":
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                break;
            case "EastAfrica":
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("SouthAfrica").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("WestAustralia").GetComponent<Transform>());
                break;
            case "NorthAfrica":
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("SouthAfrica").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                break;
            case "SouthAfrica":
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                break;
            case "WestAustralia":
                adjacentCountries.Add(GameObject.FindWithTag("EastAustralia").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("India").GetComponent<Transform>());
                break;
            case "EastAustralia":
                adjacentCountries.Add(GameObject.FindWithTag("WestAustralia").GetComponent<Transform>());
                break;
            case "India":
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("WestAustralia").GetComponent<Transform>());
                break;
            case "MiddleEast":
                adjacentCountries.Add(GameObject.FindWithTag("India").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Kazakhstan").GetComponent<Transform>());
                break;
            case "Kazakhstan":
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Russia").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                break;
            case "China":
                adjacentCountries.Add(GameObject.FindWithTag("India").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Russia").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Kazakhstan").GetComponent<Transform>());
                break;
            case "Russia":
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Kazakhstan").GetComponent<Transform>());
                break;

        }
    }
}
