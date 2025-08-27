using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrashCanUI : MonoBehaviour
{
    [Header("UI References")]
    public Button trashCanButton;
    public TextMeshProUGUI markedCountText; // Optional: Shows how many cards are marked
    
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color confirmColor = Color.red;
    
    private CardManager cardManager;
    private GameManager gameManager;
    private CardAnimationManager cardAnimationManager;
    private Image buttonImage;
    
    void Start()
    {
        cardManager = FindObjectOfType<CardManager>();
        gameManager = FindObjectOfType<GameManager>();
        cardAnimationManager = FindObjectOfType<CardAnimationManager>();
        buttonImage = GetComponent<Image>();
        
        if (trashCanButton == null)
            trashCanButton = GetComponent<Button>();
            
        // Set up button click
        if (trashCanButton != null)
        {
            trashCanButton.onClick.AddListener(OnTrashCanClicked);
        }
        
        // Make sure this GameObject has the TrashCan tag for drag detection
        if (!gameObject.CompareTag("TrashCan"))
        {
            gameObject.tag = "TrashCan";
        }
    }
    
    void Update()
    {
        UpdateVisuals();
    }
    
    void UpdateVisuals()
    {
        if (cardManager == null || buttonImage == null) return;
        
        bool hasPending = cardManager.hasPendingDiscards;
        bool isAnimating = cardAnimationManager != null && 
                          (cardAnimationManager.IsAnimating() || cardAnimationManager.IsRepositioning());
        
        // Update text if available
        if (markedCountText != null)
        {
            if (hasPending)
            {
                markedCountText.text = "!";  // Simple indicator
                markedCountText.gameObject.SetActive(true);
            }
            else
            {
                markedCountText.gameObject.SetActive(false);
            }
        }
        
        // Update button interactability - disable if animating
        if (trashCanButton != null)
        {
            trashCanButton.interactable = !isAnimating;
        }
        
        // Update button color based on state
        if (isAnimating)
        {
            buttonImage.color = Color.gray; // Gray when disabled
        }
        else if (hasPending)
        {
            buttonImage.color = confirmColor; // Red when ready to confirm
        }
        else
        {
            buttonImage.color = normalColor; // Normal when no cards pending
        }
        
        // Notify GameManager to update UI (especially attack button state)
        if (gameManager != null)
        {
            gameManager.UpdateUI();
        }
    }
    
    public void OnTrashCanClicked()
    {
        if (cardManager == null) return;
        
        if (cardManager.hasPendingDiscards)
        {
            // Confirm discard (uses 1 discard action, refills hand)
            // Debug.Log($"Confirming discard of trashed cards (1 discard action)");
            cardManager.ConfirmDiscards();
        }
        else
        {
            Debug.Log("No cards pending discard");
        }
    }
    
    // Optional: Right-click to cancel discards
    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1)) // Right click
        {
            if (cardManager != null)
            {
                cardManager.CancelDiscards();
                Debug.Log("Cancelled all discard markings");
            }
        }
    }
    
    // Visual feedback when dragging over trash can
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<DraggableCard>() != null && buttonImage != null)
        {
            buttonImage.color = highlightColor;
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<DraggableCard>() != null)
        {
            UpdateVisuals(); // Return to appropriate color
        }
    }
}
