using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound : MonoBehaviour
{
    //Elements of the Audioclip that I want to be changeable
    public enum AudioTypes { sfx, music}
    public AudioTypes type;
    [HideInInspector] public AudioSource source;
    public string clipname;
    public AudioClip audioClip;
    public bool isLoop;
    public bool playOnAwake;
}
