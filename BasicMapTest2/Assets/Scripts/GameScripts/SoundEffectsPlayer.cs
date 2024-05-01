using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundEffectsPlayer : MonoBehaviour
{
    public AudioSource audioSrc;
    public AudioClip errorSFX;
    public AudioClip correctClickSFX;

    private void Start()
    {
        audioSrc.volume = 0.2f;
    }

    public void PlayErrorSound()
    {
        audioSrc.clip = errorSFX;
        audioSrc.Play();
    }

    public void PlayCorrectClickSFX()
    {
        audioSrc.clip = correctClickSFX;
        audioSrc.Play();
    }
}
