using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;

public class Juice : MonoBehaviour
{
    [Header("Scale Animation Settings")]
    public bool animateScale = false;
    public float scaleMultiplier = 1.5f;       // Scale multiplier for the initial effect
    public float scaleDuration = 0.5f;
    public Ease scaleEase = Ease.OutBack;
    public bool shouldAnimateWhilePaused = true;     // Continue animation even when Time.timeScale = 0
    private Tween activationTween; // Armazena a referência ao tween de ativação
    private Vector3 baseScale;                 // Stores the original scale of the object

    [Header("Deactivation/Destruction Animation Settings")]
    public bool animateOnDeactivateOrDestroy = true;
    public float deactivationScaleDuration = 0.5f; // Duration of the deactivation scale animation
    public Ease deactivationEase = Ease.InBack;
    public float fadeOutDuration = 0.5f;          // Duration for fade-out effect

    [Header("Rotation Animation Settings")]
    public bool animateRotation = false;
    public Vector3 rotationAxis = new Vector3(0, 360, 0); // Rotates around the Y-axis by default
    public float rotationDuration = 2f;

    [Header("Vertical Bounce Animation Settings")]
    public bool animateVerticalBounce = false;
    public float bounceHeight = 0.5f;          // Height of the bounce
    public float bounceDuration = 1f;
    public Ease bounceEase = Ease.InOutSine;

    [Header("Rainbow Mode Settings")]
    public bool rainbowMode = false;           // Activates rainbow color cycling mode
    public float rainbowSpeed = 5f;            // Speed of color transition

    private Material material;
    private MaterialPropertyBlock propBlock;
    private Renderer objectRenderer;

    private void Awake()
    {
        baseScale = transform.localScale; // Store the original scale
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            material = objectRenderer.material;
            propBlock = new MaterialPropertyBlock();
        }
    }

    private void OnEnable()
    {
        PlayActivationAnimation();

    }

    private void Update()
    {
        if (rainbowMode && objectRenderer != null)
        {
            ApplyRainbowEffect();
        }
    }

    public void PlayActivationAnimation()
    {

        // Só aplica a animação se o jogo não estiver pausado
        if (Time.timeScale == 0)
        {
            // Se o jogo estiver pausado, apenas assegura que o objeto está ativo, mas sem alterar a escala.
            gameObject.SetActive(true);
            return;
        }

        gameObject.SetActive(true);

        if (animateRotation)
            PlayRotationAnimation();

        if (animateVerticalBounce)
            PlayVerticalBounceAnimation();

        if (!animateScale) return;

        // Salve a escala original antes de começar a animação
        Vector3 originalScale = transform.localScale;

        // Se o jogo não está pausado, comece com a escala zero para o efeito de "pop-in"
        transform.localScale = Vector3.zero;

        // Animação suave para aumentar a escala até o tamanho desejado
        activationTween = transform.DOScale(baseScale * scaleMultiplier, scaleDuration)
            .SetEase(scaleEase);
    }




    public void PlayDeactivationOrDestroyAnimation(System.Action onComplete)
    {
        Debug.Log("PlayDeactivationOrDestroyAnimation");
        if (!animateOnDeactivateOrDestroy)
        {
            onComplete?.Invoke();
            return;
        }

        // Scale down animation
        transform.DOScale(Vector3.zero, deactivationScaleDuration)
            .SetEase(deactivationEase)
            .SetUpdate(shouldAnimateWhilePaused) // Ensures animation runs even when timeScale = 0
            .OnComplete(() =>
            {
                onComplete?.Invoke();
                gameObject.SetActive(false);
            });

        // Optional: Fade out if material allows
        if (objectRenderer != null)
        {
            FadeOutEffect();
        }
    }

    private void FadeOutEffect()
    {
        if (material == null || !material.HasProperty("_Color")) return;

        Color startColor = material.color;
        Color endColor = startColor;
        endColor.a = 0; // Fade to transparent

        material.DOColor(endColor, fadeOutDuration)
            .SetUpdate(shouldAnimateWhilePaused) // Ensures fade-out runs even when timeScale = 0
            .OnComplete(() =>
            {
                material.color = startColor; // Reset color for next activation
            });
    }

    private void PlayRotationAnimation()
    {
        transform.DORotate(rotationAxis, rotationDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental)
            .SetUpdate(shouldAnimateWhilePaused); // Ensures animation runs even when timeScale = 0
    }

    private void PlayVerticalBounceAnimation()
    {
        float startY = transform.position.y;
        transform.DOMoveY(startY + bounceHeight, bounceDuration)
            .SetEase(bounceEase)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(shouldAnimateWhilePaused); // Ensures animation runs even when timeScale = 0
    }

    private void ApplyRainbowEffect()
    {
        float hue = Mathf.Repeat(Time.time * rainbowSpeed, 1);
        Color rainbowColor = Color.HSVToRGB(hue, 1, 1);

        objectRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", rainbowColor);
        objectRenderer.SetPropertyBlock(propBlock);
    }

    public void Deactivate(System.Action onComplete = null) 
    {
        
        if (activationTween != null && activationTween.IsActive())
        {
            activationTween.Kill(); // Cancela a animação de ativação
            activationTween = null; // Reseta a variável
        }
        
        PlayDeactivationOrDestroyAnimation(() =>
        {
            onComplete?.Invoke(); 
        });
        return;
    }

    public void DestroyWithAnimation()
    {
        PlayDeactivationOrDestroyAnimation(() => Destroy(gameObject));
    }
}
