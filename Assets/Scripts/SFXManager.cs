using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SFX
{
    public string name;
    public AudioClip clip;
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

    public void PlaySFX(string sfxName)
    {
        foreach (SFX sfx in sfxList)
        {
            if (sfx.name == sfxName)
            {
                AudioSource.PlayClipAtPoint(sfx.clip, Camera.main.transform.position);
                return;
            }
        }
    }
}
