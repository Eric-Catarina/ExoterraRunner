using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class PauseMenu : MonoBehaviour
{

    [SerializeField] private Button SFXButton;
    [SerializeField] private TextMeshProUGUI SFXText;
    [SerializeField] private AudioManager audioManager;
    private bool sfxOn = true;

    void Awake()
    {
        }
    public void PauseGame()
    {
        Time.timeScale = 0;
    }
    public void UnpauseGame()
    {
        Time.timeScale = 1;
    }
    public void Home()
    {
        UnpauseGame();
        SceneManager.LoadSceneAsync(0);
    }

    public void Restart()
    {
        UnpauseGame();
        RestartScene();
    }
    
    public static void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TurnSFX()
    {
        
        sfxOn = !sfxOn;
        PlayerPrefs.SetInt("SFX", sfxOn ? 1 : 0);
        if (sfxOn)
        {
            TurnOnSound();
        }
        else
        {
            TurnOffSound();
        }
    }
    public void TurnOnSound()
    {
        SFXButton.GetComponent<Image>().color = Color.white;
        SFXText.text = "ON";
        audioManager.UnmuteMaster();
    }
    public void TurnOffSound()
    {
        SFXButton.GetComponent<Image>().color = Color.gray;
        SFXText.text = "OFF";
        audioManager.MuteMaster();
    }
    public void Initialize()
    {
        sfxOn = PlayerPrefs.GetInt("SFX", 1) == 1;
        if (sfxOn)
        {
            TurnOnSound();
        }
        else
        {
            TurnOffSound();
        }

        audioManager = GameObject.FindWithTag("Audio").GetComponent<AudioManager>();
    }

}
