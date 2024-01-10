using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sugar : MonoBehaviour
{
   public static event Action OnSugarCollected;
    public GameObject sugarVFX, sugarContainer;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            OnSugarCollected?.Invoke();
            GameObject sugarVFXInstance = Instantiate(sugarVFX, transform.position, Quaternion.identity);
            // ReparentVfx(sugarVFXInstance);
            Destroy(sugarVFXInstance, 1f);
            Destroy(gameObject);
        }
    }
    private void ReparentVfx(GameObject vfx)
    {
        vfx.transform.parent = sugarContainer.transform;
    }
}
