using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SFX
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1.0f;
}
public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;
    public AudioSource bgmSource;
    public AudioClip battleBGM;
    public AudioClip shopBGM;
    public AudioClip gameOverBGM;
    public List<SFX> sfxList;

    void Awake()
    {
        instance = this;
    }

    public void PlaySFX(string clipName)
    {
        SFX sfx = sfxList.Find(s => s.name == clipName);
        if (sfx != null)
        {
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();
            tempSource.clip = sfx.clip;
            tempSource.volume = sfx.volume;
            tempSource.Play();
            Destroy(tempSource, sfx.clip.length);
        }
        else
        {
            Debug.Log("Could not find SFX clip: " + clipName);
        }
    }

    public void FadeToShopBGM()
    {
        StartCoroutine(FadeBGM(shopBGM));
    }

    public void FadeToBattleBGM()
    {
        StartCoroutine(FadeBGM(battleBGM));
    }

    public void FadeToGameOverBGM()
    {
        StartCoroutine(FadeBGM(gameOverBGM));
    }

    IEnumerator FadeBGM(AudioClip newClip)
    {
        // Fade out current BGM
        float fadeDuration = 0.5f;
        float startVolume = bgmSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        bgmSource.volume = 0;
        bgmSource.clip = newClip;
        bgmSource.Play();

        // Fade in new BGM
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0, startVolume, t / fadeDuration);
            yield return null;
        }

        bgmSource.volume = startVolume;
    }


}
