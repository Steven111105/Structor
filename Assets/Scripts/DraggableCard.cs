using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Card Settings")]
    public CardData cardData;
    
    [Header("Discard System")]
    public bool markedForDiscard = false;
    
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 startPosition;
    private GridManager gridManager;
    private CardManager cardManager;
    private GameManager gameManager;

    void Start()
    {
        InitializeComponents();
    }
    
    void InitializeComponents()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        // Set the image
        if (cardData != null && cardData.cardSprite != null)
        {
            var image = GetComponent<Image>();
            if (image != null)
            {
                image.sprite = cardData.cardSprite;
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Ensure components are initialized
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
            
        if (rectTransform != null)
        {
            startPosition = rectTransform.anchoredPosition;
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError($"Missing CanvasGroup component on {gameObject.name}");
        }
        
        // Notify hover effect that dragging started
        CardHoverEffect hoverEffect = GetComponent<CardHoverEffect>();
        if (hoverEffect != null)
        {
            hoverEffect.OnDragStart();
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // Ensure components are initialized
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
            
        // Add detailed error checking
        if (rectTransform == null)
        {
            Debug.LogError($"RectTransform is null on {gameObject.name}");
            return;
        }
        
        if (canvas == null)
        {
            Debug.LogError($"Canvas is null on {gameObject.name} - card may not be properly parented to UI Canvas");
            return;
        }
        
        if (eventData == null)
        {
            Debug.LogError($"PointerEventData is null");
            return;
        }
        
        // Perform the drag operation
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Ensure components are initialized
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
            
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        // Notify hover effect that dragging ended
        CardHoverEffect hoverEffect = GetComponent<CardHoverEffect>();
        if (hoverEffect != null)
        {
            hoverEffect.OnDragEnd();
        }
        
        // Check if dropped on trash can first
        if (CheckDroppedOnTrashCan(eventData.position))
        {
            MarkForDiscard();
            return;
        }
        
        // Convert screen position to grid position
        Vector2Int gridPosition = gridManager.ScreenToGridPosition(eventData.position);
        
        // Try to place the object
        bool placed = TryPlaceOnGrid(gridPosition);
        
        if (!placed)
        {
            // Return to start position if placement failed
            rectTransform.anchoredPosition = startPosition;
        }
    }
    
    bool CheckDroppedOnTrashCan(Vector2 screenPosition)
    {
        // Find the trash can UI element by tag or name
        GameObject trashCan = GameObject.FindGameObjectWithTag("TrashCan");
        if (trashCan == null)
        {
            // Fallback: find by name
            trashCan = GameObject.Find("TrashCan");
        }
        
        if (trashCan == null) return false;
        
        // Check if the screen position is over the trash can
        RectTransform trashRect = trashCan.GetComponent<RectTransform>();
        if (trashRect != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                trashRect, screenPosition, canvas.worldCamera, out localPoint);
            
            return trashRect.rect.Contains(localPoint);
        }
        
        return false;
    }
    
    void MarkForDiscard()
    {
        // Update hand size counter (card is gone from hand)
        if (cardManager != null)
        {
            cardManager.currentHandSize--;
            cardManager.OnCardTrashed();
        }
        
        // Don't use discard or refill hand yet - wait for trash can button click
        
        Debug.Log($"{cardData.cardName} dragged to trash - card removed (discard pending)");
        
        // Trigger repositioning of remaining cards before destroying this one
        TriggerHandRepositioning();
        
        // Destroy the card immediately
        Destroy(gameObject);
    }
    
    public void UnmarkForDiscard()
    {
        markedForDiscard = false;
        
        // Restore normal appearance
        var image = GetComponent<Image>();
        if (image != null)
        {
            image.color = Color.white; // Normal color
        }
        
        // Update UI to re-enable attack button if no more marked cards
        if (gameManager != null)
        {
            gameManager.UpdateUI();
        }
        
        Debug.Log($"{cardData.cardName} unmarked for discard");
    }
    
    bool TryPlaceOnGrid(Vector2Int gridPosition)
    {
        if (cardData == null || gridManager == null) return false;
        
        // Check if this is a booster card
        if (cardData.cardType == CardType.Booster)
        {
            return TryApplyBooster(gridPosition);
        }
        
        // Regular tile placement
        return PlaceRegularTile(gridPosition);
    }
    
    bool TryApplyBooster(Vector2Int gridPosition)
    {
        // Check if there's already a wire at this position
        var existingWire = gridManager.GetGridObject(gridPosition);
        if (existingWire == null)
        {
            Debug.Log("Cannot apply booster - no wire at this position");
            return false;
        }
        
        // Check if it's a valid wire type (not sensor, not already boosted)
        if (existingWire.CardData.cardType == CardType.Sensor)
        {
            Debug.Log("Cannot apply booster to sensors");
            return false;
        }
        
        // Apply the booster effect
        ApplyBoosterToWire(existingWire);
        
        // Update hand size counter
        if (cardManager != null)
        {
            cardManager.currentHandSize--;
        }
        
        // Trigger repositioning of remaining cards before destroying this one
        TriggerHandRepositioning();
        
        // Remove this card from hand
        Destroy(gameObject);
        
        Debug.Log($"Applied {cardData.cardName} booster to wire at {gridPosition}");
        return true;
    }
    
    void ApplyBoosterToWire(GridObject wire)
    {
        // Don't modify the ScriptableObject! Store booster effects on the GridObject instead
        if (cardData.isAdditiveBoost)
        {
            // Additive: +X damage (e.g., +2 damage)
            wire.damageBoostMultiplier += cardData.damageMultiplier - 1f;
        }
        else
        {
            // Multiplicative: xX damage (e.g., x2 damage)
            wire.damageBoostMultiplier *= cardData.damageMultiplier;
        }
        
        wire.isBoosted = true;
        
        // Add visual indicator
        AddBoosterVisualEffect(wire.gameObject);
    }
    
    void AddBoosterVisualEffect(GameObject wireObject)
    {
        // Add a glowing effect or color change to show it's boosted
        var spriteRenderer = wireObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow; // Or any other visual indicator
        }
        
        // Could also add a particle system or other effects here
    }
    
    /// <summary>
    /// Triggers repositioning of remaining cards in hand after this card is removed
    /// </summary>
    void TriggerHandRepositioning()
    {
        Debug.Log($"TriggerHandRepositioning called for card {gameObject.name}");
        
        // Find the CardAnimationManager to trigger repositioning
        CardAnimationManager animationManager = FindObjectOfType<CardAnimationManager>();
        if (animationManager != null)
        {
            Debug.Log("Found CardAnimationManager, removing card and repositioning");
            
            // Remove this card from the animation manager's tracking before repositioning
            animationManager.RemoveCard(gameObject, false); // Don't animate removal since we're destroying it
            
            // Use the animation manager to start the coroutine since this card will be destroyed
            animationManager.StartCoroutine(DelayedRepositioning(animationManager));
        }
        else
        {
            Debug.LogError("CardAnimationManager not found! Cannot reposition cards.");
        }
    }
    
    /// <summary>
    /// Delays repositioning slightly to ensure this card is fully removed first
    /// </summary>
    System.Collections.IEnumerator DelayedRepositioning(CardAnimationManager animationManager)
    {
        Debug.Log("DelayedRepositioning coroutine started");
        yield return new WaitForEndOfFrame(); // Wait one frame for destroy to process
        
        if (animationManager != null)
        {
            Debug.Log("Calling RepositionAllCards");
            animationManager.RepositionAllCards();
        }
        else
        {
            Debug.LogError("AnimationManager became null in DelayedRepositioning");
        }
    }
    
    bool PlaceRegularTile(Vector2Int gridPosition)
    {
        // Create the grid object using GridManager's method
        var gridObject = gridManager.CreateGridObject(cardData.cardName, cardData.cardType, gridPosition);
        
        if (gridObject != null)
        {
            Debug.Log($"PlaceRegularTile successful for {cardData.cardName}");
            
            // Update hand size counter
            if (cardManager != null)
            {
                cardManager.currentHandSize--;
            }
            
            // Trigger repositioning of remaining cards before destroying this one
            TriggerHandRepositioning();
            
            // Remove this card from hand
            Destroy(gameObject);
            Debug.Log($"Placed {cardData.cardName} at grid position {gridPosition}");
            return true;
        }
        else
        {
            Debug.Log($"Cannot place {cardData.cardName} at {gridPosition} - space occupied or invalid");
            return false;
        }
    }
}
