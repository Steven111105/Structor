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

    public List<SFX> sfxList;

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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
}
