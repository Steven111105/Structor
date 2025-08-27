using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI attackCounterText;
    public TextMeshProUGUI damageCounterText;
    public TextMeshProUGUI discardCounterText;
    public TextMeshProUGUI quotaText;
    public TextMeshProUGUI gameStatusText;
    public Button attackButton;
    public Button nextTurnButton;
    public Button resetButton;
    
    [Header("Panels")]
    public GameObject shopPanel;
    public GameObject defeatPanel;
    
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        if (gameManager != null)
        {
            // Connect UI references to GameManager
            gameManager.attackCounterText = attackCounterText;
            gameManager.damageCounterText = damageCounterText;
            gameManager.discardCounterText = discardCounterText;
            gameManager.quotaText = quotaText;
            gameManager.attackButton = attackButton;
            
            // Setup event listeners
            gameManager.OnGameWon.AddListener(ShowShopPanel);
            gameManager.OnGameLost.AddListener(ShowDefeatPanel);
        }
        
        // Setup reset button
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(() => {
                if (gameManager != null) gameManager.ResetGame();
                HideGameOverPanels();
            });
        }
        
        // Hide game over panels initially
        HideGameOverPanels();
    }
    
    void ShowShopPanel()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
        
        if (gameStatusText != null)
        {
            gameStatusText.text = "VICTORY!";
            gameStatusText.color = Color.green;
        }
    }
    
    void ShowDefeatPanel()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }
        
        if (gameStatusText != null)
        {
            gameStatusText.text = "DEFEAT!";
            gameStatusText.color = Color.red;
        }
    }
    
    void HideGameOverPanels()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }
        
        if (gameStatusText != null)
        {
            gameStatusText.text = "";
        }
    }
}
