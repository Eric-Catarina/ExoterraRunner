using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    [SerializeField] private GameObject levelEndOverlay;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI levelEndText;

    private readonly List<string> phrases = new List<string>
    {
        "Exploration Failed!",
        "Rest in orbit.",
        "Gravity won this time.",
        "Calling for backup...",
        "What an epic fall!",
        "Game over... but the universe remembers you.",
        "Out of fuel for another try?",
        "Ran too far, flew too little.",
        "The planet thanks you for your sacrifice.",
        "Fell with style!",
        "Explorer down in action.",
        "Luck left you... for now.",
        "Back to square one.",
        "Planetary collision detected.",
        "Gotta fly to survive.",
        "Exotic attempt failed.",
        "Nothing like an otherworldly fall!",
        "The stars didn’t save you this time.",
        "RIP - Rest In Planetary dust",
        "If the platforms could talk...",
        "A leap of courage... almost!",
        "Intergalactic fate: failure.",
        "Gravity did its job.",
        "Rest in stardust.",
        "You hit the zero point!",
        "Your last flight was legendary.",
        "Ran out of fuel... or luck.",
        "The beauty of the cosmos... and the fall!",
        "Nothing like gravity to teach you a lesson.",
        "Another chance to challenge the planets?"
    };

    public void Start()
    {
        EndLevel();
    }

    public void EndLevel()
    {
        // levelEndOverlay.SetActive(true);

        // Escolhe uma frase aleatória e aplica ao texto
        int randomIndex = Random.Range(0, phrases.Count);
        levelEndText.text = phrases[randomIndex];

        // Animação de entrada para o overlay
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 1f);

        levelEndOverlay.transform.localScale = Vector3.zero;
        levelEndOverlay.transform.DOScale(1, 1f).SetEase(Ease.OutBack);

        // Aplica animação em loop no texto
        AnimateTextLoop();
    }

    private void AnimateTextLoop()
    {
        // Define a posição inicial e a escala do texto
        levelEndText.transform.localScale = Vector3.one * 0.9f;
        levelEndText.transform.localPosition = new Vector3(levelEndText.transform.localPosition.x, levelEndText.transform.localPosition.y - 10, levelEndText.transform.localPosition.z);

        // Anima o texto com efeito de "pulse" e movimento vertical
        levelEndText.transform.DOScale(1.05f, 0.8f)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo); // Loop infinito para efeito de "pulse"

        levelEndText.transform.DOLocalMoveY(levelEndText.transform.localPosition.y + 10, 0.8f)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo); // Loop infinito para efeito de movimento suave
    }
}
