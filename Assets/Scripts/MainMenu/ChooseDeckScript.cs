using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseDeckScript : MonoBehaviour
{
    public List<DeckConfiguration> availableDecks;
    public Button nextDeckButton;
    public Button prevDeckButton;
    public Transform deckDotsParent;
    public Button pickDeckButton;
    public Button backButton;
    public TMP_Text deckNameText;
    public TMP_Text deckDescriptionText;
    public int currentDeckIndex = 0;
    public Sprite emptyDotSprite;
    public Sprite filledDotSprite;

    void Start()
    {
        string temp = "" + currentDeckIndex;
        PlayerPrefs.SetInt("hasUnlockedDeck0", 1);
        InstantiateButtons();
        OpenDeckSelection();
    }

    public void InstantiateButtons()
    {
        nextDeckButton.onClick.AddListener(NextDeck);
        nextDeckButton.onClick.AddListener(() =>SFXManager.instance.PlaySFX("ButtonClick"));
        prevDeckButton.onClick.AddListener(PrevDeck);
        prevDeckButton.onClick.AddListener(() => SFXManager.instance.PlaySFX("ButtonClick"));
        pickDeckButton.onClick.AddListener(PickDeck);
        backButton.onClick.AddListener(Back);
    }

    public void UpdateDeckUI()
    {
        deckNameText.text = availableDecks[currentDeckIndex].deckName;
        deckDescriptionText.text = availableDecks[currentDeckIndex].deckDescription;
        foreach (Transform child in deckDotsParent)
        {
            child.GetComponent<Image>().sprite = emptyDotSprite;
        }
        deckDotsParent.GetChild(currentDeckIndex).GetComponent<Image>().sprite = filledDotSprite;

        //check if has unlocked this deck, if not, disable pick button
        string temp = "hasUnlockedDeck" + currentDeckIndex;
        if (PlayerPrefs.GetInt(temp) == 1)
        {
            pickDeckButton.interactable = true;
        }
        else
        {
            pickDeckButton.interactable = false;
        }
    }

    public void OpenDeckSelection()
    {
        gameObject.SetActive(true);
        currentDeckIndex = PlayerPrefs.GetInt("SelectedDeckIndex", 0);
        UpdateDeckUI();
    }
    public void NextDeck()
    {
        if (currentDeckIndex < availableDecks.Count - 1)
        {
            currentDeckIndex++;
        }
        else
        {
            currentDeckIndex = 0;
        }
        UpdateDeckUI();
    }

    public void PrevDeck()
    {
        if (currentDeckIndex > 0)
        {
            currentDeckIndex--;
        }
        else
        {
            currentDeckIndex = availableDecks.Count - 1;
        }
        UpdateDeckUI();
    }


    public void PickDeck()
    {
        if (PlayerPrefs.GetInt("hasSeenTutorial", 0) == 0)
        {
            TutorialManager.instance.OpenTutorial();
            PlayerPrefs.SetInt("hasSeenTutorial", 1);
            return;
        }
        // Send current deck index to gamemanager
        SelectedDeckData.instance.selectedDeck = availableDecks[currentDeckIndex];
        SceneManager.LoadScene("Gameplay");
    }

    public void Back()
    {
        // Load previous scene or main menu
        gameObject.SetActive(false);
    }
}
