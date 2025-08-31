using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;
    public GameObject shopPanel;
    public GameObject componentPanel;
    public GameObject boostersPanel;
    public GameObject permanentUpgrade;
    [SerializeField] List<CardData> boosterPool;
    [SerializeField] TMP_Text coinText;
    Button componentBuyButton1;
    Button componentBuyButton2;
    Button boosterBuyButton1;
    Button boosterBuyButton2;
    Button handUpgradeBuyButton;
    Button discardUpgradeBuyButton;
    CardData component1;
    CardData component2;
    CardData booster1;
    CardData booster2;

    void Awake()
    {
        instance = this;
        InitializeBuyButtons();
        shopPanel.SetActive(false);
    }

    void InitializeBuyButtons()
    {
        // Assuming the first two children of componentPanel and boostersPanel are the buy buttons
        componentBuyButton1 = componentPanel.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        Debug.Log($"Is comp buy button 1 null {componentBuyButton1 == null}");
        componentBuyButton2 = componentPanel.transform.GetChild(1).GetChild(0).GetComponent<Button>();
        boosterBuyButton1 = boostersPanel.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        boosterBuyButton2 = boostersPanel.transform.GetChild(1).GetChild(0).GetComponent<Button>();
        handUpgradeBuyButton = permanentUpgrade.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        discardUpgradeBuyButton = permanentUpgrade.transform.GetChild(1).GetChild(0).GetComponent<Button>();

        componentBuyButton1.onClick.AddListener(() => BuyItem(0, 0));
        componentBuyButton2.onClick.AddListener(() => BuyItem(0, 1));
        boosterBuyButton1.onClick.AddListener(() => BuyItem(1, 0));
        boosterBuyButton2.onClick.AddListener(() => BuyItem(1, 1));
        handUpgradeBuyButton.onClick.AddListener(() => BuyItem(2, 0));
        discardUpgradeBuyButton.onClick.AddListener(() => BuyItem(2, 1));
        UpdateBuyButtons();

        Debug.Log("Initialized buy Buttons");

    }

    [ContextMenu("Show Shop Panel")]
    public void ShowShopPanel()
    {
        SetupShopItems();
        UpdateBuyButtons();
        shopPanel.SetActive(true);
    }

    private void SetupShopItems()
    {
        // TODO: Implement shop item setup logic here
        // Randomize Component
        // Get the list of components from selected deck data, we randomize from the original
        // enable all items first
        componentBuyButton1.transform.parent.gameObject.SetActive(true);
        componentBuyButton2.transform.parent.gameObject.SetActive(true);
        boosterBuyButton1.transform.parent.gameObject.SetActive(true);
        boosterBuyButton2.transform.parent.gameObject.SetActive(true);
        handUpgradeBuyButton.transform.parent.gameObject.SetActive(true);
        discardUpgradeBuyButton.transform.parent.gameObject.SetActive(true);

        DeckConfiguration components = SelectedDeckData.instance.selectedDeck;
        int random1 = Random.Range(0, components.cardEntries.Length);
        int random2 = Random.Range(0, components.cardEntries.Length);
        while (random2 == random1 && components.cardEntries.Length > 1)
        {
            random2 = Random.Range(0, components.cardEntries.Length);
        }
        componentPanel.transform.GetChild(0).GetComponent<Image>().sprite = components.cardEntries[random1].cardData.cardSprite;
        component1 = components.cardEntries[random1].cardData;
        componentPanel.transform.GetChild(1).GetComponent<Image>().sprite = components.cardEntries[random2].cardData.cardSprite;
        component2 = components.cardEntries[random2].cardData;

        // Randomize Boosters
        random1 = Random.Range(0, boosterPool.Count);
        random2 = Random.Range(0, boosterPool.Count);
        while (random2 == random1 && components.cardEntries.Length > 1)
        {
            random2 = Random.Range(0, boosterPool.Count);
        }
        boostersPanel.transform.GetChild(0).GetComponent<Image>().sprite = boosterPool[random1].cardSprite;
        booster1 = boosterPool[random1];
        boostersPanel.transform.GetChild(1).GetComponent<Image>().sprite = boosterPool[random2].cardSprite;
        booster2 = boosterPool[random2];
    }

    void BuyItem(int panelIndex, int buttonIndex)
    {
        if (panelIndex == 0) // Component Panel
        {
            if (buttonIndex == 0)
                CardManager.instance.PurchaseCard(component1);
            else if (buttonIndex == 1)
                CardManager.instance.PurchaseCard(component2);

            GameManager.instance.coins -= 7;
        }
        else if (panelIndex == 1) // Boosters Panel
        {
            if (buttonIndex == 0)
                CardManager.instance.PurchaseCard(booster1);
            else if (buttonIndex == 1)
                CardManager.instance.PurchaseCard(booster2);
            GameManager.instance.coins -= 7;
        }
        else if (panelIndex == 2) // Permanent Upgrade
        {
            if (buttonIndex == 0)
                GameManager.instance.maxHandSize += 1;
            else if (buttonIndex == 1)
                GameManager.instance.maxDiscards += 1;
            GameManager.instance.coins -= 10;
        }

        shopPanel.transform.GetChild(panelIndex).GetChild(buttonIndex).gameObject.SetActive(false);
        SFXManager.instance.PlaySFX("Purchase");
        UpdateBuyButtons();
    }

    void UpdateBuyButtons()
    {
        int money = GameManager.instance.coins;
        coinText.text = money.ToString();

        if (money < 7)
        {
            componentBuyButton1.interactable = false;
            componentBuyButton2.interactable = false;
            boosterBuyButton1.interactable = false;
            boosterBuyButton2.interactable = false;
        }
        else
        {
            componentBuyButton1.interactable = true;
            componentBuyButton2.interactable = true;
            boosterBuyButton1.interactable = true;
            boosterBuyButton2.interactable = true;
        }

        if (money < 10)
        {
            if (discardUpgradeBuyButton != null)
                discardUpgradeBuyButton.interactable = false;
            if (handUpgradeBuyButton != null)
                handUpgradeBuyButton.interactable = false;
        }
        else
        {
            if (discardUpgradeBuyButton != null)
                discardUpgradeBuyButton.interactable = true;
            if (handUpgradeBuyButton != null)
                handUpgradeBuyButton.interactable = true;
        }
    }

    public void HideShopPanel()
    {
        shopPanel.SetActive(false);
    }

    void OnDestroy()
    {
        instance = null;
    }
}
