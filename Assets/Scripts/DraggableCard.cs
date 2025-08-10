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
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        gridManager = FindObjectOfType<GridManager>();
        cardManager = FindObjectOfType<CardManager>();
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
        startPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
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
    
    bool PlaceRegularTile(Vector2Int gridPosition)
    {
        // Create the grid object using GridManager's method
        var gridObject = gridManager.CreateGridObject(cardData.cardName, cardData.cardType, gridPosition);
        
        if (gridObject != null)
        {
            // Update hand size counter
            if (cardManager != null)
            {
                cardManager.currentHandSize--;
            }
            
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
