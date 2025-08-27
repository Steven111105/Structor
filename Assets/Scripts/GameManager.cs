using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public EventSystem ResetGameEvent;
    int levelIndex;
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

    // Events
    public UnityEngine.Events.UnityEvent OnGameWon = new UnityEngine.Events.UnityEvent();
    public UnityEngine.Events.UnityEvent OnGameLost = new UnityEngine.Events.UnityEvent();
    public UnityEngine.Events.UnityEvent OnAttackComplete = new UnityEngine.Events.UnityEvent();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    void Start()
    {
        InitializeGame();
        SetupEventListeners();
        UpdateUI();
    }

    void InitializeGame()
    {
        levelIndex = 0;
        currentAttack = 0;
        currentDiscards = 0;
        totalDamageDealt = 0;
        gameActive = true;
        // Start with battle
        InitializeBattle();
    }

    void InitializeBattle()
    {
        GridManager.instance.ClearGridObjects();
        currentAttack = 0;
        currentDiscards = 0;
        totalDamageDealt = 0;
        gameActive = true;
        // Deal initial hand of cards

        CardManager.instance.RefillHandToMaxSize(maxHandSize);

        SetQuota(levelIndex);
        Debug.Log($"Game started! Quota: {damageQuota} damage in {maxAttacks} attacks");
    }

    void SetQuota(int levelIndex)
    {
        int startingQuota = 300;
        int cycleIncrease = 300;
        int cycleIndex = levelIndex / 3;
        int tierIndex = levelIndex % 3;

        float cycleMultiplier;
        switch (cycleIndex)
        {
            case 0: // Small
                cycleMultiplier = 1.0f;
                break;
            case 1: // Medium
                cycleMultiplier = 1.6f;
                break;
            case 2: // Large
                cycleMultiplier = 2.2f;
                break;
            default:
                cycleMultiplier = 1.0f;
                break;
        }

        damageQuota = Mathf.RoundToInt((startingQuota + (cycleIndex * cycleIncrease)) * cycleMultiplier);
        // Debug.Log($"[GameManager] Level {levelIndex} - Cycle {cycleIndex} => Quota set to {damageQuota}");
    }

    void SetupEventListeners()
    {
        // Setup UI button listeners
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(ExecuteAttack);
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
            if (canAttack)
            {
                canAttack = CardManager.instance.GetMarkedForDiscardCount() == 0;
            }

            // Block if cards are animating
            if (canAttack)
            {
                canAttack = !CardAnimationManager.instance.IsAnimating() && !CardAnimationManager.instance.IsRepositioning();
            }

            attackButton.interactable = canAttack;
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
        if (CardManager.instance.GetMarkedForDiscardCount() > 0)
        {
            Debug.Log("Cannot attack - there are cards pending discard. Please confirm or cancel discards first.");
            return;
        }

        currentAttack++;
        Debug.Log($"=== ATTACK {currentAttack} ===");

        // Reset damage tracking for this attack
        int damageThisAttack = 0;

        // Execute the attack through GridManager
        if (GridManager.instance != null)
        {
            // Track damage before firing
            int damageBeforeAttack = totalDamageDealt;

            GridManager.instance.FireBeams();

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
        CardManager.instance.RefillHandToMaxSize(maxHandSize);

        // Debug.Log($"Hand refilled. Turn {currentAttack + 1} ready.");
    }

    public void OnSensorHit(int damage)
    {
        totalDamageDealt += damage;
        // Debug.Log($"Sensor hit! +{damage} damage. Total: {totalDamageDealt}/{damageQuota}");

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
            ProcessBattleWin();
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

    void ProcessBattleWin()
    {
        // add round
        levelIndex++;
        
        // reset damage, quota, discard, etc etc
        totalDamageDealt = 0;
        currentAttack = 0;
        currentDiscards = 0;

        // Open shop panel
        // ShopManager.instance.ShowShopPanel();
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

        UpdateUI();

        // Check if game should end (out of discards)
        if (currentDiscards >= maxDiscards)
        {
            Debug.Log("Maximum discards reached!");
            // Could trigger some UI feedback or warning
        }
    }

    // This is called after we show the end screen so when we go back to the main menu we need to initialize again
    public void ResetGame()
    {
        levelIndex = 0;
        // ResetGameEvent.Invoke;
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


// Quota Calculation:
// baseQuota = startingQuota + (cycleIndex * cycleIncrease)
// quota = round(baseQuota * tierMultiplier)

// startingQuota = 500, cycleIncrease = 300
// tierMultiplier
// Small (round % 3 == 1): 1.0

// Medium (round % 3 == 2): 1.6

// Large (round % 3 == 0): 2.2