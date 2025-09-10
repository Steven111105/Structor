using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public ChooseDeckScript chooseDeckScript;
    public GameObject basicTutorial;
    public GameObject deckSpecificTutorialParent;

    void Awake()
    {
        basicTutorial.SetActive(false);
        foreach (Transform child in deckSpecificTutorialParent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
    public void OpenTutorial()
    {
        basicTutorial.SetActive(true);
        foreach (Transform child in deckSpecificTutorialParent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
    public void OpenDeckSpecificTutorial()
    {
        basicTutorial.SetActive(false);
        int index = chooseDeckScript.currentDeckIndex;
        deckSpecificTutorialParent.transform.GetChild(index).gameObject.SetActive(true);
    }

    public void CloseTutorial()
    {
        basicTutorial.SetActive(false);
        foreach (Transform child in deckSpecificTutorialParent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
