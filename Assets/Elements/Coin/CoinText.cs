using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private int coinCount = 0;
    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        Coin.OnCoinCollected += AddCoin;
    }
    private void OnEnable()
    {
    }
    
    public void AddCoin()
    {
        coinCount++;
        textMesh.text = coinCount.ToString();
    }

    private void OnDestroy()
    {
        Coin.OnCoinCollected -= AddCoin;
    }
}
