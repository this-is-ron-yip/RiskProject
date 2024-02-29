using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Territory : MonoBehaviour
{
    public string name { get; set; }
    public int troopsCount { get; set; }
    public string occupiedBy;
    [field: SerializeField] public List<Transform> adjacentCountries { get; set; }

    private void OnValidate()
    {
        FillAdjCountriesList();

        //RunTestCode();
    }


    private void RunTestCode()
    {
        //debugging
        if(gameObject.tag == "Oceania")
        {
        Debug.Log(GameObject.FindWithTag("Oceania").GetComponent<Territory>().adjacentCountries[0].gameObject.tag);
        Debug.Log(GameObject.FindWithTag("Oceania").GetComponent<Territory>().adjacentCountries[1].gameObject.tag);
        }
    }

    private void FillAdjCountriesList()
    {
        adjacentCountries.Clear();
        int count = 0;
        switch (this.gameObject.tag)
        {
            case "NorthAmerica":
                adjacentCountries.Add(GameObject.FindWithTag("SouthAmerica").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Europe").GetComponent<Transform>());
                break;
            case "SouthAmerica":
                adjacentCountries.Add(GameObject.FindWithTag("NorthAmerica").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Africa").GetComponent<Transform>());
                break;
            case "Europe":
                adjacentCountries.Add(GameObject.FindWithTag("Africa").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Asia").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("NorthAmerica").GetComponent<Transform>());
                break;
            case "Africa":
                adjacentCountries.Add(GameObject.FindWithTag("SouthAmerica").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Europe").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Oceania").GetComponent<Transform>());
                break;
            case "Asia":
                adjacentCountries.Add(GameObject.FindWithTag("Oceania").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Europe").GetComponent<Transform>());
                break;
            case "Oceania":
                adjacentCountries.Add(GameObject.FindWithTag("Africa").GetComponent<Transform>());
                adjacentCountries.Add(GameObject.FindWithTag("Asia").GetComponent<Transform>());
                break;
        }
    }

}
