using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Sprite mutedSprite;
    public Sprite unMutedSprite;
    public AudioMixer mainMixer;

    public AudioSource menuMusic;
    public AudioSource battleMusic;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void PlayBattleMusic()
    {
        menuMusic.Stop();
        battleMusic.Play();
    }

    public void ToggleMute(bool audioOn)
    {
        float volume = audioOn ? 0f : -80f; // -80dB is effectively silence
        mainMixer.SetFloat("MasterVolume", volume);
    }
}
