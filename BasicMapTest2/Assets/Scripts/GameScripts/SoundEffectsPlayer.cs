using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsPlayer : MonoBehaviour
{
    public AudioSource audioSrc;
    public AudioClip errorSFX;

    public void PlayErrorSound()
    {
        audioSrc.clip = errorSFX;
        audioSrc.Play();
    }
}
