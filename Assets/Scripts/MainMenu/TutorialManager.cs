using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;
    public GameObject tutorialPanel;
    public Image tutorialImage;
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;
    public List<Sprite> tutorialPages;
    int currentPageIndex = 0;
    int totalPages;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            totalPages = tutorialPages.Count;
            nextButton.onClick.AddListener(NextPage);
            nextButton.onClick.AddListener(() => SFXManager.instance.PlaySFX("ButtonClick"));
            prevButton.onClick.AddListener(PrevPage);
            prevButton.onClick.AddListener(() => SFXManager.instance.PlaySFX("ButtonClick"));
            closeButton.onClick.AddListener(CloseTutorial);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void OpenTutorial()
    {
        tutorialPanel.SetActive(true);
        tutorialImage.sprite = tutorialPages[0];
        prevButton.interactable = false;
        if (totalPages <= 1)
        {
            nextButton.interactable = false;
        }
        else
        {
            nextButton.interactable = true;
        }
    }

    public void NextPage()
    {
        if (currentPageIndex < totalPages)
        {
            currentPageIndex++;
            if (currentPageIndex == totalPages)
            {
                CloseTutorial();
                return;
            }
            tutorialImage.sprite = tutorialPages[currentPageIndex];
            prevButton.interactable = true;
        }
    }
    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            tutorialImage.sprite = tutorialPages[currentPageIndex];
            nextButton.interactable = true;
            if (currentPageIndex == 0)
            {
                prevButton.interactable = false;
            }
        }
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
        currentPageIndex = 0;
        tutorialImage.sprite = tutorialPages[0];
        prevButton.interactable = false;
        if (totalPages <= 1)
        {
            nextButton.interactable = false;
        }
        else
        {
            nextButton.interactable = true;
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
