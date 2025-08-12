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
    private bool isHovering = false;
    private bool isBeingDragged = false;
    
    void Start()
    {
        animationManager = FindObjectOfType<CardAnimationManager>();
        
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
        
        isHovering = true;
        
        if (animationManager != null)
        {
            animationManager.HighlightCard(gameObject, true);
        }
        else
        {
            // Fallback animation if no animation manager
            StartCoroutine(AnimateHover(true));
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isBeingDragged) return;
        
        isHovering = false;
        
        if (animationManager != null)
        {
            animationManager.HighlightCard(gameObject, false);
        }
        else
        {
            // Fallback animation if no animation manager
            StartCoroutine(AnimateHover(false));
        }
    }
    
    // Called by DraggableCard when drag starts
    public void OnDragStart()
    {
        isBeingDragged = true;
        isHovering = false;
    }
    
    // Called by DraggableCard when drag ends
    public void OnDragEnd()
    {
        isBeingDragged = false;
    }
    
    private System.Collections.IEnumerator AnimateHover(bool hover)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) yield break;
        
        Vector3 startScale = rectTransform.localScale;
        Vector3 startPos = rectTransform.anchoredPosition;
        
        Vector3 targetScale = hover ? Vector3.one * hoverScale : Vector3.one;
        Vector3 targetPos = hover ? startPos + Vector3.up * hoverHeight : Vector3.zero;
        
        // If not hovering, we need to calculate the original position
        if (!hover && animationManager != null)
        {
            // Let the animation manager handle returning to position
            yield break;
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationSpeed)
        {
            float normalizedTime = elapsedTime / animationSpeed;
            float easedTime = hover ? EaseOutQuad(normalizedTime) : EaseInQuad(normalizedTime);
            
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, easedTime);
            
            if (hover)
            {
                rectTransform.anchoredPosition = Vector3.Lerp(startPos, startPos + Vector3.up * hoverHeight, easedTime);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        rectTransform.localScale = targetScale;
        if (hover)
        {
            rectTransform.anchoredPosition = startPos + Vector3.up * hoverHeight;
        }
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
