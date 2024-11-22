using System;
using System.Collections;
using System.Collections.Generic;
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
        PlaySFX(audioClips[1]);
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

    private void OnDestroy()
    {
        Coin.OnCoinCollected -= PlayCoinAudio;
    }
}
