using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Define a DeckScript that will persist throughout the game.
/// </summary>
public class DeckScript : MonoBehaviour
{
    private Queue<Card> deck = new Queue<Card>(); // the actual deck of cards
    public List<Card> discard = new List<Card>(); // tbe discard pile, to be recycled

    // An array of all the territories and troop types. To be used when creating the deck.
    public string[] all_territories = {"Alaska", "NorthWestTerritory", "NorthEastTerritory", 
    "Greenland", "Qubec", "Alberta", "EastUSA", "WestUSA", "CentralAmerica", 
    "Venezuela", "Peru", "Brazil", "Argentina", "NorthAfrica", "Egypt", "Congo", "EastAfrica", 
    "SouthAfrica", "Madagascar", "Iceland", "UnitedKingdom", "Scandanavia", "Ukraine", 
    "WestEurope", "NorthEurope", "SouthEurope", "Ural", "Siberia", "Yakutsk", 
    "Kamchatka", "Mongolia", "China", "Japan", "Afghanistan", "MiddleEast", 
    "Siam", "Indonesia", "India", "NewGuinea", "WestAustralia", "EastAustralia", "NewZealand"}; 
    public string[] troop_types = {
        "Infantry", "Calvalry", "Cannon",
    };

    /// <summary>
    /// Initialize the deck
    /// </summary>
    void Start()
    {
       // initialize the deck
       InitializeDeck();
    }

    /// <summary>
    /// Returns the card at the top of the deck, and removes it from the deck. 
    /// If the deck is empty, it reshuffles the discard adn draws from there.
    /// </summary>
    /// <returns></returns>
    public Card DrawCard(){
        if (deck.Count != 0)
        {
            // draw the card
            Card result = deck.Dequeue();

            Debug.Log($"Player drew the " + result.territory_id + "-" + result.troop_type + " card");
            GameObject.FindWithTag("GameHUD").GetComponent<GameHUDScript>().infoCardTMP.text = $"Player drew the " + result.territory_id + "-" + result.troop_type + " card";
            return result;
        }
        else
        {
            Debug.Log("The deck is empty. Reshuffling...");
            RecycleDiscard();
            // draw the card
            Card result = deck.Dequeue();

            Debug.Log($"Player drew the " + result.territory_id + "-" + result.troop_type + " card");
            GameObject.FindWithTag("GameHUD").GetComponent<GameHUDScript>().infoCardTMP.text = $"Player drew the " + result.territory_id + "-" + result.troop_type + " card";
            return result;
        }
    }

    /// <summary>
    /// Initialize the deck by getting the unshuffled deck, and shuffling it.
    /// </summary>
    public void InitializeDeck(){
        Debug.Log("Initializeing deck");
        // create unshuffled deck
        List<Card> unshuffled_deck = GetUnshuffledDeck();

        // Shuffle:
        for(int i = unshuffled_deck.Count - 1; i >= 0; i--){
            // get a random card from the list, and remove it from the unshuffled deck
            Card next = unshuffled_deck.ElementAt(Random.Range(0, i + 1));
            unshuffled_deck.Remove(next);
            // add it to the front of the queue
            deck.Enqueue(next);
        }
    }

    /// <summary>
    /// If the deck is empty, call this function to create a shuffled deck from
    /// the cards in the discard pile. Clears the discard pile after the new deck is complete.
    /// </summary>
    public void RecycleDiscard(){
        Debug.Log("Shuffling discard pile");

        // Shuffle:
        for(int i = discard.Count - 1; i >= 0; i--){
            // get a random card from the list, and remove it from the unshuffled deck
            Card next = discard.ElementAt(Random.Range(0, i + 1));
            discard.Remove(next);

            // add it to the front of the queue
            deck.Enqueue(next);
        }

        // clear discard
        discard.Clear();
    }

    /// <summary>
    /// Returns an unshuffled deck with 44 cards — two wild cards, and another card per territory.
    /// </summary>
    /// <returns></returns>
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
} 