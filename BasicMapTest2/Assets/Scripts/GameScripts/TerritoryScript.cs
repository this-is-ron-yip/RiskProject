using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryScript : MonoBehaviour
{
    public string territory_id { get; set; }
    public int armyCount { get; set; } = 0;
    public int occupiedBy = -1;

    public enum Continents {NorthAmerica, SouthAmerica, Europe, Asia, Africa, Australia};

    public Continents continent; 
    [field: SerializeField] public List<Transform> adjacentCountries { get; set; }
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
