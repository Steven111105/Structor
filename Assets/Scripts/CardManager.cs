using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;
    [Header("Card Setup")]
    public Transform cardHandParent; // Assign the CardHand panel
    public GameObject cardPrefab; // Assign your card prefab

    [Header("Deck Configuration")]
    public DeckConfiguration deckConfig; // Assign your deck configuration SO

    [Header("Runtime Deck")]
    [SerializeField] private List<CardData> runtimeDeck = new List<CardData>();
    [SerializeField] private List<CardData> currentDeckComposition = new List<CardData>(); // The evolved deck with shop purchases

    [Header("Deck Status")]
    [SerializeField] private int currentDeckSize = 0;

    [Header("Hand Management")]
    public int currentHandSize = 0;
    public bool hasPendingDiscards = false; // Simple flag for pending discards
    public int cardDiscarded = 0;

    void Awake()
    {
        instance = this;
    }

    public void InitializeDeck()
    {
        runtimeDeck.Clear();
        currentDeckComposition.Clear();
        deckConfig = SelectedDeckData.instance.selectedDeck;

        LoadOriginalDeck();
    }

    void ShuffleDeck()
    {
        // Fisher-Yates shuffle algorithm
        for (int i = 0; i < runtimeDeck.Count; i++)
        {
            CardData temp = runtimeDeck[i];
            int randomIndex = Random.Range(i, runtimeDeck.Count);
            runtimeDeck[i] = runtimeDeck[randomIndex];
            runtimeDeck[randomIndex] = temp;
        }

        // Debug.Log($"Deck shuffled - {runtimeDeck.Count} cards total");
        currentDeckSize = runtimeDeck.Count;
    }

    // Method to refill the deck from the current deck composition (with shop purchases)
    public void RefillDeck()
    {
        if (currentDeckComposition.Count > 0)
        {
            Debug.Log("Resetting deck to current composition...");

            runtimeDeck.Clear();
            runtimeDeck = new List<CardData>(currentDeckComposition);
            ShuffleDeck();

            Debug.Log($"Deck reset! Contains {runtimeDeck.Count} cards");
        }
        else
        {
            Debug.LogWarning("Cannot reset deck - no deck composition available!");
        }
    }

    void LoadCurrentDeckComposition()
    {
        if (currentDeckComposition.Count > 0)
        {
            Debug.Log("Resetting deck to current composition...");

            runtimeDeck.Clear();
            runtimeDeck = new List<CardData>(currentDeckComposition);
            ShuffleDeck();

            Debug.Log($"Deck reset! Contains {runtimeDeck.Count} cards");
        }
        else
        {
            Debug.LogWarning("Cannot reset deck - no deck composition available!");
        }
    }

    // Method to reset deck to original configuration (for new game)
    public void LoadOriginalDeck()
    {
        if (deckConfig != null)
        {
            Debug.Log("Resetting deck to original configuration (new game)...");

            runtimeDeck.Clear();
            currentDeckComposition.Clear();

            runtimeDeck = deckConfig.BuildDeck();
            currentDeckComposition = new List<CardData>(runtimeDeck);
            ShuffleDeck();

            Debug.Log($"Deck reset to original! Contains {runtimeDeck.Count} cards");
        }
        else
        {
            Debug.LogWarning("Cannot reset deck - no original configuration available!");
        }
    }


    public void RefillHandToMaxSize(int maxHandSize)
    {
        // Ensure deck is initialized if this is called before Start()
        if (runtimeDeck.Count == 0 && (deckConfig != null || currentDeckComposition.Count == 0))
        {
            // Debug.Log("[CardManager] Deck not initialized yet, initializing now...");
            RefillDeck();
        }

        if (cardHandParent == null)
        {
            Debug.LogError("[CardManager] cardHandParent is null! Please assign the CardHand panel in the inspector.");
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("[CardManager] cardPrefab is null! Please assign your card prefab in the inspector.");
            return;
        }

        // Count current cards in hand
        Debug.Log($"Current child in hand = {cardHandParent.childCount}");
        currentHandSize = cardHandParent.childCount;
        Debug.Log($"[CardManager] Current hand size: {currentHandSize}, target: {maxHandSize}");

        // Collect cards to draw for batch animation
        List<CardData> cardsToCreate = new List<CardData>();

        // Add cards until we reach max hand size or run out of cards
        while (currentHandSize < maxHandSize)
        {
            // Draw the top card from the deck
            CardData drawnCard = DrawCardFromDeck();
            if (runtimeDeck.Count == 0)
            {
                Debug.Log("Deck is now empty after drawing cards.");
                // Optionally auto-refill deck here if desired
                RefillDeck();
            }

            if (drawnCard != null)
            {
                cardsToCreate.Add(drawnCard);
                currentHandSize++;
                // Debug.Log($"[CardManager] Prepared card: {drawnCard.cardName}, hand size will be: {currentHandSize}");
            }
            else
            {
                Debug.LogWarning("[CardManager] No more cards available in deck!");
                break;
            }
        }

        // Create and animate cards one by one if animation manager is available
        if (cardsToCreate.Count > 0)
        {
            List<GameObject> newCards = new List<GameObject>();

            // Create cards but don't position them yet - let animation manager handle it
            foreach (var cardData in cardsToCreate)
            {
                GameObject newCard = CreateCardForAnimation(cardData);
                newCards.Add(newCard);
            }

            CardAnimationManager.instance.DrawMultipleCards(newCards);
        }

        // Debug.Log($"[CardManager] Hand refilled to {currentHandSize}/{maxHandSize} cards. {runtimeDeck.Count} cards remaining in deck.");
    }

    GameObject CreateCardForAnimation(CardData cardData)
    {
        // Instantiate the card prefab but don't let it auto-position
        GameObject newCard = Instantiate(cardPrefab);

        // Don't parent it yet - the animation manager will handle positioning
        // We'll parent it properly when the animation starts

        // Ensure required components exist
        if (newCard.GetComponent<CanvasGroup>() == null)
        {
            newCard.AddComponent<CanvasGroup>();
            // Debug.Log($"Added missing CanvasGroup to card {newCard.name}");
        }

        // Add CardHoverEffect component if missing
        if (newCard.GetComponent<CardHoverEffect>() == null)
        {
            newCard.AddComponent<CardHoverEffect>();
            // Debug.Log($"Added CardHoverEffect to card {newCard.name}");
        }

        // Set up the DraggableCard component
        DraggableCard draggableCard = newCard.GetComponent<DraggableCard>();
        if (draggableCard != null)
        {
            draggableCard.cardData = cardData;
        }

        // Update visual elements
        UpdateCardVisuals(newCard, cardData);

        // Make sure it starts hidden/small
        RectTransform cardRect = newCard.GetComponent<RectTransform>();
        if (cardRect != null)
        {
            cardRect.localScale = Vector3.zero;
        }

        return newCard;
    }

    CardData DrawCardFromDeck()
    {
        if (runtimeDeck.Count == 0)
        {
            if (currentDeckComposition.Count > 0)
            {
                Debug.Log("Deck is empty - auto-refilling from current deck composition...");
                RefillDeck();

                if (runtimeDeck.Count == 0)
                {
                    Debug.LogError("Failed to refill deck!");
                    return null;
                }
            }
            else
            {
                Debug.LogWarning("Deck is empty and auto-refill is disabled or no deck composition available!");
                return null;
            }
        }

        // Take the top card
        CardData drawnCard = runtimeDeck[0];
        runtimeDeck.RemoveAt(0);
        currentDeckSize = runtimeDeck.Count;

        // Debug.Log($"Drew {drawnCard.cardName}. {currentDeckSize} cards remaining.");

        return drawnCard;
    }

    // Method to add cards back to the deck (for power-ups, rewards, etc.)
    public void AddCardToDeck(CardData cardData)
    {
        currentDeckComposition.Add(cardData);
        currentDeckSize = currentDeckComposition.Count;
        Debug.Log($"Added {cardData.cardName} to deck. Deck now has {currentDeckSize} cards.");
    }

    // Method to add multiple cards to the deck
    public void AddCardsToDeck(CardData cardData, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            runtimeDeck.Add(cardData);
        }
        currentDeckSize = runtimeDeck.Count;
        Debug.Log($"Added {quantity}x {cardData.cardName} to deck. Deck now has {currentDeckSize} cards.");
    }

    // SHOP METHODS - These permanently modify the deck composition
    public void PurchaseCard(CardData cardData)
    {
        // Add to current deck composition (permanent)
        currentDeckComposition.Add(cardData);
        // Also add to runtime deck for immediate use
        runtimeDeck.Add(cardData);
        currentDeckSize = runtimeDeck.Count;

        Debug.Log($"Purchased {cardData.cardName}! Added to permanent deck composition. Runtime deck now has {currentDeckSize} cards.");
    }

    public void PurchaseCards(CardData cardData, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            currentDeckComposition.Add(cardData);
            runtimeDeck.Add(cardData);
        }
        currentDeckSize = runtimeDeck.Count;

        Debug.Log($"Purchased {quantity}x {cardData.cardName}! Added to permanent deck composition. Runtime deck now has {currentDeckSize} cards.");
    }

    // Method to get current deck size
    public int GetDeckSize()
    {
        return currentDeckSize;
    }

    GameObject CreateCard(CardData cardData)
    {
        // Instantiate the card prefab
        GameObject newCard = Instantiate(cardPrefab, cardHandParent);

        // Set up the DraggableCard component
        DraggableCard draggableCard = newCard.GetComponent<DraggableCard>();
        if (draggableCard != null)
        {
            draggableCard.cardData = cardData;
        }

        // Update visual elements
        UpdateCardVisuals(newCard, cardData);

        // Use animation manager if available
        var animationManager = FindObjectOfType<CardAnimationManager>();
        if (animationManager != null)
        {
            animationManager.DrawCard(newCard, true);
        }

        return newCard;
    }

    void UpdateCardVisuals(GameObject card, CardData cardData)
    {
        // Update card name
        var cardNameText = card.transform.Find("CardName")?.GetComponent<UnityEngine.UI.Text>();
        if (cardNameText != null)
        {
            cardNameText.text = cardData.cardName;
        }

        // Update card icon
        var cardIcon = card.transform.Find("CardIcon")?.GetComponent<UnityEngine.UI.Image>();
        if (cardIcon != null && cardData.cardSprite != null)
        {
            cardIcon.sprite = cardData.cardSprite;
        }
    }

    // Call this to add a new card to the hand
    public void AddCardToHand(CardData cardData)
    {
        CreateCard(cardData);
    }

    // Call this to remove all cards and reset the hand
    public void ClearHand()
    {
        // Use animation manager if available
        CardAnimationManager.instance.ClearHand();
        // dont question it, it works cuz somehow foreach didnt destroy them all
        foreach (Transform child in cardHandParent)
        {
            DestroyImmediate(child.gameObject);
        }
        foreach (Transform child in cardHandParent)
        {
            DestroyImmediate(child.gameObject);
        }
        foreach (Transform child in cardHandParent)
        {
            DestroyImmediate(child.gameObject);
        }
        foreach (Transform child in cardHandParent)
        {
            DestroyImmediate(child.gameObject);
        }
        foreach (Transform child in cardHandParent)
        {
            DestroyImmediate(child.gameObject);
        }
        foreach (Transform child in cardHandParent)
        {
            DestroyImmediate(child.gameObject);
        }
        Debug.Log($"Hand cleared, child in parent {cardHandParent.childCount}");
        currentHandSize = 0;
    }

    // DISCARD SYSTEM METHODS
    public void OnCardTrashed()
    {
        hasPendingDiscards = true;
        cardDiscarded++;
        // Debug.Log($"Card trashed. Pending discards flag: {hasPendingDiscards}");
    }

    public void ConfirmDiscards()
    {
        if (!hasPendingDiscards)
        {
            Debug.Log("No cards pending discard");
            return;
        }

        List<CardData> drawnCards = new List<CardData>();

        for (int i = 0; i < cardDiscarded; i++)
        {
            drawnCards.Add(DrawCardFromDeck());
        }

        // Process drawn cards as needed
        if (drawnCards.Count > 0)
        {
            List<GameObject> newCards = new List<GameObject>();

            // Create cards but don't position them yet - let animation manager handle it
            foreach (var cardData in drawnCards)
            {
                GameObject newCard = CreateCardForAnimation(cardData);
                newCards.Add(newCard);
            }

            CardAnimationManager.instance.DrawMultipleCards(newCards);
        }


        // Debug.Log($"Confirming discard of trashed cards");

        // Reset flag
        hasPendingDiscards = false;
        cardDiscarded = 0;

        // Tell GameManager to handle discard logic (counts as 1 discard action)
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnCardsDiscarded();
        }
    }

    public void CancelDiscards()
    {
        // Can't cancel since cards are already destroyed - just reset flag
        if (hasPendingDiscards)
        {
            Debug.Log($"Cancelled pending discards - but cards are already gone!");
            hasPendingDiscards = false;
        }
    }

    public int GetMarkedForDiscardCount()
    {
        // Return 1 if pending, 0 if not (for UI compatibility)
        return hasPendingDiscards ? 1 : 0;
    }

    public void ResetCardsAndDeck()
    {
        ClearHand();
        LoadCurrentDeckComposition();
    }

    void OnDestroy()
    {
        instance = null;
    }
}
