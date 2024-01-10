using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    [SerializeField] private GameObject levelEndOverlay;
    public void EndLevel()
    {
        levelEndOverlay.SetActive(true);
        Time.timeScale = 0;
        UnityInterstitialAd.Instace.ShowAd();

    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision");
        if (collision.gameObject.tag == "Character")
        {
            EndLevel();
        }
    }
}
