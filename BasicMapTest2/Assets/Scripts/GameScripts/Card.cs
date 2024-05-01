/// <summary>
/// Define a card class, which should contain all of the fields of the RISK card.
/// </summary>
public class Card
{
    public string territory_id = "WILD";
    public string troop_type = "WILD";
    public string status = "IN_DECK";

    public Card(){}

    public Card(string territory_id, string troop_type, string status){
        this.territory_id = territory_id;
        this.troop_type = troop_type;
        this.status = status;
    }
    
}