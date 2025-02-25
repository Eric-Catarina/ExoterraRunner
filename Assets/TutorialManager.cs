using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] tutorialObjects;
    private int currentTutorialIndex = 0;
    private bool firstJumpCompleted = false;  // Flag para garantir que o primeiro pulo avance para o próximo tutorial

    private void OnEnable()
    {
        SmurfCatMovement.onPlayerJump += NextTutorial;  // Se inscreve no evento de pulo
    }

    private void OnDisable()
    {
        SmurfCatMovement.onPlayerJump -= NextTutorial;  // Desinscreve do evento para evitar vazamento de memória
    }

    void Update()
    {
        for (int i = 0; i < tutorialObjects.Length; i++)
        {
            if (i == currentTutorialIndex)
            {
                // if have Juice Script play PlayActivationAnimation
                Juice juice = tutorialObjects[i].GetComponent<Juice>();
                if (juice != null)
                {
                    juice.PlayActivationAnimation();
                }
                else
                {
                    tutorialObjects[i].SetActive(true);
                }
                
            }
            else
            {
                Juice juice = tutorialObjects[i].GetComponent<Juice>();
                if (juice != null)
                {
                    juice.Deactivate();
                }
                tutorialObjects[i].SetActive(false);
            }
        }
    }

    private void NextTutorial()
    {
        // Só avança para o próximo tutorial se o primeiro pulo foi completado
        if (!firstJumpCompleted)
        {
            // Marca que o primeiro pulo foi completado
            firstJumpCompleted = true;

            // Avança para o próximo tutorial
            currentTutorialIndex++;

            // Impede que o índice ultrapasse o número de tutoriais
            if (currentTutorialIndex >= tutorialObjects.Length)
            {
                currentTutorialIndex = tutorialObjects.Length - 1;
            }
        }
    }
}