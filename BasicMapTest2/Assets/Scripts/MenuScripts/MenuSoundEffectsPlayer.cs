using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSoundEffectsPlayer : MonoBehaviour
{
    public AudioSource audioSrc;
    public AudioClip introMusic;

    private void Start()
    {
        audioSrc.clip = introMusic;
        audioSrc = GetComponent<AudioSource>();
        audioSrc.volume = 0f;
        StartCoroutine(Fade(true, audioSrc, 2f, 1f));
    }

    private void Update()
    {
        if (!audioSrc.isPlaying)
        {
            audioSrc.Play();
            StartCoroutine(Fade(true, audioSrc, 2f, 1f));
        }
    }

    public IEnumerator Fade(bool fadeIn, AudioSource audioSrc, float duration, float targetVolume)
    {
        if (!fadeIn)
        {
            double lengthOfSource = (double)audioSrc.clip.samples / audioSrc.clip.frequency;
            yield return new WaitForSecondsRealtime((float)(lengthOfSource-duration));
        }

        float time = 0f;
        float startVol = audioSrc.volume;
        while (time < duration)
        {
            string fadeStatus = fadeIn ? "fadeIn" : "fadeOut";
            time += Time.deltaTime;
            audioSrc.volume += Time.deltaTime;
            audioSrc.volume = Mathf.Lerp(startVol, targetVolume, time/duration);
            yield return null;
        }
        yield break;
    }
}
