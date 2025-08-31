using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedDeckData : MonoBehaviour
{
    public static SelectedDeckData instance;

    public DeckConfiguration selectedDeck;

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
        }
    }
}
