using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [Header("--- Audio Manager ---")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] public AudioSource SFXSource;
    [SerializeField] private AudioSource fallingSource;
    [SerializeField] private float minPitch = 0.8f; // Min pitch range
    [SerializeField] private float maxPitch = 1.2f; // Max pitch range

    [Header("--- Clips ---")]
    [SerializeField] public AudioClip[] sfx;
    [SerializeField] public AudioClip[] sondTrack;
    [SerializeField] private List<AudioClip> impactSounds; // List of impact sounds

    [SerializeField] private PauseMenu pauseMenu;
    bool alreadyPlayed = false;
    bool deathMarch = false;

    private void Awake()
    {
        if (!alreadyPlayed)
        {
            musicSource.Play();
            alreadyPlayed = true;
        }
    }
    void Start()
    {
        pauseMenu.Initialize();
        Coin.OnCoinCollected += PlayCoinAudio;
    }

    IEnumerator Chronos()
    {
        yield return new WaitForSeconds(1);
        ResumeSong();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        // Apply random pitch
        SFXSource.pitch = Random.Range(minPitch, maxPitch);
        SFXSource.PlayOneShot(clip);
    }

    public void MuteMaster()
    {
        audioMixer.SetFloat("MasterVolume", -80);
    }

    public void UnmuteMaster()
    {
        audioMixer.SetFloat("MasterVolume", 0);
    }

    public void PlayCoinAudio()
    {
        PlaySFX(sfx[0]);
    }

    public void PlayFallingAudio ()
    {
        // Check if its not playing, then change the pit and Play()
        if (!fallingSource.isPlaying)
        {
            fallingSource.pitch = Random.Range(minPitch, maxPitch);
            fallingSource.Play();
        }

    }

    public void StopFallingAudio()
    {
        if (fallingSource != null && fallingSource.isPlaying)
        {
            fallingSource.Stop();
        }
    }

    // Play a random impact sound from the list
    public void PlayRandomImpactSound()
    {
        if (impactSounds.Count > 0)
        {
            // Select a random clip from the list
            AudioClip randomImpactClip = impactSounds[UnityEngine.Random.Range(0, impactSounds.Count)];
            SFXSource.PlayOneShot(randomImpactClip);
        }
        else
        {
            Debug.LogWarning("No impact sounds are set in the impactSounds list.");
        }
    }
    
    public void PlayJumpSound()
    {
        SFXSource.pitch = Random.Range(minPitch, maxPitch);

        PlaySFX(sfx[1]);
    }

    public void PlayDeathSound()
    {
        musicSource.Pause();
        if (!deathMarch)
        {
            PlaySFX(sfx[2]);
            deathMarch = true;
        }
        else
            return;
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void PlayMusic()
    {
        musicSource.Play();
    }

    public void PlayReviveSound()
    {
        PlaySFX(sfx[3]);
        StartCoroutine(Chronos());
        deathMarch = false;
    }
    public void ResumeSong()
    {
        musicSource.Play();
    }
    private void OnDestroy()
    {
        Coin.OnCoinCollected -= PlayCoinAudio;
    }
}
