using System;
using TMPro;
using UnityEngine;

public class CoinText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private int coinCount;
    public bool isGems = false;

    private const string COIN_PREF_KEY = "CoinCount"; // Key for PlayerPrefs

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        if (isGems)
        {
            return;
        }

        // Load coin count from PlayerPrefs
        coinCount = PlayerPrefs.GetInt(COIN_PREF_KEY, 0);
        
        textMesh.text = coinCount.ToString();

        Coin.OnCoinCollected += AddCoin;
    }

    public void AddCoin()
    {
        
        if (isGems)
        {
            return;
        }
        coinCount++;
        textMesh.text = coinCount.ToString();

        // Save updated coin count to PlayerPrefs
        PlayerPrefs.SetInt(COIN_PREF_KEY, coinCount);
        PlayerPrefs.Save(); // Ensures the data is written to disk
    }

    private void OnDestroy()
    {
        Coin.OnCoinCollected -= AddCoin;
    }
}