using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] tutorialObjects;
    private int currentTutorialIndex = 0;
    private bool firstJumpCompleted = false;
    private int tutorialCompletionCount = 0;  // Contador de quantas vezes o tutorial foi completado

    private const string TutorialCompletionKey = "TutorialCompletionCount";  // Chave para PlayerPrefs

    private void OnEnable()
    {
        SmurfCatMovement.onPlayerJump += NextTutorial;
    }

    private void OnDisable()
    {
        SmurfCatMovement.onPlayerJump -= NextTutorial;
    }

    private void Start()
    {
        // Recupera o número de vezes que o tutorial foi completado
        // tutorialCompletionCount = PlayerPrefs.GetInt(TutorialCompletionKey, 0); // Recupera o valor salvo anteriormente
        tutorialCompletionCount = 0;
    }

    void Update()
    {
        if (tutorialCompletionCount >= 3)  // Verifica se o tutorial já foi completado 3 vezes
        {
            // Se o tutorial foi completado 3 vezes, desabilita o tutorial
            HideAllTutorials();
            return;
        }

        for (int i = 0; i < tutorialObjects.Length; i++)
        {
            if (i == currentTutorialIndex)
            {
                TryGetComponent<Juice>(out Juice juice);
                if (juice != null)
                {
                    // Ativa o tutorial atual com animação
                    juice.PlayActivationAnimation();
                }
                else
                {
                    tutorialObjects[i].SetActive(true);
                }
            }
            else
            {
                TryGetComponent<Juice>(out Juice juice);
                if (juice != null)
                {
                    // Desativa o tutorial anterior com animação
                    juice.PlayDeactivationOrDestroyAnimation(() =>
                    {
                        NextTutorial();
                    });
                }
            }
        }
    }

    private void NextTutorial()
    {
        if (!firstJumpCompleted)
        {
            firstJumpCompleted = true;
            HideCurrentTutorial();

            currentTutorialIndex++;
            if (currentTutorialIndex >= tutorialObjects.Length)
            {
                currentTutorialIndex = tutorialObjects.Length - 1;
            }

            // Salva o progresso no PlayerPrefs
            tutorialCompletionCount++;
            PlayerPrefs.SetInt(TutorialCompletionKey, tutorialCompletionCount); // Salva no PlayerPrefs
            PlayerPrefs.Save();  // Garante que as alterações no PlayerPrefs sejam salvas
        }
    }

    private void HideCurrentTutorial()
    {
        Juice juice = tutorialObjects[currentTutorialIndex].GetComponent<Juice>();
        if (juice != null)
        {
            juice.Deactivate(() => {
                // Callback após a desativação da animação
                ActivateNextTutorial();
            });
        }
    }

    private void ActivateNextTutorial()
    {
        // Ativa o próximo tutorial com animação
        Juice juice = tutorialObjects[currentTutorialIndex].GetComponent<Juice>();
        if (juice != null)
        {
            juice.PlayActivationAnimation();
        }
    }

    private void HideAllTutorials()
    {
        foreach (var tutorial in tutorialObjects)
        {
            tutorial.SetActive(false);
        }
    }
}
