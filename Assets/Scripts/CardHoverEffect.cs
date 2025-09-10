using UnityEngine;
using UnityEngine.EventSystems;

// Add this component to card prefabs to enable hover animations
public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    [Range(0.1f, 2f)]
    public float hoverScale = 1.1f;
    [Range(0f, 100f)]
    public float hoverHeight = 20f;
    [Range(0.1f, 1f)]
    public float animationSpeed = 0.2f;
    
    private CardAnimationManager animationManager;
    private bool isBeingDragged = false;
    private Coroutine currentHoverCoroutine;
    private Vector3 basePosition; // The card's base position
    private Vector3 hoverPosition; // The card's hover position (base + height)
    
    // Static flag to track if any card is being dragged globally
    private static bool anyCardBeingDragged = false;
    
    void Start()
    {
        animationManager = FindObjectOfType<CardAnimationManager>();
        
        // Store the base position and calculate hover position
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            basePosition = rectTransform.anchoredPosition;
            hoverPosition = basePosition + Vector3.up * hoverHeight;
        }
        
        // Check if this card has a DraggableCard component
        var draggableCard = GetComponent<DraggableCard>();
        if (draggableCard != null)
        {
            // We could add events here to detect when dragging starts/stops
            // For now, we'll use a simple approach
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isBeingDragged) return;
        
        // Don't hover if any card is being dragged (including this one)
        if (anyCardBeingDragged) return;
        
        // Don't hover if cards are being repositioned by animation manager
        if (animationManager != null && animationManager.IsRepositioning()) return;
        
        
        // Stop any existing hover animation first
        if (currentHoverCoroutine != null)
        {
            StopCoroutine(currentHoverCoroutine);
        }
        
        // Always get the correct base position from CardAnimationManager
        if (animationManager != null)
        {
            Vector3 correctPosition = animationManager.GetCardPosition(gameObject);
            if (correctPosition != Vector3.zero)
            {
                basePosition = correctPosition;
                hoverPosition = basePosition + Vector3.up * hoverHeight;
            }
        }
        else
        {
            // Fallback: use current position if no animation manager
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                basePosition = rectTransform.anchoredPosition;
                hoverPosition = basePosition + Vector3.up * hoverHeight;
            }
        }
        
        // Always use our own animation logic
        currentHoverCoroutine = StartCoroutine(AnimateHover(true));
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isBeingDragged) return;
        
        // Don't process hover exit if any card is being dragged
        if (anyCardBeingDragged) return;
        
        // Don't process hover exit if cards are being repositioned
        if (animationManager != null && animationManager.IsRepositioning()) return;
        
        
        // Stop any existing hover animation first
        if (currentHoverCoroutine != null)
        {
            StopCoroutine(currentHoverCoroutine);
        }
        
        // Calculate the correct base position from CardAnimationManager if available
        if (animationManager != null)
        {
            Vector3 correctPosition = animationManager.GetCardPosition(gameObject);
            if (correctPosition != Vector3.zero)
            {
                basePosition = correctPosition;
            }
        }
        
        // Always use our own animation logic
        currentHoverCoroutine = StartCoroutine(AnimateHover(false));
    }
    
    // Called by DraggableCard when drag starts
    public void OnDragStart()
    {
        isBeingDragged = true;
        anyCardBeingDragged = true; // Set global flag
        
        // Stop any hover animation when dragging starts
        if (currentHoverCoroutine != null)
        {
            StopCoroutine(currentHoverCoroutine);
            currentHoverCoroutine = null;
        }
        
        // Immediately force card to normal state when drag starts
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
            // Also force position to base position to ensure card goes down
            if (animationManager != null)
            {
                Vector3 correctPosition = animationManager.GetCardPosition(gameObject);
                if (correctPosition != Vector3.zero)
                {
                    basePosition = correctPosition;
                    rectTransform.anchoredPosition = basePosition;
                }
            }
            else
            {
                rectTransform.anchoredPosition = basePosition;
            }
        }
    }
    
    // Called by DraggableCard when drag ends
    public void OnDragEnd()
    {
        isBeingDragged = false;
        anyCardBeingDragged = false; // Clear global flag
        
        // Keep card in normal state after drag - no hover checking
        if (currentHoverCoroutine != null)
        {
            StopCoroutine(currentHoverCoroutine);
            currentHoverCoroutine = null;
        }
        
        // Ensure normal scale immediately
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
        }
        
        // Get the correct base position from CardAnimationManager after repositioning
        if (animationManager != null)
        {
            // Wait a frame for repositioning to complete, then update base position
            StartCoroutine(UpdateBasePositionAfterFrame());
        }
        else
        {
            // Fallback: use current position if no animation manager
            if (rectTransform != null)
            {
                basePosition = rectTransform.anchoredPosition;
                hoverPosition = basePosition + Vector3.up * hoverHeight;
            }
        }
    }
    
    private System.Collections.IEnumerator UpdateBasePositionAfterFrame()
    {
        yield return null; // Wait one frame for repositioning to complete
        
        if (animationManager != null)
        {
            Vector3 correctPosition = animationManager.GetCardPosition(gameObject);
            if (correctPosition != Vector3.zero)
            {
                basePosition = correctPosition;
                hoverPosition = basePosition + Vector3.up * hoverHeight;
            }
        }
    }
    
    /// <summary>
    /// Call this when the card's position changes due to repositioning
    /// </summary>
    public void UpdateOriginalPosition()
    {
        // Don't update position if we're currently being repositioned
        if (animationManager != null && animationManager.IsRepositioning()) return;
        
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            basePosition = rectTransform.anchoredPosition;
            hoverPosition = basePosition + Vector3.up * hoverHeight;
        }
    }
    
    /// <summary>
    /// Sets the base position directly (used by CardAnimationManager after repositioning)
    /// </summary>
    public void SetBasePosition(Vector3 position)
    {
        basePosition = position;
        hoverPosition = basePosition + Vector3.up * hoverHeight;
    }
    
    private System.Collections.IEnumerator AnimateHover(bool hover)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) yield break;
        
        Vector3 startScale = rectTransform.localScale;
        Vector3 startPos = hover ? basePosition : hoverPosition;
        
        Vector3 targetScale = hover ? Vector3.one * hoverScale : Vector3.one;
        Vector3 targetPos = hover ? hoverPosition : basePosition;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationSpeed)
        {
            float normalizedTime = elapsedTime / animationSpeed;
            float easedTime = hover ? EaseOutQuad(normalizedTime) : EaseInQuad(normalizedTime);
            
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, easedTime);
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, targetPos, easedTime);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        rectTransform.localScale = targetScale;
        rectTransform.anchoredPosition = targetPos;
        
        // Clear the coroutine reference when animation completes
        currentHoverCoroutine = null;
    }
    
    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
    
    private float EaseInQuad(float t)
    {
        return t * t;
    }
}
