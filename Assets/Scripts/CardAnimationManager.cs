using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;

public class CardAnimationManager : MonoBehaviour
{
    public static CardAnimationManager instance;
    [Header("Card Hand Settings")]
    public Transform cardHandParent; // The parent container for cards
    public Transform deckPosition; // Where cards come from (deck position)
    
    [Header("Hand Layout")]
    [Range(0f, 1000f)]
    public float handWidth = 800f; // Total width of the hand area
    [Range(0f, 500f)]
    public float cardSpacing = 120f; // Spacing between cards
    [Range(0f, 100f)]
    public float maxCardOverlap = 50f; // Maximum overlap when hand is full
    
    [Header("Animation Settings")]
    [Range(0.1f, 2f)]
    public float drawAnimationDuration = 0.6f;
    [Range(0.1f, 1f)]
    public float repositionDuration = 0.3f;
    [Range(0f, 0.5f)]
    public float staggerDelay = 0.1f; // Delay between multiple card draws
    
    [Header("Card Effects")]
    [Range(0f, 360f)]
    public float drawRotationAmount = 15f;
    [Range(0.5f, 2f)]
    public float drawScaleEffect = 1.2f;
    
    private List<GameObject> cardsInHand = new List<GameObject>();
    private bool isAnimating = false;
    private bool isDrawingCard = false; // Track individual card draw state
    private bool isRepositioning = false; // Track repositioning state
    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        // Initialize if no card hand parent assigned
        if (cardHandParent == null)
        {
            cardHandParent = transform;
        }
    }

    /// <summary>
    /// Adds a card to the hand with animation
    /// </summary>
    public void DrawCard(GameObject cardObject, bool animate = true)
    {
        if (cardObject == null) return;

        // If we're currently drawing a card and this is an animated draw, wait
        if (animate && isDrawingCard)
        {
            Debug.LogWarning($"Already drawing a card, queuing {cardObject.name}");
            StartCoroutine(WaitAndDrawCard(cardObject, animate));
            return;
        }

        // Ensure the card is properly parented to the hand
        if (cardHandParent != null && cardObject.transform.parent != cardHandParent)
        {
            cardObject.transform.SetParent(cardHandParent, false);

            // Verify the cardHandParent is under a Canvas
            Canvas parentCanvas = cardHandParent.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError($"CardHandParent '{cardHandParent.name}' is not under a Canvas! Cards won't be able to find Canvas for dragging.");
            }
        }

        // Set up proper anchors and pivots for the card
        SetupCardTransform(cardObject);

        // Set initial position and properties
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        if (cardRect == null) return;

        if (animate)
        {
            isDrawingCard = true; // Mark that we're drawing a card
                                  // Debug.Log($"Starting card draw animation for {cardObject.name}. Current hand size: {cardsInHand.Count}");

            // Start from deck position
            if (deckPosition != null)
            {
                // Use a simpler approach for deck position
                cardRect.anchoredPosition = new Vector3(300f, 0f, 0f); // Right side start
                // Debug.Log($"Set {cardObject.name} start position to deck: {cardRect.anchoredPosition}");
            }
            else
            {
                cardRect.anchoredPosition = new Vector3(300f, 0f, 0f); // Default deck position
                Debug.Log($"Set {cardObject.name} start position to default deck: {cardRect.anchoredPosition}");
            }

            cardRect.localScale = Vector3.zero;
            cardRect.rotation = Quaternion.Euler(0, 0, Random.Range(-drawRotationAmount, drawRotationAmount));

            // Animate to hand position
            StartCoroutine(AnimateCardDraw(cardObject));
        }
        else
        {
            // Instant positioning
            PositionCard(cardObject, cardsInHand.Count - 1, false);
        }
        SFXManager.instance.PlaySFX("Draw");
    }
    
    /// <summary>
    /// Waits for current card animation to finish, then draws the next card
    /// </summary>
    private IEnumerator WaitAndDrawCard(GameObject cardObject, bool animate)
    {
        yield return new WaitUntil(() => !isDrawingCard);
        DrawCard(cardObject, animate);
    }
    
    /// <summary>
    /// Removes a card from the hand and repositions remaining cards
    /// </summary>
    public void RemoveCard(GameObject cardObject, bool animate = true)
    {
        if (cardObject == null) return;
        
        int cardIndex = cardsInHand.IndexOf(cardObject);
        if (cardIndex >= 0)
        {
            cardsInHand.RemoveAt(cardIndex);
            
            if (animate)
            {
                RepositionAllCards();
            }
        }
    }

    /// <summary>
    /// Clears all cards from the hand
    /// </summary>
    public void ClearHand()
    {
        // Stop any running animations
        StopAllCoroutines();

        cardsInHand.Clear();
        isAnimating = false;
        isDrawingCard = false;
    }
    
    /// <summary>
    /// Draws multiple cards with staggered animation
    /// </summary>
    public void DrawMultipleCards(List<GameObject> cards)
    {
        StartCoroutine(DrawCardsSequentially(cards));
    }
    
    private IEnumerator DrawCardsSequentially(List<GameObject> cards)
    {
        isAnimating = true;
        isRepositioning = true; // Block hover during staggered drawing
        
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                // Debug.Log($"Drawing card {i + 1}/{cards.Count}: {cards[i].name}");
                
                // Start drawing this card
                DrawCard(cards[i], true);
                
                // Wait for this card's animation to complete
                yield return new WaitUntil(() => !isDrawingCard);
                
                // Optional: Add a small delay between cards
                if (staggerDelay > 0 && i < cards.Count - 1)
                {
                    yield return new WaitForSeconds(staggerDelay);
                }
            }
        }
        
        // Wait a bit longer to ensure all positioning is settled
        yield return new WaitForSeconds(0.2f);
        
        // Update all hover effects with correct positions after all cards are drawn
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i] != null)
            {
                CardHoverEffect hoverEffect = cardsInHand[i].GetComponent<CardHoverEffect>();
                if (hoverEffect != null)
                {
                    Vector3 correctPosition = CalculateCardPosition(i);
                    hoverEffect.SetBasePosition(correctPosition);
                }
            }
        }
        
        // Debug.Log("Finished drawing all cards sequentially");
        isAnimating = false;
        isRepositioning = false; // Re-enable hover after all cards are properly positioned
    }
    
    private IEnumerator AnimateCardDraw(GameObject cardObject)
    {
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        if (cardRect == null) yield break;
        
        // Store initial values
        Vector3 startPos = new Vector3(300f, 0f, 0f); // Start from right side (deck position)
        Vector3 startScale = Vector3.zero;
        
        // Calculate target values
        int targetIndex = cardsInHand.Count; // This card will be at this index when added
        int totalCardsAfterAdd = cardsInHand.Count + 1; // Total cards after this one is added
        Vector3 targetPos = CalculateCardPosition(targetIndex, totalCardsAfterAdd);
        Vector3 targetScale = Vector3.one;
        
        // Debug.Log($"Card {cardObject.name} animating to index {targetIndex} of {totalCardsAfterAdd}, position {targetPos}");
        
        // Set initial state
        cardRect.anchoredPosition = startPos;
        cardRect.localScale = startScale;
        cardRect.rotation = Quaternion.identity; // No rotation
        
        // Trigger repositioning immediately when card spawn starts
        // Debug.Log($"Card {cardObject.name} starting animation - triggering immediate reposition of existing cards");
        
        // Add this card to tracking list for position calculations
        cardsInHand.Add(cardObject);
        
        // Reposition existing cards immediately (excluding this new one which will animate separately)
        RepositionExistingCards();

        float elapsedTime = 0f;
        
        // Simple animation - just position and scale
        while (elapsedTime < drawAnimationDuration)
        {
            float normalizedTime = elapsedTime / drawAnimationDuration;
            
            // Linear position movement
            cardRect.anchoredPosition = Vector3.Lerp(startPos, targetPos, normalizedTime);
            
            // Ease out scaling for smoother effect
            float easedScale = EaseOutQuart(normalizedTime);
            cardRect.localScale = Vector3.Lerp(startScale, targetScale, easedScale);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final values are set
        cardRect.anchoredPosition = targetPos;
        cardRect.localScale = targetScale;
        cardRect.rotation = Quaternion.identity;
        
        // Card is already in tracking list, no need to add again
        // Repositioning already happened at the start, no need to do it again
        // Debug.Log("Card animation complete - repositioning already handled at start");
        
        // Mark that this card's animation is complete
        isDrawingCard = false;
        // Debug.Log($"Card draw animation completed for {cardObject.name}");
    }
    
    private void PositionCard(GameObject cardObject, int index, bool animate = true)
    {
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        if (cardRect == null) return;

        Vector3 targetPos = CalculateCardPosition(index); // This will use current cardsInHand.Count which is correct here
        Quaternion targetRot = CalculateCardRotation(index);
        
        // Debug.Log($"Positioning card {cardObject.name} at index {index} to position {targetPos}");

        if (animate)
        {
            StartCoroutine(AnimateCardPosition(cardRect, targetPos, targetRot));
        }
        else
        {
            cardRect.anchoredPosition = targetPos;
            cardRect.rotation = targetRot;
        }
    }
    
    private IEnumerator AnimateCardPosition(RectTransform cardRect, Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startPos = cardRect.anchoredPosition;
        Quaternion startRot = cardRect.rotation;
        float elapsedTime = 0f;
        
        while (elapsedTime < repositionDuration)
        {
            float normalizedTime = elapsedTime / repositionDuration;
            float easedTime = EaseInOutQuad(normalizedTime);
            
            cardRect.anchoredPosition = Vector3.Lerp(startPos, targetPos, easedTime);
            cardRect.rotation = Quaternion.Lerp(startRot, targetRot, easedTime);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        cardRect.anchoredPosition = targetPos;
        cardRect.rotation = targetRot;
    }
    
    /// <summary>
    /// Repositions all cards in hand with animation
    /// </summary>
    public void RepositionAllCards()
    {
        // Debug.Log($"Repositioning all {cardsInHand.Count} cards");
        
        isRepositioning = true;
        StartCoroutine(RepositionCardsCoroutine());
    }
    
    private IEnumerator RepositionCardsCoroutine()
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i] != null)
            {
                PositionCard(cardsInHand[i], i, true);
            }
        }
        
        // Wait for repositioning animation to complete
        yield return new WaitForSeconds(repositionDuration + 0.1f);
        
        // NOW update hover effect base positions after repositioning is complete
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i] != null)
            {
                CardHoverEffect hoverEffect = cardsInHand[i].GetComponent<CardHoverEffect>();
                if (hoverEffect != null)
                {
                    // Force update to the correct final position
                    Vector3 correctPosition = CalculateCardPosition(i);
                    hoverEffect.SetBasePosition(correctPosition);
                }
            }
        }
        
        isRepositioning = false;
    }
    
    /// <summary>
    /// Repositions only the existing cards (not the currently animating one)
    /// </summary>
    private void RepositionExistingCards()
    {
        // Debug.Log($"Repositioning existing {cardsInHand.Count - 1} cards (excluding newest)");
        
        // Reposition all cards except the last one (which is still animating)
        for (int i = 0; i < cardsInHand.Count - 1; i++)
        {
            if (cardsInHand[i] != null)
            {
                PositionCard(cardsInHand[i], i, true);
            }
        }
    }
    
    private Vector3 CalculateCardPosition(int index, int totalCardCount = -1)
    {
        // Use provided total or default to current count
        float totalCards = totalCardCount >= 0 ? totalCardCount : cardsInHand.Count;
        
        // Debug.Log($"Calculating position for index {index} with {totalCards} total cards");
        
        if (totalCards <= 1)
        {
            // Single card should be at center
            // Debug.Log($"Single card positioning: (0, 0, 0)");
            return new Vector3(0, 0, 0);
        }
        
        // Calculate spacing based on hand width and number of cards
        float actualSpacing = Mathf.Min(cardSpacing, (handWidth - 100f) / (totalCards - 1));
        
        // Handle overlap when hand is crowded
        if (actualSpacing < maxCardOverlap)
        {
            actualSpacing = maxCardOverlap;
        }
        
        // Calculate total width of all cards
        float totalWidth = (totalCards - 1) * actualSpacing;
        
        // Center the cards: start from negative half of total width
        float startX = -(totalWidth * 0.5f);
        
        // Calculate this card's X position relative to center
        float cardX = startX + (index * actualSpacing);
        
        // No arc effect - all cards at same Y level
        Vector3 result = new Vector3(cardX, 0, 0);
        // Debug.Log($"Card {index} position calculated: {result}");
        return result;
    }
    
    private Quaternion CalculateCardRotation(int index)
    {
        if (cardsInHand.Count <= 1)
        {
            return Quaternion.identity;
        }
        
        // Slight rotation based on position in hand
        float totalCards = cardsInHand.Count;
        float normalizedPos = (float)index / (totalCards - 1);
        float rotationAngle = Mathf.Lerp(-5f, 5f, normalizedPos);
        
        return Quaternion.Euler(0, 0, rotationAngle);
    }
    
    /// <summary>
    /// Gets the calculated position for a specific card
    /// </summary>
    public Vector3 GetCardPosition(GameObject cardObject)
    {
        int cardIndex = cardsInHand.IndexOf(cardObject);
        if (cardIndex >= 0)
        {
            return CalculateCardPosition(cardIndex);
        }
        return Vector3.zero;
    }
    
    /// <summary>
    /// Gets the current number of cards in hand
    /// </summary>
    public int GetCardCount()
    {
        // Clean up any null references
        cardsInHand.RemoveAll(card => card == null);
        return cardsInHand.Count;
    }
    
    /// <summary>
    /// Checks if animation is currently playing
    /// </summary>
    public bool IsAnimating()
    {
        return isAnimating || isDrawingCard;
    }
    
    /// <summary>
    /// Checks if cards are currently being repositioned
    /// </summary>
    public bool IsRepositioning()
    {
        return isRepositioning;
    }
    
    /// <summary>
    /// Forces immediate repositioning of all cards
    /// </summary>
    public void ForceRepositionCards()
    {
        Debug.Log($"ForceRepositionCards called - {cardsInHand.Count} cards in list");
        
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i] != null)
            {
                Vector3 expectedPos = CalculateCardPosition(i);
                Debug.Log($"Positioning card {i} to {expectedPos}");
                PositionCard(cardsInHand[i], i, false);
                
                // Update hover effect base position after repositioning
                CardHoverEffect hoverEffect = cardsInHand[i].GetComponent<CardHoverEffect>();
                if (hoverEffect != null)
                {
                    hoverEffect.UpdateOriginalPosition();
                }
            }
            else
            {
                Debug.LogWarning($"Card at index {i} is null!");
            }
        }
    }
    
    /// <summary>
    /// Debug method to log card positions
    /// </summary>
    [ContextMenu("Debug Card Positions")]
    public void DebugCardPositions()
    {
        Debug.Log($"=== CARD POSITIONING DEBUG ===");
        Debug.Log($"Cards in hand: {cardsInHand.Count}");
        Debug.Log($"Hand width: {handWidth}, Card spacing: {cardSpacing}");
        
        if (cardHandParent != null)
        {
            RectTransform parentRect = cardHandParent.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                Debug.Log($"Parent anchor: {parentRect.anchorMin} - {parentRect.anchorMax}");
                Debug.Log($"Parent pivot: {parentRect.pivot}");
                Debug.Log($"Parent position: {parentRect.anchoredPosition}");
                Debug.Log($"Parent size: {parentRect.sizeDelta}");
            }
        }
        
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i] != null)
            {
                Vector3 expectedPos = CalculateCardPosition(i);
                RectTransform cardRect = cardsInHand[i].GetComponent<RectTransform>();
                if (cardRect != null)
                {
                    Vector3 actualPos = cardRect.anchoredPosition;
                    Debug.Log($"Card {i}: Expected {expectedPos}, Actual {actualPos}, Pivot: {cardRect.pivot}");
                }
            }
        }
        
        // Calculate and show total spread
        if (cardsInHand.Count > 1)
        {
            float totalCards = cardsInHand.Count;
            float actualSpacing = Mathf.Min(cardSpacing, (handWidth - 100f) / (totalCards - 1));
            if (actualSpacing < maxCardOverlap) actualSpacing = maxCardOverlap;
            float totalSpread = (totalCards - 1) * actualSpacing;
            Debug.Log($"Total spread: {totalSpread}, Actual spacing: {actualSpacing}");
        }
    }
    
    /// <summary>
    /// Highlights a card (for hover effects, etc.)
    /// </summary>
    public void HighlightCard(GameObject cardObject, bool highlight)
    {
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        if (cardRect == null) return;
        
        if (highlight)
        {
            StartCoroutine(AnimateCardHighlight(cardRect, true));
        }
        else
        {
            int cardIndex = cardsInHand.IndexOf(cardObject);
            if (cardIndex >= 0)
            {
                StartCoroutine(AnimateCardHighlight(cardRect, false, cardIndex));
            }
        }
    }
    
    private IEnumerator AnimateCardHighlight(RectTransform cardRect, bool highlight, int cardIndex = -1)
    {
        Vector3 startScale = cardRect.localScale;
        Vector3 startPos = cardRect.anchoredPosition;
        
        Vector3 targetScale = highlight ? Vector3.one * 1.1f : Vector3.one;
        
        // Calculate absolute positions to prevent drift
        Vector3 basePos = cardIndex >= 0 ? CalculateCardPosition(cardIndex) : startPos;
        Vector3 hoverPos = basePos + Vector3.up * 20f;
        Vector3 targetPos = highlight ? hoverPos : basePos;
        
        // If highlighting and current position is already higher than target hover position, clamp it
        if (highlight && startPos.y >= hoverPos.y)
        {
            targetPos = startPos; // Stay at current position
        }
        
        float duration = 0.2f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float normalizedTime = elapsedTime / duration;
            float easedTime = highlight ? EaseOutQuad(normalizedTime) : EaseInQuad(normalizedTime);
            
            cardRect.localScale = Vector3.Lerp(startScale, targetScale, easedTime);
            cardRect.anchoredPosition = Vector3.Lerp(startPos, targetPos, easedTime);
            
            elapsedTime += Time.deltaTime;
            yield return null;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        cardRect.localScale = targetScale;
        cardRect.anchoredPosition = targetPos;
    }
    
    // Easing functions
    private float EaseOutQuart(float t)
    {
        return 1f - Mathf.Pow(1f - t, 4f);
    }
    
    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }
    
    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
    
    private float EaseInQuad(float t)
    {
        return t * t;
    }
    
    /// <summary>
    /// Ensures cards have proper pivot and anchor settings for centering
    /// </summary>
    [ContextMenu("Fix Card Anchors and Pivots")]
    public void FixCardAnchorsAndPivots()
    {
        foreach (var card in cardsInHand)
        {
            if (card != null)
            {
                RectTransform cardRect = card.GetComponent<RectTransform>();
                if (cardRect != null)
                {
                    // Set pivot to center for proper rotation and scaling
                    cardRect.pivot = new Vector2(0.5f, 0.5f);
                    
                    // Set anchor to center of parent
                    cardRect.anchorMin = new Vector2(0.5f, 0.5f);
                    cardRect.anchorMax = new Vector2(0.5f, 0.5f);
                }
            }
        }
        
        // Force reposition after fixing anchors
        ForceRepositionCards();
        Debug.Log("Fixed card anchors and pivots, repositioned cards");
    }
    
    /// <summary>
    /// Sets up a new card with proper anchor and pivot settings
    /// </summary>
    public void SetupCardTransform(GameObject cardObject)
    {
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        if (cardRect != null)
        {
            // Set pivot to center
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Set anchor to center of parent for positioning relative to center
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            
            // Reset any size delta issues
            if (cardRect.sizeDelta == Vector2.zero)
            {
                cardRect.sizeDelta = new Vector2(100, 140); // Default card size
            }
        }
    }
    
    /// <summary>
    /// Scans the cardHandParent for existing cards and adds them to tracking
    /// </summary>
    [ContextMenu("Rebuild Card List")]
    public void RebuildCardList()
    {
        cardsInHand.Clear();
        
        if (cardHandParent != null)
        {
            // Find all child objects that look like cards
            for (int i = 0; i < cardHandParent.childCount; i++)
            {
                Transform child = cardHandParent.GetChild(i);
                GameObject childObj = child.gameObject;
                
                // Check if this looks like a card (has RectTransform and maybe DraggableCard component)
                RectTransform rectTransform = childObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    cardsInHand.Add(childObj);
                    SetupCardTransform(childObj); // Ensure proper setup
                    Debug.Log($"Added existing card: {childObj.name}");
                }
            }
            
            Debug.Log($"Rebuilt card list - found {cardsInHand.Count} cards");
            ForceRepositionCards();
        }
        else
        {
            Debug.LogWarning("No cardHandParent assigned!");
        }
    }
}
