using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryScript : MonoBehaviour
{

    /* TODO: 
     * Re-add countries to TerritoryEnum
     * Add Adj countries to FillAdjTerritoriesList() method
     * Add countries to their respective continents in the SetContinents() method
     * Set territories' enums in the SetTerritoriesEnums method
     * 
     * Check if there are any references to countries/territories/continents in other scripts that needs updating
     */

    public int armyCount { get; set; } = 0;
    public int occupiedBy = -1;

    public enum Continents {NorthAmerica, SouthAmerica, Europe, Asia, Africa, Australia};

    // TODO: should be a total of 42 territories
    public enum TerritoryEnum {Alaska, NorthWestTerritory, NorthEastTerritory, Greenland, Qubec, Alberta, EastUSA, WestUSA, CentralAmerica, Venezuela, Peru, Brazil,
                                Argentina, NorthAfrica, Egypt, Congo, EastAfrica, SouthAfrica, Madagascar, Iceland, UnitedKingdom, Scandanavia, Ukraine, WestEurope, NorthEurope,
                                SouthEurope, Ural, Siberia, Yakutsk, Kamchatka, Mongolia, China, Japan, Afghanistan, MiddleEast, Siam, Indonesia, India, NewGuinea, WestAustralia,
                                EastAustralia, NewZealand};

    public Continents continent; 
    public TerritoryEnum territoryEnum;
    [field: SerializeField] public List<Transform> adjacentCountries { get; set; }

    [field: SerializeField] public List<TerritoryEnum> adjacentCountryEnums { get; set; } // Store id rather than transform
    private TextMesh armyText; // TextMesh to display the army count.

    public const int NUMBER_OF_TERRITORIES = 6; // TODO: change to proper number after testing is done
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
            case "Alaska":
                adjacentCountries.Add(GameObject.FindWithTag("NorthWestTerritory").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthWestTerritory);
                adjacentCountries.Add(GameObject.FindWithTag("Alberta").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Alberta);
                adjacentCountries.Add(GameObject.FindWithTag("Kamchatka").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Kamchatka);
                adjacentCountries.Add(GameObject.FindWithTag("Japan").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Japan);
                break;
            case "NorthWestTerritory":
                adjacentCountries.Add(GameObject.FindWithTag("Alaska").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Alaska);
                adjacentCountries.Add(GameObject.FindWithTag("NorthEastTerritory").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthEastTerritory);
                adjacentCountries.Add(GameObject.FindWithTag("Alberta").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Alberta);
                adjacentCountries.Add(GameObject.FindWithTag("Qubec").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Qubec);
                break;
            case "NorthEastTerritory":
                adjacentCountries.Add(GameObject.FindWithTag("NorthWestTerritory").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthWestTerritory);
                adjacentCountries.Add(GameObject.FindWithTag("Greenland").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Greenland);
                adjacentCountries.Add(GameObject.FindWithTag("Qubec").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Qubec);
                break;
            case "Greenland":
                adjacentCountries.Add(GameObject.FindWithTag("NorthEastTerritory").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthEastTerritory);
                adjacentCountries.Add(GameObject.FindWithTag("Iceland").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Iceland);
                break;
            case "Qubec":
                adjacentCountries.Add(GameObject.FindWithTag("NorthEastTerritory").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthEastTerritory);
                adjacentCountries.Add(GameObject.FindWithTag("EastUSA").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastUSA);
                break;
            case "Alberta":
                adjacentCountries.Add(GameObject.FindWithTag("Alaska").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Alaska);
                adjacentCountries.Add(GameObject.FindWithTag("NorthWestTerritory").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthWestTerritory);
                adjacentCountries.Add(GameObject.FindWithTag("Qubec").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Qubec);
                adjacentCountries.Add(GameObject.FindWithTag("WestUSA").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestUSA);
                break;
            case "EastUSA":
                adjacentCountries.Add(GameObject.FindWithTag("WestUSA").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestUSA);
                adjacentCountries.Add(GameObject.FindWithTag("Qubec").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Qubec);
                break;
            case "WestUSA":
                adjacentCountries.Add(GameObject.FindWithTag("EastUSA").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastUSA);
                adjacentCountries.Add(GameObject.FindWithTag("Qubec").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Qubec);
                adjacentCountries.Add(GameObject.FindWithTag("Alberta").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Alberta);
                adjacentCountries.Add(GameObject.FindWithTag("CentralAmerica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.CentralAmerica);
                break;
            case "CentralAmerica":
                adjacentCountries.Add(GameObject.FindWithTag("WestUSA").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestUSA);
                adjacentCountries.Add(GameObject.FindWithTag("Venezuela").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Venezuela);
                break;
            case "Venezuela":
                adjacentCountries.Add(GameObject.FindWithTag("Peru").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Peru);
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Brazil);
                adjacentCountries.Add(GameObject.FindWithTag("CentralAmerica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.CentralAmerica);
                break;
            case "Brazil":
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("Venezuela").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Venezuela);
                adjacentCountries.Add(GameObject.FindWithTag("Peru").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Peru);
                adjacentCountries.Add(GameObject.FindWithTag("Argentina").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Argentina);
                break;
            case "Peru":
                adjacentCountries.Add(GameObject.FindWithTag("Venezuela").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Venezuela);
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Brazil);
                adjacentCountries.Add(GameObject.FindWithTag("Argentina").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Argentina);
                break;
            case "Argentina":
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Brazil);
                adjacentCountries.Add(GameObject.FindWithTag("Peru").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Peru);
                break;
            case "NorthAfrica":
                adjacentCountries.Add(GameObject.FindWithTag("Brazil").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Brazil);
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Egypt);
                adjacentCountries.Add(GameObject.FindWithTag("Congo").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Congo);
                break;
            case "Egypt":
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("Congo").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Congo);
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.MiddleEast);
                break;
            case "EastAfrica":
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("Madagascar").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Madagascar);
                adjacentCountries.Add(GameObject.FindWithTag("Congo").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Congo);
                adjacentCountries.Add(GameObject.FindWithTag("SouthAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.SouthAfrica);
                break;
            case "Congo":
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("NorthAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("SouthAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.SouthAfrica);
                break;
            case "SouthAfrica":
                adjacentCountries.Add(GameObject.FindWithTag("Madagascar").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Madagascar);
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("Congo").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Congo);
                break;
            case "Madagascar":
                adjacentCountries.Add(GameObject.FindWithTag("SouthAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.SouthAfrica);
                adjacentCountries.Add(GameObject.FindWithTag("EastAfrica").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAfrica);
                break;
            case "Iceland":
                adjacentCountries.Add(GameObject.FindWithTag("Greenland").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Greenland);
                adjacentCountries.Add(GameObject.FindWithTag("UnitedKingdom").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.UnitedKingdom);
                adjacentCountries.Add(GameObject.FindWithTag("Scandanavia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Scandanavia);
                break;
            case "UnitedKingdom":
                adjacentCountries.Add(GameObject.FindWithTag("Iceland").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Iceland);
                adjacentCountries.Add(GameObject.FindWithTag("WestEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestEurope);
                adjacentCountries.Add(GameObject.FindWithTag("Scandanavia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Scandanavia);
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthEurope);
                break;
            case "Scandanavia":
                adjacentCountries.Add(GameObject.FindWithTag("Iceland").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Iceland);
                adjacentCountries.Add(GameObject.FindWithTag("UnitedKingdom").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.UnitedKingdom);
                adjacentCountries.Add(GameObject.FindWithTag("Ukraine").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Ukraine);
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthEurope);
                break;
            case "Ukraine":
                adjacentCountries.Add(GameObject.FindWithTag("Scandanavia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Scandanavia);
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.SouthEurope);
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthEurope);
                adjacentCountries.Add(GameObject.FindWithTag("Ural").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Ural);
                adjacentCountries.Add(GameObject.FindWithTag("Afghanistan").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Afghanistan);
                break;
            case "SouthEurope":
                adjacentCountries.Add(GameObject.FindWithTag("WestEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestEurope);
                adjacentCountries.Add(GameObject.FindWithTag("Ukraine").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Ukraine);
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthEurope);
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.MiddleEast);
                break;
            case "NorthEurope":
                adjacentCountries.Add(GameObject.FindWithTag("WestEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestEurope);
                adjacentCountries.Add(GameObject.FindWithTag("Ukraine").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Ukraine);
                adjacentCountries.Add(GameObject.FindWithTag("NorthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NorthEurope);
                adjacentCountries.Add(GameObject.FindWithTag("Iceland").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Iceland);
                adjacentCountries.Add(GameObject.FindWithTag("UnitedKingdom").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.UnitedKingdom);
                adjacentCountries.Add(GameObject.FindWithTag("Scandanavia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Scandanavia);
                break;
            case "Ural":
                adjacentCountries.Add(GameObject.FindWithTag("Ukraine").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Ukraine);
                adjacentCountries.Add(GameObject.FindWithTag("Siberia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Siberia);
                adjacentCountries.Add(GameObject.FindWithTag("Afghanistan").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Afghanistan);
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.China);
                break;
            case "Siberia":
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.China);
                adjacentCountries.Add(GameObject.FindWithTag("Ural").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Ural);
                adjacentCountries.Add(GameObject.FindWithTag("Yakutsk").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Yakutsk);
                adjacentCountries.Add(GameObject.FindWithTag("Mongolia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Mongolia);
                break;
            case "Yakutsk":
                adjacentCountries.Add(GameObject.FindWithTag("Mongolia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Mongolia);
                adjacentCountries.Add(GameObject.FindWithTag("Siberia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Siberia);
                adjacentCountries.Add(GameObject.FindWithTag("Kamchatka").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Kamchatka);
                break;
            case "Kamchatka":
                adjacentCountries.Add(GameObject.FindWithTag("Mongolia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Mongolia);
                adjacentCountries.Add(GameObject.FindWithTag("Japan").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Japan);
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.China);
                adjacentCountries.Add(GameObject.FindWithTag("Yakutsk").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Yakutsk);
                adjacentCountries.Add(GameObject.FindWithTag("Alaska").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Alaska);
                break;
            case "Mongolia":
                adjacentCountries.Add(GameObject.FindWithTag("Siberia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Siberia);
                adjacentCountries.Add(GameObject.FindWithTag("Yakutsk").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Yakutsk);
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.China);
                adjacentCountries.Add(GameObject.FindWithTag("Kamchatka").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Kamchatka);
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.China);
                break;
            case "Japan":
                adjacentCountries.Add(GameObject.FindWithTag("Kamchatka").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Kamchatka);
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.China);
                adjacentCountries.Add(GameObject.FindWithTag("Alaska").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Alaska);
                break;
            case "MiddleEast":
                adjacentCountries.Add(GameObject.FindWithTag("Egypt").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Egypt);
                adjacentCountries.Add(GameObject.FindWithTag("SouthEurope").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.SouthEurope);
                adjacentCountries.Add(GameObject.FindWithTag("Afghanistan").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Afghanistan);
                break;
            case "Afghanistan":
                adjacentCountries.Add(GameObject.FindWithTag("MiddleEast").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.MiddleEast);
                adjacentCountries.Add(GameObject.FindWithTag("Ural").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Ural);
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.China);
                adjacentCountries.Add(GameObject.FindWithTag("India").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.India);
                adjacentCountries.Add(GameObject.FindWithTag("Ukraine").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Ukraine);
                break;
            case "China":
                adjacentCountries.Add(GameObject.FindWithTag("Siberia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Siberia);
                adjacentCountries.Add(GameObject.FindWithTag("Ural").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Ural);
                adjacentCountries.Add(GameObject.FindWithTag("Mongolia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Mongolia);
                adjacentCountries.Add(GameObject.FindWithTag("Kamchatka").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Kamchatka);
                adjacentCountries.Add(GameObject.FindWithTag("Siam").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Siam);
                adjacentCountries.Add(GameObject.FindWithTag("India").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.India);
                adjacentCountries.Add(GameObject.FindWithTag("Afghanistan").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Afghanistan);
                adjacentCountries.Add(GameObject.FindWithTag("Japan").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Japan);
                adjacentCountries.Add(GameObject.FindWithTag("Indonesia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Indonesia);
                break;
            case "India":
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.China);
                adjacentCountries.Add(GameObject.FindWithTag("Siam").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Siam);
                adjacentCountries.Add(GameObject.FindWithTag("Afghanistan").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Afghanistan);
                break;
            case "Siam":
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.China);
                adjacentCountries.Add(GameObject.FindWithTag("India").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.India);
                break;
            case "Indonesia":
                adjacentCountries.Add(GameObject.FindWithTag("China").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.China);
                adjacentCountries.Add(GameObject.FindWithTag("NewGuinea").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NewGuinea);
                break;
            case "NewGuinea":
                adjacentCountries.Add(GameObject.FindWithTag("Indonesia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.Indonesia);
                adjacentCountries.Add(GameObject.FindWithTag("WestAustralia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestAustralia);
                break;
            case "WestAustralia":
                adjacentCountries.Add(GameObject.FindWithTag("EastAustralia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAustralia);
                adjacentCountries.Add(GameObject.FindWithTag("NewGuinea").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NewGuinea);
                break;
            case "EastAustralia":
                adjacentCountries.Add(GameObject.FindWithTag("WestAustralia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.WestAustralia);
                adjacentCountries.Add(GameObject.FindWithTag("NewZealand").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.NewZealand);
                break;
            case "NewZealand":
                adjacentCountries.Add(GameObject.FindWithTag("EastAustralia").GetComponent<Transform>());
                adjacentCountryEnums.Add(TerritoryEnum.EastAustralia);
                break;
        }
    }

    private void SetContinents()
    {
        adjacentCountries.Clear();
        switch (this.gameObject.tag)
        {
            case "Alaska":
                continent = Continents.NorthAmerica;
                break;
            case "NorthWestTerritory":
                continent = Continents.NorthAmerica;
                break;
            case "NorthEastTerritory":
                continent = Continents.NorthAmerica;
                break;
            case "Greenland":
                continent = Continents.NorthAmerica;
                break;
            case "Qubec":
                continent = Continents.NorthAmerica;
                break;
            case "Alberta":
                continent = Continents.NorthAmerica;
                break;
            case "EastUSA":
                continent = Continents.NorthAmerica;
                break;
            case "WestUSA":
                continent = Continents.NorthAmerica;
                break;
            case "CentralAmerica":
                continent = Continents.NorthAmerica;
                break;
            case "Venezuela":
                continent = Continents.SouthAmerica;
                break;
            case "Peru":
                continent = Continents.SouthAmerica;
                break;
            case "Brazil":
                continent = Continents.SouthAmerica;
                break;
            case "Argentina":
                continent = Continents.SouthAmerica;
                break;
            case "NorthAfrica":
                continent = Continents.Africa;
                break;
            case "Egypt":
                continent = Continents.Africa;
                break;
            case "EastAfrica":
                continent = Continents.Africa;
                break;
            case "Congo":
                continent = Continents.Africa;
                break;
            case "SouthAfrica":
                continent = Continents.Africa;
                break;
            case "Madagascar":
                continent = Continents.Africa;
                break;
            case "Iceland":
                continent = Continents.Europe;
                break;
            case "UnitedKingdom":
                continent = Continents.Europe;
                break;
            case "Scandanavia":
                continent = Continents.Europe;
                break;
            case "Ukraine":
                continent = Continents.Europe;
                break;
            case "WestEurope":
                continent = Continents.Europe;
                break;
            case "SouthEurope":
                continent = Continents.Europe;
                break;
            case "NorthEurope":
                continent = Continents.Europe;
                break;
            case "Ural":
                continent = Continents.Asia;
                break;
            case "Siberia":
                continent = Continents.Asia;
                break;
            case "Yakutsk":
                continent = Continents.Asia;
                break;
            case "Kamchatka":
                continent = Continents.Asia;
                break;
            case "Mongolia":
                continent = Continents.Asia;
                break;
            case "Japan":
                continent = Continents.Asia;
                break;
            case "China":
                continent = Continents.Asia;
                break;
            case "MiddleEast":
                continent = Continents.Asia;
                break;
            case "Afghanistan":
                continent = Continents.Asia;
                break;
            case "India":
                continent = Continents.Asia;
                break;
            case "Siam":
                continent = Continents.Asia;
                break;
            case "Indonesia":
                continent = Continents.Asia;
                break;
        }
    }
    private void SetTerritoryEnums()
    {
        adjacentCountries.Clear();
        switch (this.gameObject.tag)
        {
            case "Alaska":
                territoryEnum = TerritoryEnum.Alaska;
                break;
            case "NorthWestTerritory":
                territoryEnum = TerritoryEnum.NorthWestTerritory;
                break;
            case "NorthEastTerritory":
                territoryEnum = TerritoryEnum.NorthEastTerritory;
                break;
            case "Greenland":
                territoryEnum = TerritoryEnum.Greenland;
                break;
            case "Qubec":
                territoryEnum = TerritoryEnum.Qubec;
                break;
            case "Alberta":
                territoryEnum = TerritoryEnum.Alberta;
                break;
            case "EastUSA":
                territoryEnum = TerritoryEnum.EastUSA;
                break;
            case "WestUSA":
                territoryEnum = TerritoryEnum.WestUSA;
                break;
            case "CentralAmerica":
                territoryEnum = TerritoryEnum.CentralAmerica;
                break;
            case "Venezuela":
                territoryEnum = TerritoryEnum.Venezuela;
                break;
            case "Peru":
                territoryEnum = TerritoryEnum.Peru;
                break;
            case "Brazil":
                territoryEnum = TerritoryEnum.Brazil;
                break;
            case "Argentina":
                territoryEnum = TerritoryEnum.Argentina;
                break;
            case "NorthAfrica":
                territoryEnum = TerritoryEnum.NorthAfrica;
                break;
            case "Egypt":
                territoryEnum = TerritoryEnum.Egypt;
                break;
            case "EastAfrica":
                territoryEnum = TerritoryEnum.EastAfrica;
                break;
            case "Congo":
                territoryEnum = TerritoryEnum.Congo;
                break;
            case "SouthAfrica":
                territoryEnum = TerritoryEnum.SouthAfrica;
                break;
            case "Madagascar":
                territoryEnum = TerritoryEnum.Madagascar;
                break;
            case "Iceland":
                territoryEnum = TerritoryEnum.Iceland;
                break;
            case "Scandanavia":
                territoryEnum = TerritoryEnum.Scandanavia;
                break;
            case "Ukraine":
                territoryEnum = TerritoryEnum.Ukraine;
                break;
            case "UnitedKingdom":
                territoryEnum = TerritoryEnum.UnitedKingdom;
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
            case "Siam":
                territoryEnum = TerritoryEnum.Siam;
                break;
            case "Siberia":
                territoryEnum = TerritoryEnum.Siberia;
                break;
            case "Yakutsk":
                territoryEnum = TerritoryEnum.Yakutsk;
                break;
            case "Kamchatka":
                territoryEnum = TerritoryEnum.Kamchatka;
                break;
            case "Japan":
                territoryEnum = TerritoryEnum.Japan;
                break;
            case "Mongolia":
                territoryEnum = TerritoryEnum.Mongolia;
                break;
            case "China":
                territoryEnum = TerritoryEnum.China;
                break;
            case "MiddleEast":
                territoryEnum = TerritoryEnum.MiddleEast;
                break;
            case "Afghanistan":
                territoryEnum = TerritoryEnum.Afghanistan;
                break;
            case "India":
                territoryEnum = TerritoryEnum.India;
                break;
            case "Ural":
                territoryEnum = TerritoryEnum.Ural;
                break;
            case "Indonesia":
                territoryEnum = TerritoryEnum.Indonesia;
                break;
        }
    }
}
