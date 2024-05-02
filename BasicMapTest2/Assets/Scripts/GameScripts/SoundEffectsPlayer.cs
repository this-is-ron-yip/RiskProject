using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundEffectsPlayer : MonoBehaviour
{
    public AudioSource audioSrc;
    public AudioClip errorSFX;
    public AudioClip correctClickSFX;
    public AudioClip victoryMusic;
    public AudioClip swordClashingSFX;
    public AudioClip fortifySFX;
    public AudioClip wonAttackSound;
    public AudioClip lostAttackSound;

    private void Start()
    {
        audioSrc.volume = 0.1f;
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

    internal void PlayVictorySound()
    {
        audioSrc.clip = victoryMusic;
        audioSrc.Play();
    }

    internal void PlaySwordClashingSFX()
    {
        audioSrc.clip = swordClashingSFX;
        audioSrc.Play();
    }

    internal void PlayFortifySFX()
    {
        audioSrc.clip = fortifySFX;
        audioSrc.Play();
    }

    internal void playWonAttackSound()
    {
        audioSrc.clip = wonAttackSound;
        audioSrc.Play();
    }

    internal void playLostAttackSound()
    {
        audioSrc.clip = lostAttackSound;
        audioSrc.Play();
    }
}
