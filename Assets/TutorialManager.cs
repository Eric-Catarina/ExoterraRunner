using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] tutorialObjects;
    public GameObject tutorialPanel;
    public PauseMenu pauseMenu;
    private int currentTutorialIndex = 0;
    private bool firstJumpCompleted = false;
    private int tutorialCompletionCount = 0;

    private const string TutorialCompletionKey = "TutorialCompletionCount";  // Chave para PlayerPrefs

    private void OnEnable()
    {
        SmurfCatMovement.onPlayerJump += HandlePlayerJump;  // Se inscreve no evento de pulo do jogador
    }

    private void OnDisable()
    {
        SmurfCatMovement.onPlayerJump -= HandlePlayerJump;  // Desinscreve do evento de pulo do jogador
        SmurfCatMovement.onPlayerHorizontalSwipe -= HandlePlayerHorizontalMovement;
    }

    private void Start()
    {
        // Carrega o número de vezes que o tutorial foi completado
        // tutorialCompletionCount = PlayerPrefs.GetInt(TutorialCompletionKey, 0);
        tutorialCompletionCount = 0;
        // Se o tutorial já foi completado 3 vezes, desativa todos os tutoriais.
        if (tutorialCompletionCount >= 3)
        {
            HideAllTutorials();
        }
        Invoke("ShowCurrentTutorial",2f);
    }

    private void HandlePlayerJump()
    {
        // Quando o jogador pular, avançamos para o próximo tutorial (se o primeiro pulo foi completado)
            firstJumpCompleted = true;
            Unpause();
            HideTutorialPanel();
            HideAllTutorials();
            Invoke("MoveToNextTutorial", 2f);
            // MoveToNextTutorial();
            SmurfCatMovement.onPlayerJump -= HandlePlayerJump;  // Desinscreve do evento de pulo do jogador
        
    }
    
    private void HandlePlayerHorizontalMovement()
    {
        SmurfCatMovement.onPlayerHorizontalSwipe -= HandlePlayerHorizontalMovement;
        MoveToNextTutorial();
    }

    private void MoveToNextTutorial()
    {
        // Desativa o tutorial atual e avança para o próximo
        Juice juice = tutorialObjects[currentTutorialIndex].GetComponent<Juice>();
        if (juice != null)
        {
            juice.PlayDeactivationOrDestroyAnimation(() =>
            {
                Debug.Log("Desativando tutorial");
                // Avança para o próximo tutorial após a animação de desativação
                currentTutorialIndex++;
                ShowCurrentTutorial();
            });
        }

        // Salva o progresso do tutorial no PlayerPrefs
        tutorialCompletionCount++;
        PlayerPrefs.SetInt(TutorialCompletionKey, tutorialCompletionCount);
        PlayerPrefs.Save(); // Garante que o progresso seja salvo
    }

    private void ShowCurrentTutorial()
    {
        // Exibe o tutorial atual com animação de ativação
        if (currentTutorialIndex >= tutorialObjects.Length) return;
        ShowTutorialPanel();
        if (currentTutorialIndex == 1) SmurfCatMovement.onPlayerHorizontalSwipe += HandlePlayerHorizontalMovement;  // Se inscreve no evento de pulo do jogador


        Juice juice = tutorialObjects[currentTutorialIndex].GetComponent<Juice>();
        if (juice != null)
        {
            juice.PlayActivationAnimation();
            Pause();
        }
        else
        {
            // tutorialObjects[currentTutorialIndex].SetActive(true);
        }
    }

    private void HideAllTutorials()
    {
        // Desativa todos os tutoriais
        foreach (var tutorial in tutorialObjects)
        {
            tutorial.GetComponent<Juice>().Deactivate();
        }
    }
    
    // disable movefoward until player jumps
    private void Pause()
    {
        pauseMenu.PauseGame();
    }
    private void Unpause()
    {
        pauseMenu.UnpauseGame();
    }
    
    private void ShowTutorialPanel()
    {
        tutorialPanel.GetComponent<Juice>().PlayActivationAnimation();
    }
    
    private void HideTutorialPanel()
    {
        
        tutorialPanel.GetComponent<Juice>().Deactivate();
        
    }
    
}
