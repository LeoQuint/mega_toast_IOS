using UnityEngine;
using System.Collections.Generic;

public class SoundController : MonoBehaviour {

    public static SoundController instance = null;

    public AudioClip music;
    public float musicVolume = 1f;
    AudioSource aud;
    public float soundFXVolume = 1f;

    public List<SoundFXClass> soundFXList;

    AudioSource musicAud;

    void Awake()
    {


        //Check if instance already exists
        if (instance == null)
        {
            //if not, set instance to this
            instance = this;
        }
        //If instance already exists and it's not this:
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

    }

    void Start()
    {
        aud = gameObject.GetComponent<AudioSource>();
        musicAud = transform.FindChild("Music").GetComponent<AudioSource>();
        aud.volume = soundFXVolume;
        musicAud.volume = musicVolume;
    }

    public void Mute()
    {
        aud.volume = 0f;
        musicAud.volume = 0f;
    }
    public void UnMute()
    {
        aud.volume = soundFXVolume;
        musicAud.volume = musicVolume;
    }

    public void SetVolume(float vol)
    {
        aud.volume = vol;
    }

    public void PlayClip(int clipPos)
    {
        aud.PlayOneShot(soundFXList[clipPos].clip, soundFXList[clipPos].volume);
    }
}
[System.Serializable]
public class SoundFXClass
{
    public AudioClip clip;
    public float volume;
}