using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundEffectsPlayer : MonoBehaviour
{
    public AudioSource audioSrc;
    public AudioClip errorSFX;
    public AudioClip correctClickSFX;

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
