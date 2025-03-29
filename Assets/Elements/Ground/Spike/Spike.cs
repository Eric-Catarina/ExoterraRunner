using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    public GameObject spikeVFX;
    public static event Action OnSpikeHit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            OnSpikeHit?.Invoke();

            other.GetComponent<SmurfCatMovement>().Die();
            
        }
    }
    
}
