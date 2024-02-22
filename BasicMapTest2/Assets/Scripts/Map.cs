using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class Map : MonoBehaviour
{
    public Transform[] childObjects;
    public int playerTurn; 
    public int playerCount = 2;
    public List<Player> players = new List<Player>();
    public List<Transform> territories = new List<Transform>();
    public Dictionary<Transform, List<Transform>> adjacencyList = new Dictionary<Transform, List<Transform>>();

    
    private void OnValidate()
    {
        playerTurn = 1; //start with player 1

        //functions to fill up the fields with appropriate data
        FillTerritoriesList();
        FillAdjacencyList();
        FillPlayersList();

        //RunTestCode(); //testing if the adjacency list stores the right contents
    }

    private void FillPlayersList()
    {
        players.Clear();
        players = GameObject.FindObjectsOfType<Player>().ToList();        
    }

    public List<Transform> GetAdjacentTerritories(Transform territory)
    {
        List<Transform> listOfAdjTerrs = new List<Transform>();
        foreach (var pair in adjacencyList)
        {
            if (pair.Key.gameObject.tag == territory.gameObject.tag)
            {
                listOfAdjTerrs = adjacencyList[pair.Key];
            }
        }
        listOfAdjTerrs.Add(null);
        return listOfAdjTerrs;
    }

    private void RunTestCode()
    {
        //just debugging
        int count = 0;
        foreach (var pair in adjacencyList)
        {
            Debug.Log("Territory: " + pair.Key.gameObject.tag);

            foreach (var adjTerr in pair.Value)
            {
                Debug.Log("AdjacentTerritory: " + adjTerr.gameObject.tag);
            }

        }
    }

    public void OnDrawGizmos()
    {
        //using this function because it executes for every frame inside the unity editor.
        //this function will draw the lines between the countries (which will indicate the routes the piece will take) using the adjacency list
        Gizmos.color = Color.green;

        foreach (var pair in adjacencyList)
        {
            Vector3 startPos = pair.Key.position;

            foreach (var adjTerr in pair.Value)
            {
                Vector3 endPos = adjTerr.position;
                Gizmos.DrawLine(startPos, endPos);
            }
        }

    }

    private void FillAdjacencyList()
    {

        adjacencyList.Clear();
        foreach (Transform territory in territories)
        {
            adjacencyList.Add(territory, GameObject.FindWithTag(territory.gameObject.tag).GetComponent<Territory>().adjacentCountries);
            //Debug.Log("Adjacents to " + territory.gameObject.tag + ": " + adjacencyList[territory][0].gameObject.tag + " " + adjacencyList[territory][1].gameObject.tag);
        }

    }

    private void FillTerritoriesList()
    {
        territories.Clear();
        childObjects = GetComponentsInChildren<Transform>();

        foreach (Transform territory in childObjects)
        {
            if (territory != this.transform)
            {
                territories.Add(territory);
            }
        }
    }
}
