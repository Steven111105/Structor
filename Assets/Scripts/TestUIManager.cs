using UnityEngine;
using UnityEngine.UI;

public class TestUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Button fireBeamButton;
    public Text damageText;
    public Text quotaText;
    public Transform cardHand; // Parent for draggable cards
    
    [Header("Test Settings")]
    public GridManager gridManager;
    public GameObject draggableCardPrefab;
    public TestCardSetup testCardSetup;
    
    [Header("Debug Info")]
    public int currentQuota = 0;
    public int targetQuota = 100;
    
    void Start()
    {
        SetupUI();
        SetupEventListeners();
        CreateTestCards();
    }
    
    void SetupUI()
    {
        if (fireBeamButton != null)
        {
            fireBeamButton.onClick.AddListener(() => gridManager.FireBeams());
        }
        
        UpdateUI();
    }
    
    void SetupEventListeners()
    {
        if (gridManager != null)
        {
            gridManager.OnSensorHit.AddListener(OnSensorHit);
            gridManager.OnBeamProcessed.AddListener(OnBeamProcessed);
            gridManager.OnBeamFired.AddListener(OnBeamFired);
        }
    }
    
    void CreateTestCards()
    {
        if (testCardSetup == null || draggableCardPrefab == null || cardHand == null) return;
        
        // Create test cards in hand
        CreateCardInHand(testCardSetup.straightWire);
        CreateCardInHand(testCardSetup.leftBendWire);
        CreateCardInHand(testCardSetup.rightBendWire);
        CreateCardInHand(testCardSetup.booster);
        CreateCardInHand(testCardSetup.sensor2x2);
    }
    
    void CreateCardInHand(CardData cardData)
    {
        if (cardData == null) return;
        
        GameObject cardObj = Instantiate(draggableCardPrefab, cardHand);
        DraggableCard draggable = cardObj.GetComponent<DraggableCard>();
        if (draggable != null)
        {
            draggable.cardData = cardData;
        }
        
        // Set card visual
        Image cardImage = cardObj.GetComponent<Image>();
        if (cardImage != null && cardData.cardSprite != null)
        {
            cardImage.sprite = cardData.cardSprite;
        }
        
        // Add card name text
        Text cardText = cardObj.GetComponentInChildren<Text>();
        if (cardText != null)
        {
            cardText.text = cardData.cardName;
        }
    }
    
    // Event handlers
    void OnSensorHit(int contribution, Vector2Int position)
    {
        currentQuota += contribution;
        Debug.Log($"Sensor hit at {position} for {contribution} points! Total: {currentQuota}");
        UpdateUI();
    }
    
    void OnBeamProcessed(float boostedDamage)
    {
        Debug.Log($"Beam boosted to {boostedDamage} damage!");
    }
    
    void OnBeamFired(float damage, Vector2Int position, Direction direction)
    {
        Debug.Log($"Beam fired towards {direction} with {damage} damage at {position}");
    }
    
    void UpdateUI()
    {
        if (damageText != null)
        {
            damageText.text = $"Base Damage: {gridManager?.baseDamage ?? 1f}";
        }
        
        if (quotaText != null)
        {
            quotaText.text = $"Quota: {currentQuota} / {targetQuota}";
        }
    }
    
    [ContextMenu("Reset Quota")]
    public void ResetQuota()
    {
        currentQuota = 0;
        UpdateUI();
    }
    
    void Update()
    {
        // Debug controls
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetQuota();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreateTestCards();
        }
    }
}
