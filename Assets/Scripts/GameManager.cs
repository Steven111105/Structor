using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int levelIndex;
    [Header("Game Settings")]
    public int maxHandSize = 5;
    public int maxAttacks = 3;
    public int maxDiscards = 3;
    public int damageQuota = 100;

    [Header("Current Game State")]
    public int currentAttack = 0;
    public int currentDiscards = 0;
    public int totalDamageDealt = 0;
    public int coins = 0;
    public bool gameActive = true;

    [Header("UI References")]
    public TMP_Text attackCounterText;
    public TMP_Text damageCounterText;
    public TMP_Text discardCounterText;
    public TMP_Text moneyText;
    public Button attackButton;
    public TMP_Text debuffText;
    public Transform discardPanel;

    [Header("Chips Gained")]
    public GameObject gainedChipsPanel;
    public TMP_Text attackSavedLabel;
    public TMP_Text attackSavedGain;
    public TMP_Text damageSavedLabel;
    public TMP_Text damageSavedGain;
    public TMP_Text totalGainLabel;
    public TMP_Text totalGainText;

    [Header("Discard UI")]
    public Sprite countOn;
    public Sprite countOff;

    public GameObject battlePanel;
    public GameObject gameOverPanel;
    int highestDamage;
    bool isLoading = false;

    // Signal interf 20% less atack
    // Memory fragmentation max hand -1 (temporary)
    // System uodate (discard all after attack)
    // Energy Drain (1st card doesnt do anything) //maybe scrap
    // Boosters block (cant use boosters)

    public bool isSignalInterference = false;
    public bool isMemoryFragmentation = false;
    public bool isSystemUpdate = false;
    public bool isBoostersBlock = false;

    void Awake()
    {
        instance = this;
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
        coins = 0;
        gameActive = true;
        // Start with battle
        CardManager.instance.InitializeDeck();
        gainedChipsPanel.transform.parent.gameObject.SetActive(false);
        InitializeBattle();
    }

    [ContextMenu("Initialize Next Battle")]
    public void InitializeBattle()
    {
        if (isLoading) return;
        isSignalInterference = false;
        isMemoryFragmentation = false;
        isSystemUpdate = false;
        isBoostersBlock = false;
        isLoading = true;
        ShopManager.instance.HideShopPanel();
        GridManager.instance.InitializeGrid();
        CardManager.instance.ResetCardsAndDeck();
        currentAttack = 0;
        currentDiscards = 0;
        totalDamageDealt = 0;
        gameActive = true;
        SetQuota(levelIndex);

        Invoke(nameof(OpenBattleScreen), 1f);
        SFXManager.instance.FadeToBattleBGM();
    }

    void OpenBattleScreen()
    {
        battlePanel.SetActive(true);
        moneyText.text = coins.ToString();
        ShopManager.instance.HideShopPanel();
        if (isMemoryFragmentation)
        {
            // Deal initial hand of cards
            CardManager.instance.RefillHandToMaxSize(maxHandSize - 1);
        }
        else
        {
            CardManager.instance.RefillHandToMaxSize(maxHandSize);

        }

        Debug.Log($"Game started! Level {levelIndex}, Quota: {damageQuota} damage in {maxAttacks} attacks");
        isLoading = false;
    }

    void SetQuota(int levelIndex)
    {
        int startingQuota = 300;
        int cycleIncrease = 200;
        int cycleIndex = levelIndex / 3;
        int tierIndex = levelIndex % 3;

        float tierMultiplier;
        switch (tierIndex)
        {
            case 0: // Small
                tierMultiplier = 1.0f;
                debuffText.text = "";
                debuffText.transform.parent.gameObject.SetActive(false);
                break;
            case 1: // Medium
                tierMultiplier = 1.5f;
                debuffText.text = "";
                debuffText.transform.parent.gameObject.SetActive(false);
                break;
            case 2: // Large
                tierMultiplier = 2f;
                SetDebuff();
                debuffText.transform.parent.gameObject.SetActive(true);
                break;
            default:
                tierMultiplier = 1.0f;
                break;
        }
        // (starting quota + (cycleIndex * cycleIncrease)) * cycleMultiplier
        damageQuota = Mathf.RoundToInt((startingQuota + (cycleIndex * cycleIncrease)) * tierMultiplier);
        // Debug.Log($"[GameManager] Level {levelIndex} - Cycle {cycleIndex} => Quota set to {damageQuota}");
    }

    void SetDebuff()
    {
        int random = Random.Range(0, 4);
        switch (random)
        {
            case 0:
                isSignalInterference = true;
                Debug.Log("Signal Interference! Max attacks reduced by 20%");
                debuffText.text = "Current Debuff:\nSignal Interference\nOutput is reduced by 20%";
                break;
            case 1:
                isMemoryFragmentation = true;
                Debug.Log("Memory Fragmentation! Max hand size reduced by 1");
                debuffText.text = "Current Debuff:\nMemory Fragmentation\nMax hand size reduced by 1";
                break;
            case 2:
                isSystemUpdate = true;
                Debug.Log("System Update! All cards will be discarded after each attack");
                debuffText.text = "Current Debuff:\nSystem Update\nAll cards discarded after each attack";
                break;
            case 3:
                isBoostersBlock = true;
                Debug.Log("Boosters Block! Cannot use booster cards this round");
                debuffText.text = "Current Debuff:\nBoosters Block\nCannot use booster cards this round";
                break;
            default:
                break;
        }

    }

    void SetupEventListeners()
    {
        // Setup UI button listeners
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(() => StartCoroutine(ExecuteAttack()));
        }
    }

    public void UpdateUI()
    {
        if (attackCounterText != null)
        {
            attackCounterText.text = $"Attacks:\n{currentAttack}/{maxAttacks}";
        }

        if (damageCounterText != null)
        {
            damageCounterText.text = $"{totalDamageDealt}/{damageQuota}";
        }

        if (discardCounterText != null)
        {
            discardCounterText.text = $"Discards: {currentDiscards}/{maxDiscards}";
            UpdateDiscardUI();
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

    public IEnumerator ExecuteAttack()
    {
        if (!gameActive || currentAttack >= maxAttacks)
        {
            Debug.Log("Cannot attack - game over or max attacks reached");
            yield break;
        }

        // Check if there are cards pending discard
        if (CardManager.instance.GetMarkedForDiscardCount() > 0)
        {
            Debug.Log("Cannot attack - there are cards pending discard. Please confirm or cancel discards first.");
            yield break;
        }

        currentAttack++;
        SFXManager.instance.PlaySFX("Shoot");
        Debug.Log($"=== ATTACK {currentAttack} ===");

        // Reset damage tracking for this attack
        int damageThisAttack = 0;

        // Execute the attack through GridManager

        // Track damage before firing
        int damageBeforeAttack = totalDamageDealt;

        GridManager.instance.FireBeams();

        Debug.Log("Calling ShowSuccessfulBeamPaths and waiting for it to complete...");
        yield return StartCoroutine(GridManager.instance.ShowSuccessfulBeamPaths());
        Debug.Log("Calling ShowSuccessfulBeamPaths and waiting for it to complete... DONE");


        // Calculate damage dealt this attack
        damageThisAttack = totalDamageDealt - damageBeforeAttack;
        Debug.Log($"Attack {currentAttack} dealt {damageThisAttack} damage");

        // Check win/lose conditions
        CheckGameState();

        // If game is still active, automatically start next turn
        if (gameActive && currentAttack < maxAttacks)
        {
            NextTurn();
        }

        // Update UI
        UpdateUI();
        yield return null;
    }

    void UpdateDiscardUI()
    {
        for (int i = 0; i < maxDiscards; i++)
        {
            discardPanel.GetChild(i).gameObject.SetActive(true);
            discardPanel.GetChild(i).GetComponent<Image>().sprite = (i < currentDiscards) ? countOff : countOn;
        }
    }

    public void NextTurn()
    {
        if (!gameActive)
        {
            Debug.Log("Cannot start next turn - game is over");
            return;
        }

        Debug.Log("=== NEXT TURN ===");
        if (isSystemUpdate)
        {
            Debug.Log("Clearing hand");
            CardManager.instance.ClearHand();
        }
        // Refill hand to max size
        Debug.Log($"Refilling hand from {CardManager.instance.currentHandSize} to {maxHandSize}");
        if (isMemoryFragmentation)
        {
            CardManager.instance.RefillHandToMaxSize(maxHandSize - 1);
        }
        else
        {
            CardManager.instance.RefillHandToMaxSize(maxHandSize);
        }

        // Debug.Log($"Hand refilled. Turn {currentAttack + 1} ready.");
    }

    public void OnSensorHit(int damage)
    {
        totalDamageDealt += damage;

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
            gameOverPanel.GetComponent<GameOverManager>().ShowGameOverScreen();
        }
    }

    void ProcessBattleWin()
    {
        levelIndex++;

        CalculateMoneyGained();
        ShowChipsGained();
        // Invoke(nameof(OpenShop), 1f);
        
    }

    int extraEnergyGain = 0;
    void CalculateMoneyGained()
    {
        int coinsEarned = 0;
        coinsEarned += (maxAttacks - currentAttack) * 2;

        int extraQuota = totalDamageDealt - damageQuota;
        int threshold = 20;

        extraEnergyGain = 0;
        while (extraQuota >= threshold)
        {
            coinsEarned += 1;
            extraEnergyGain++;
            extraQuota -= threshold;
            threshold *= 3;
        }

        // Apply coins earned
        coins += coinsEarned;
        moneyText.text = coins.ToString();
        Debug.Log($"Coins earned: {coinsEarned} ({(maxAttacks - currentAttack) * 2} from attack), Total coins: {coins}");
    }

    void ShowChipsGained()
    {
        int attackGain = (maxAttacks - currentAttack) * 2;
        int totalGained = attackGain + extraEnergyGain;

        gainedChipsPanel.transform.parent.gameObject.SetActive(true);
        attackSavedLabel.text = $"Attack Saved: {maxAttacks - currentAttack}";
        attackSavedGain.text = attackGain.ToString();
        
        damageSavedLabel.text = $"Extra Energy: {totalDamageDealt - damageQuota}";
        damageSavedGain.text = extraEnergyGain.ToString();

        totalGainLabel.text = "Total Chips Gained:";
        totalGainText.text = totalGained.ToString();
    }

    public void OpenShop()
    {
        SFXManager.instance.FadeToShopBGM();
        ShopManager.instance.ShowShopPanel();
        gameOverPanel.SetActive(false);
        ResetRound();
    }

    void ResetRound()
    {

        // reset damage, quota, discard, etc etc
        totalDamageDealt = 0;
        currentAttack = 0;
        currentDiscards = 0;
        GridManager.instance.ClearGridObjects();
        gainedChipsPanel.transform.parent.gameObject.SetActive(false);
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

    public bool CanDiscard()
    {
        return gameActive && currentDiscards < maxDiscards;
    }

    void OnDestroy()
    {
        instance = null;
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