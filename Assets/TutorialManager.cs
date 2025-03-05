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
    public static event Action onFirstTutorialStarted;


    private const string TutorialCompletionKey = "TutorialCompletionCount";  // Chave para PlayerPrefs

    private void OnEnable()
    {
        // SmurfCatMovement.onPlayerJump += HandlePlayerJump;  // Se inscreve no evento de pulo do jogador
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
            // HideAllTutorials();
        }
        Invoke("ShowCurrentTutorial",2f);
    }

    private void HandlePlayerJump()
    {
        // Quando o jogador pular, avançamos para o próximo tutorial (se o primeiro pulo foi completado)
            Unpause();
            HideTutorialPanel();
            HideTutorial(0);
            Invoke("MoveToNextTutorial", 2f);
            // MoveToNextTutorial();
            SmurfCatMovement.onPlayerJump -= HandlePlayerJump;  // Desinscreve do evento de pulo do jogador
        
    }
    
    private void HandlePlayerHorizontalMovement()
    {
        SmurfCatMovement.onPlayerHorizontalSwipe -= HandlePlayerHorizontalMovement;
        Unpause();
        HideTutorialPanel();
        HideTutorial(1);
    }

    private void MoveToNextTutorial()
    {
        // Desativa o tutorial atual e avança para o próximo
        currentTutorialIndex++;
        Juice juice = tutorialObjects[currentTutorialIndex].GetComponent<Juice>();
        if (juice != null)
        {
            ShowCurrentTutorial();
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
        
        if (currentTutorialIndex == 0){
            SmurfCatMovement.onPlayerJump += HandlePlayerJump;  // Se inscreve no evento de pulo do jogador
            onFirstTutorialStarted?.Invoke();
        }

        if (currentTutorialIndex == 1) SmurfCatMovement.onPlayerHorizontalSwipe += HandlePlayerHorizontalMovement;  // Se inscreve no evento de pulo do jogador
        
        Juice juice = tutorialObjects[currentTutorialIndex].GetComponent<Juice>();
        if (juice != null)
        {
            juice.PlayActivationAnimation();
        }

        Pause();
    }

    private void HideTutorial(int index)
    {
        Juice juice = tutorialObjects[index].GetComponent<Juice>();
        if (juice != null)
        {
            juice.Deactivate();
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
