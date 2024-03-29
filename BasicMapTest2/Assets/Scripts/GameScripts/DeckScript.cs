using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;


public class DeckScript : MonoBehaviour
{
    private Queue<Card> deck = new Queue<Card>(); // the actual deck of cards

    // TODO: change back to false when your'e done testing
    public bool canDraw = true; // Flag to control when rolling is allowed

    // Start is called before the first frame update
    void Start()
    {
       // initialize the deck
       InitializeDeck();
    }

    // Update is called once per frame
    /* void Update()
    {
    } */

    // Returns null if the action is not allowed or if the deck is empty
    public Card DrawCard(){
         // if the deck is empty, return error message
         // otherwise, return card? figure this out.
        if(!canDraw)
        {
            Debug.Log("Not allowed to draw card.");
        }
        else if (deck.Count != 0)
        {
            // draw the card
            Card result = deck.Dequeue();

            Debug.Log($"Player drew the " + result.territory_id + "-" + result.troop_type + " card");
            return result;
        }
        else
        {
            Debug.Log("The deck is empty. Can not draw card.");
        }
        return null;
    }

    // instantiate unshuffled deck
    public void InitializeDeck(){
        Debug.Log("Initializeing deck");
        // create unshuffled deck
        List<Card> unshuffled_deck = GetUnshuffledDeck();

        // Shuffle:
        for(int i = unshuffled_deck.Count - 1; i >= 0; i--){
            // get a random card from the list, and remove it from the unshuffled deck
            Card next = unshuffled_deck.ElementAt(UnityEngine.Random.Range(0, i + 1));
            unshuffled_deck.Remove(next);

            // add it to the front of the queue
            deck.Enqueue(next);
        }
    }

    public List<Card> GetUnshuffledDeck(){
        List<Card> unshuffled_deck = new List<Card>();
   
        // Starting off with 8 cards for now. Change later.
        for(int i = 0; i < 8; i++){
            Card card = new(territories_in_order[i], troop_types_in_order[i], "IN_DECK");
            unshuffled_deck.Add(card);
        }
        return unshuffled_deck;
    }

    // Method to allow dice rolling, could be called with a player number if needed
    public void AllowDraw(int playerNumber) // default parameter in case you don't want to specify
    {
        canDraw = true;
    }

    // Method to prevent dice rolling, if needed
    public void PreventRoll()
    {
        canDraw = false;
    }

    // Used to create deck. These two arrays should be the same size as the deck.
    public string[] territories_in_order = {
        "North America", "South America", "Europe", "Asia", "Africa", "Australia",
            "Antarctica", "Wild"
    };
    public string[] troop_types_in_order = {
        "Infantry", "Calvalry", "Cannon", "Infantry", "Calvalry", "Cannon", "Infantry", "Wild"
    };
} 