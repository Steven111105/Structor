using UnityEngine;

[CreateAssetMenu(fileName = "New Deck Configuration", menuName = "Card Game/Deck Configuration")]
public class DeckConfiguration : ScriptableObject
{
    [System.Serializable]
    public class CardEntry
    {
        public CardData cardData;
        [Min(1)]
        public int startingQuantity = 1;
    }
    
    [Header("Deck Composition")]
    public CardEntry[] cardEntries;
    
    [Header("Deck Info")]
    [TextArea(2, 4)]
    public string deckDescription = "Default deck configuration";
    
    // Validate the deck configuration
    void OnValidate()
    {
        if (cardEntries != null)
        {
            foreach (var entry in cardEntries)
            {
                if (entry.startingQuantity < 1)
                {
                    entry.startingQuantity = 1;
                }
            }
        }
    }
    
    // Helper method to get total cards in deck
    public int GetTotalCardCount()
    {
        int total = 0;
        if (cardEntries != null)
        {
            foreach (var entry in cardEntries)
            {
                if (entry.cardData != null)
                {
                    total += entry.startingQuantity;
                }
            }
        }
        return total;
    }
    
    // Method to build a fresh deck list from this configuration
    public System.Collections.Generic.List<CardData> BuildDeck()
    {
        var deck = new System.Collections.Generic.List<CardData>();
        
        if (cardEntries != null)
        {
            foreach (var entry in cardEntries)
            {
                if (entry.cardData != null)
                {
                    for (int i = 0; i < entry.startingQuantity; i++)
                    {
                        deck.Add(entry.cardData);
                    }
                }
            }
        }
        
        return deck;
    }
    
    // Method to get a summary of the deck composition
    public string GetDeckSummary()
    {
        if (cardEntries == null || cardEntries.Length == 0)
        {
            return "Empty deck";
        }
        
        var summary = new System.Text.StringBuilder();
        summary.AppendLine($"Deck: {GetTotalCardCount()} total cards");
        
        foreach (var entry in cardEntries)
        {
            if (entry.cardData != null)
            {
                summary.AppendLine($"- {entry.startingQuantity}x {entry.cardData.cardName}");
            }
        }
        
        return summary.ToString();
    }
}
