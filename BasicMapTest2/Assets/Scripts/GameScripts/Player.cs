using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int soldierCount;
    public int playerNumber;
    public bool isTurn;
    public GameObject soldierPrefab;
    public List<Territory> territories = new List<Territory>();
    public List<GameObject> soldiers = new List<GameObject>();
    

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {
        CreateSoldierIfKeyPushed();
    }

    private void CreateSoldierIfKeyPushed()
    {
        if (Input.GetKeyUp(KeyCode.N))
        {
            GenerateSoldier(GameObject.FindWithTag("Europe").transform.position); //new objects made in asia to test
        }
    }

    private void PutSoldiersOnTerritories()
    {
        throw new NotImplementedException();
    }

    private void GenerateSoldier(Vector3 position)
    {
        GameObject newSoldier = Instantiate(soldierPrefab, transform.position, Quaternion.identity);


    }

}
