using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [Header("--- Audio Manager ---")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] public AudioSource SFXSource;
    [SerializeField] private AudioSource fallingSource;

    [Header("--- Clips ---")]
    [SerializeField] public AudioClip[] audioClips;
    [SerializeField] private List<AudioClip> impactSounds; // List of impact sounds

    [SerializeField] private PauseMenu pauseMenu;

    void Start()
    {
        pauseMenu.Initialize();
        PlaySFX(audioClips[0]);
        Coin.OnCoinCollected += PlayCoinAudio;
    }

    public void PlaySFX(AudioClip clip)
    {
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
        PlaySFX(audioClips[1]);
    }

    public void PlayFallingAudio()
    {
        // Only play if the fallingSource isn't already playing
        if (fallingSource != null && !fallingSource.isPlaying)
        {
            fallingSource.Play(); // Play the falling sound on the dedicated source
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

    private void OnDestroy()
    {
        Coin.OnCoinCollected -= PlayCoinAudio;
    }
}
