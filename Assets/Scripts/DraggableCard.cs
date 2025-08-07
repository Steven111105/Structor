using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Card Settings")]
    public CardData cardData;
    
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 startPosition;
    private GridManager gridManager;
    
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        gridManager = FindObjectOfType<GridManager>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
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
        
        // Try to place the card on the grid
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPosition.z = 0;
        
        Vector2Int gridPosition = gridManager.WorldToGridPosition(worldPosition);
        
        // Try to place the object
        bool placed = TryPlaceOnGrid(gridPosition);
        
        if (!placed)
        {
            // Return to start position if placement failed
            rectTransform.anchoredPosition = startPosition;
        }
    }
    
    bool TryPlaceOnGrid(Vector2Int gridPosition)
    {
        if (cardData == null || gridManager == null) return false;
        
        // Create the grid object
        GameObject gridObj = new GameObject(cardData.cardName);
        GridObject gridObjectComponent = gridObj.AddComponent<GridObject>();
        gridObjectComponent.cardData = cardData;
        gridObjectComponent.gridPosition = gridPosition;
        
        // Try to place it on the grid
        bool success = gridManager.PlaceObject(gridPosition, gridObjectComponent, cardData.size);
        
        if (success)
        {
            // Position the object in world space
            Vector3 worldPos = gridManager.GridToWorldPosition(gridPosition);
            gridObj.transform.position = worldPos;
            
            // Add visual representation
            SetupGridObjectVisuals(gridObj);
            
            // Remove this card from hand
            Destroy(gameObject);
            
            Debug.Log($"Placed {cardData.cardName} at grid position {gridPosition}");
            return true;
        }
        else
        {
            // Placement failed, destroy the object
            Destroy(gridObj);
            Debug.Log($"Cannot place {cardData.cardName} at {gridPosition} - space occupied or invalid");
            return false;
        }
    }
    
    void SetupGridObjectVisuals(GameObject gridObj)
    {
        // Add sprite renderer for visuals
        SpriteRenderer spriteRenderer = gridObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = cardData.cardSprite;
        spriteRenderer.sortingOrder = 1;
        
        // Add collider for mouse interaction (clicking to rotate)
        BoxCollider2D collider = gridObj.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(cardData.size.x, cardData.size.y);
        
        // Scale the object based on card size
        if (cardData.size.x > 1 || cardData.size.y > 1)
        {
            gridObj.transform.localScale = new Vector3(cardData.size.x, cardData.size.y, 1);
        }
    }
}
