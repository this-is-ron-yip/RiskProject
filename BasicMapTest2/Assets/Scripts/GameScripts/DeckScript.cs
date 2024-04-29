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
    public List<Card> discard = new List<Card>(); // discard, to be recycled

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
         // otherwise, return card? TODO: figure this out.
        if (deck.Count != 0)
        {
            // draw the card
            Card result = deck.Dequeue();

            Debug.Log($"Player drew the " + result.territory_id + "-" + result.troop_type + " card");
            return result;
        }
        else
        {
            Debug.Log("The deck is empty. Reshuffling...");
            RecycleDiscard();
            // draw the card
            Card result = deck.Dequeue();

            Debug.Log($"Player drew the " + result.territory_id + "-" + result.troop_type + " card");
            return result;

        }
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

    public void RecycleDiscard(){
        Debug.Log("Shuffling discard pile");

        // Shuffle:
        for(int i = discard.Count - 1; i >= 0; i--){
            // get a random card from the list, and remove it from the unshuffled deck
            Card next = discard.ElementAt(UnityEngine.Random.Range(0, i + 1));
            discard.Remove(next);

            // add it to the front of the queue
            deck.Enqueue(next);
        }

        // clear discard
        discard.Clear();
    }

    public List<Card> GetUnshuffledDeck(){
        List<Card> unshuffled_deck = new List<Card>();
   
        // Note that the game description never specifies which troop type must match to which territory
        // So we can assign them arbitrarily.
        for(int i = 0; i < all_territories.Count(); i++){
            Card card = new(all_territories[i], troop_types[i % 3], "IN_DECK");
            unshuffled_deck.Add(card);
        }
        unshuffled_deck.Add(new Card("WILD", "WILD", "WILD"));
        unshuffled_deck.Add(new Card("WILD", "WILD", "WILD")); // Add two wild cards
        return unshuffled_deck;
    }

    // Used to create deck. These two arrays should be the same size as the deck.
    // TODO: update the list to have all 42
    public string[] all_territories = {"Canada", "EastAmerica", "WestAmerica", "Brazil",
        "Argentina", "UnitedKingdom", "Peru",  "WestEurope", "NorthEurope",  "SouthEurope", "Egypt",
         "EastAfrica", "NorthAfrica", "SouthAfrica", "WestAustralia", "EastAustralia", "India", "MiddleEast",
         "Kazakhstan", "China", "Russia"
    };
    public string[] troop_types = {
        "Infantry", "Calvalry", "Cannon",
    };
} 