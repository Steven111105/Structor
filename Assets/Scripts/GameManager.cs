using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int maxHandSize = 5;
    public int maxAttacks = 3;
    public int maxDiscards = 3;
    public int damageQuota = 100;
    
    [Header("Current Game State")]
    public int currentAttack = 0;
    public int currentDiscards = 0;
    public int totalDamageDealt = 0;
    public bool gameActive = true;
    
    [Header("UI References")]
    public TextMeshProUGUI attackCounterText;
    public TextMeshProUGUI damageCounterText;
    public TextMeshProUGUI discardCounterText;
    public TextMeshProUGUI quotaText;
    public Button attackButton;
    public Button nextTurnButton;
    
    [Header("References")]
    public CardManager cardManager;
    public GridManager gridManager;
    
    // Events
    public UnityEngine.Events.UnityEvent OnGameWon = new UnityEngine.Events.UnityEvent();
    public UnityEngine.Events.UnityEvent OnGameLost = new UnityEngine.Events.UnityEvent();
    public UnityEngine.Events.UnityEvent OnAttackComplete = new UnityEngine.Events.UnityEvent();
    
    void Start()
    {
        InitializeGame();
        SetupEventListeners();
        UpdateUI();
    }
    
    void InitializeGame()
    {
        currentAttack = 0;
        currentDiscards = 0;
        totalDamageDealt = 0;
        gameActive = true;

        // Deal initial hand of cards
        if (cardManager != null)
        {
            Debug.Log($"[GameManager] Dealing initial hand of {maxHandSize} cards...");
            cardManager.RefillHandToMaxSize(maxHandSize);
        }
        else
        {
            Debug.LogError("[GameManager] CardManager reference is null! Cannot deal cards.");
        }
        
        Debug.Log($"Game started! Quota: {damageQuota} damage in {maxAttacks} attacks");
    }
    
    void SetupEventListeners()
    {
        // Listen to sensor hits to track damage
        if (gridManager != null)
        {
            gridManager.OnSensorHit.AddListener(OnSensorHit);
        }
        
        // Setup UI button listeners
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(ExecuteAttack);
        }
        
        if (nextTurnButton != null)
        {
            nextTurnButton.onClick.AddListener(NextTurn);
        }
    }
    
    public void UpdateUI()
    {
        if (attackCounterText != null)
        {
            attackCounterText.text = $"Attacks: {currentAttack}/{maxAttacks}";
        }
        
        if (damageCounterText != null)
        {
            damageCounterText.text = $"Damage: {totalDamageDealt}/{damageQuota}";
        }
        
        if (discardCounterText != null)
        {
            discardCounterText.text = $"Discards: {currentDiscards}/{maxDiscards}";
        }
        
        if (quotaText != null)
        {
            quotaText.text = $"Target: {damageQuota}";
        }
        
        // Update button states
        if (attackButton != null)
        {
            bool canAttack = gameActive && currentAttack < maxAttacks;
            
            // Also check if there are cards pending discard
            if (canAttack && cardManager != null)
            {
                canAttack = cardManager.GetMarkedForDiscardCount() == 0;
            }
            
            attackButton.interactable = canAttack;
        }
        
        if (nextTurnButton != null)
        {
            nextTurnButton.interactable = gameActive && currentAttack < maxAttacks;
        }
    }
    
    public void ExecuteAttack()
    {
        if (!gameActive || currentAttack >= maxAttacks)
        {
            Debug.Log("Cannot attack - game over or max attacks reached");
            return;
        }
        
        // Check if there are cards pending discard
        if (cardManager != null && cardManager.GetMarkedForDiscardCount() > 0)
        {
            Debug.Log("Cannot attack - there are cards pending discard. Please confirm or cancel discards first.");
            return;
        }
        
        currentAttack++;
        Debug.Log($"=== ATTACK {currentAttack} ===");
        
        // Reset damage tracking for this attack
        int damageThisAttack = 0;
        
        // Execute the attack through GridManager
        if (gridManager != null)
        {
            // Track damage before firing
            int damageBeforeAttack = totalDamageDealt;
            
            gridManager.FireBeams();
            
            // Calculate damage dealt this attack
            damageThisAttack = totalDamageDealt - damageBeforeAttack;
            Debug.Log($"Attack {currentAttack} dealt {damageThisAttack} damage");
        }
        
        // Check win/lose conditions
        CheckGameState();
        
        // If game is still active, automatically start next turn
        if (gameActive && currentAttack < maxAttacks)
        {
            NextTurn();
        }
        
        // Update UI
        UpdateUI();
        
        // Fire event
        OnAttackComplete?.Invoke();
    }
    
    public void NextTurn()
    {
        if (!gameActive)
        {
            Debug.Log("Cannot start next turn - game is over");
            return;
        }
        
        Debug.Log("=== NEXT TURN ===");
        
        // Refill hand to max size
        if (cardManager != null)
        {
            cardManager.RefillHandToMaxSize(maxHandSize);
        }
        
        Debug.Log($"Hand refilled. Turn {currentAttack + 1} ready.");
    }
    
    void OnSensorHit(int damage, Vector2Int position)
    {
        totalDamageDealt += damage;
        Debug.Log($"Sensor hit! +{damage} damage. Total: {totalDamageDealt}/{damageQuota}");
        
        // Update UI immediately when damage is dealt
        UpdateUI();
    }
    
    void CheckGameState()
    {
        if (totalDamageDealt >= damageQuota)
        {
            // Player wins!
            gameActive = false;
            Debug.Log("=== VICTORY! ===");
            Debug.Log($"Quota achieved! {totalDamageDealt}/{damageQuota} damage dealt in {currentAttack} attacks");
            OnGameWon?.Invoke();
        }
        else if (currentAttack >= maxAttacks)
        {
            // Player loses - ran out of attacks
            gameActive = false;
            Debug.Log("=== DEFEAT! ===");
            Debug.Log($"Quota failed! Only {totalDamageDealt}/{damageQuota} damage dealt in {maxAttacks} attacks");
            OnGameLost?.Invoke();
        }
    }
    
    public void DiscardCard()
    {
        if (!gameActive || currentDiscards >= maxDiscards)
        {
            Debug.Log("Cannot discard - game over or max discards reached");
            return;
        }
        
        currentDiscards++;
        Debug.Log($"Card discarded. Discards used: {currentDiscards}/{maxDiscards}");
        
        // TODO: Implement actual card discard logic
        
        UpdateUI();
    }
    
    public void OnCardsDiscarded()
    {
        if (!gameActive || currentDiscards >= maxDiscards)
        {
            Debug.Log("Cannot discard - game over or max discards reached");
            return;
        }
        
        currentDiscards++;
        Debug.Log($"Cards discarded. Discards used: {currentDiscards}/{maxDiscards}");
        
        // Refill hand after discard
        if (cardManager != null)
        {
            cardManager.RefillHandToMaxSize(maxHandSize);
        }
        
        UpdateUI();
        
        // Check if game should end (out of discards)
        if (currentDiscards >= maxDiscards)
        {
            Debug.Log("Maximum discards reached!");
            // Could trigger some UI feedback or warning
        }
    }
    
    [ContextMenu("Reset Game")]
    public void ResetGame()
    {
        Debug.Log("=== GAME RESET ===");
        
        // Clear the grid
        if (gridManager != null)
        {
            gridManager.ClearAllTestObjects();
        }
        
        // Reset game state
        InitializeGame();
        
        // Refill hand with proper method
        if (cardManager != null)
        {
            cardManager.RefillHandToMaxSize(maxHandSize);
        }
        
        UpdateUI();
    }
    
    [ContextMenu("Debug Game State")]
    public void DebugGameState()
    {
        Debug.Log($"=== GAME STATE DEBUG ===");
        Debug.Log($"Attacks: {currentAttack}/{maxAttacks}");
        Debug.Log($"Damage: {totalDamageDealt}/{damageQuota}");
        Debug.Log($"Discards: {currentDiscards}/{maxDiscards}");
        Debug.Log($"Game Active: {gameActive}");
        Debug.Log($"Hand Size: {maxHandSize}");
    }
}
