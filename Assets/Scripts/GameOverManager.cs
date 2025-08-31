using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public TMP_Text quotaText;
    public void ShowGameOverScreen()
    {
        gameObject.SetActive(true);
        quotaText.text = $"Quota Reached: {GameManager.instance.levelIndex + 1}";
        SFXManager.instance.FadeToGameOverBGM();
    }

    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
