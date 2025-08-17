using UnityEngine;
using UnityEditor;

// This script helps you set up the Card Animation system in your scene
[System.Serializable]
public class CardAnimationSetup : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(3, 10)]
    public string instructions = @"1. Create an empty GameObject and add CardAnimationManager component
2. Assign CardHandParent (usually the UI panel containing cards)
3. Optionally create a DeckPosition GameObject to set where cards animate from
4. Disable any Layout Groups on the CardHandParent
5. Assign the CardAnimationManager to your CardManager if desired";
    
    [Header("References")]
    public CardAnimationManager animationManager;
    public CardManager cardManager;
    public Transform cardHandParent;
    public Transform deckPosition;
    
    [Header("Quick Setup")]
    public bool autoSetupOnAwake = false;
    
    void Awake()
    {
        if (autoSetupOnAwake)
        {
            SetupCardAnimation();
        }
    }
    
    [ContextMenu("Setup Card Animation System")]
    public void SetupCardAnimation()
    {
        // Find CardManager if not assigned
        if (cardManager == null)
        {
            cardManager = FindObjectOfType<CardManager>();
        }
        
        // Find or create animation manager
        if (animationManager == null)
        {
            animationManager = FindObjectOfType<CardAnimationManager>();
            if (animationManager == null)
            {
                GameObject animManagerGO = new GameObject("CardAnimationManager");
                animManagerGO.transform.SetParent(transform);
                animationManager = animManagerGO.AddComponent<CardAnimationManager>();
            }
        }
        
        // Setup animation manager
        if (animationManager != null)
        {
            if (cardHandParent != null)
            {
                animationManager.cardHandParent = cardHandParent;
            }
            else if (cardManager != null && cardManager.cardHandParent != null)
            {
                animationManager.cardHandParent = cardManager.cardHandParent;
                cardHandParent = cardManager.cardHandParent;
            }
            
            if (deckPosition != null)
            {
                animationManager.deckPosition = deckPosition;
            }
            else
            {
                // Create a deck position if none exists
                GameObject deckPosGO = new GameObject("DeckPosition");
                deckPosGO.transform.SetParent(transform);
                // Position it to the right of the hand
                if (cardHandParent != null)
                {
                    deckPosGO.transform.position = cardHandParent.position + Vector3.right * 300f;
                }
                animationManager.deckPosition = deckPosGO.transform;
                deckPosition = deckPosGO.transform;
            }
            
            // Set better default values for card layout
            animationManager.handWidth = 600f; // Reduced from 800f for better card spacing
            animationManager.cardSpacing = 100f; // Reduced from 120f
            animationManager.maxCardOverlap = 60f; // Increased from 50f
        }
        
        // Disable layout groups on card hand parent
        if (cardHandParent != null)
        {
            var layoutGroups = cardHandParent.GetComponents<UnityEngine.UI.LayoutGroup>();
            foreach (var layoutGroup in layoutGroups)
            {
                layoutGroup.enabled = false;
                Debug.Log($"Disabled {layoutGroup.GetType().Name} on {cardHandParent.name}");
            }
            
            // Also disable ContentSizeFitter if present
            var contentSizeFitter = cardHandParent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
                Debug.Log($"Disabled ContentSizeFitter on {cardHandParent.name}");
            }
            
            // Ensure card hand parent is properly anchored for centering
            RectTransform parentRect = cardHandParent.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                // Set anchors to center-bottom for typical hand positioning
                parentRect.anchorMin = new Vector2(0.5f, 0f);
                parentRect.anchorMax = new Vector2(0.5f, 0f);
                parentRect.pivot = new Vector2(0.5f, 0.5f);
                Debug.Log($"Set {cardHandParent.name} anchors to center-bottom for proper centering");
            }
        }
        
        Debug.Log("Card Animation System setup complete!");
        
        // Force reposition any existing cards
        if (animationManager != null)
        {
            animationManager.ForceRepositionCards();
        }
    }
    
    [ContextMenu("Test Draw Cards")]
    public void TestDrawCards()
    {
        if (cardManager != null)
        {
            cardManager.RefillHandToMaxSize(5);
        }
        else
        {
            Debug.LogWarning("No CardManager assigned!");
        }
    }
    
    [ContextMenu("Clear Hand")]
    public void TestClearHand()
    {
        if (cardManager != null)
        {
            cardManager.ClearHand();
        }
        else
        {
            Debug.LogWarning("No CardManager assigned!");
        }
    }
}
