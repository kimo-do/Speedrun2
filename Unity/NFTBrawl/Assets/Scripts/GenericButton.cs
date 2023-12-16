using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericButton : MonoBehaviour
{
    private Button myButton;
    private AudioSource myAudioSource;

    private void Awake()
    {
        myButton = GetComponent<Button>();
        myAudioSource = GetComponent<AudioSource>();

        if (myButton != null)
        {
            myButton.onClick.AddListener(PlaySound);
        }
    }

    private void PlaySound()
    {
        if (myAudioSource != null)
        {
            myAudioSource.Play();
        }
    }

}
