using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Juice : MonoBehaviour
{
    [Header("Scale Animation Settings")]
    public bool animateScale = false;
    public float scaleMultiplier = 1.5f;       // Multiplicador para a escala final
    public float scaleDuration = 0.5f;
    public Ease scaleEase = Ease.OutBack;
    public bool waitForOneFrame = false;       // Espera um frame antes de iniciar a animação de escala
    private Vector3 baseScale;                 // Armazena a escala original do objeto
    private bool scaleAnimationStarted = false;

    [Header("Rotation Animation Settings")]
    public bool animateRotation = false;
    public Vector3 rotationAxis = new Vector3(0, 360, 0); // Gira no eixo Y por padrão
    public float rotationDuration = 2f;

    [Header("Vertical Bounce Animation Settings")]
    public bool animateVerticalBounce = false;
    public float bounceHeight = 0.5f;          // Altura do movimento de subida/descida
    public float bounceDuration = 1f;
    public Ease bounceEase = Ease.InOutSine;

    [Header("Rainbow Mode Settings")]
    public bool rainbowMode = false;           // Ativa/desativa o modo arco-íris
    public float rainbowSpeed = 5f;            // Velocidade da transição rápida de cores

    private Material material;
    private MaterialPropertyBlock propBlock;
    private Renderer objectRenderer;

    private void Start()
    {
        baseScale = transform.localScale; // Armazena a escala original do objeto

        // Configuração para manipulação de cor no Rainbow Mode
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            material = objectRenderer.material;
            propBlock = new MaterialPropertyBlock();
        }

        // Inicia as animações configuradas
        if (animateScale)
        {
            StartCoroutine(CheckAndPlayScaleAnimation());
        }

        if (animateRotation)
        {
            PlayRotationAnimation();
        }

        if (animateVerticalBounce)
        {
            PlayVerticalBounceAnimation();
        }
    }

    private void Update()
    {
        // Inicia o modo arco-íris se habilitado
        if (rainbowMode && objectRenderer != null)
        {
            ApplyRainbowEffect();
        }
    }

    private IEnumerator CheckAndPlayScaleAnimation()
    {
        if (waitForOneFrame)
        {
            yield return null; // Espera um frame
        }

        // Aguarda até que o jogo seja despausado
        while (Time.timeScale == 0)
        {
            yield return null;
        }

        // Só inicia a animação de escala se ela ainda não foi iniciada
        if (!scaleAnimationStarted)
        {
            scaleAnimationStarted = true;
            PlayScaleAnimation();
        }
    }

    private void PlayScaleAnimation()
    {
        Vector3 targetScale = baseScale * scaleMultiplier; // Define a escala final baseada na escala original
        transform.localScale = Vector3.zero; // Começa invisível para um efeito de aparição
        transform.DOScale(targetScale, scaleDuration).SetEase(scaleEase);
    }

    private void PlayRotationAnimation()
    {
        transform.DORotate(rotationAxis, rotationDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental); // Gira continuamente
    }

    private void PlayVerticalBounceAnimation()
    {
        float startY = transform.position.y;
        transform.DOMoveY(startY + bounceHeight, bounceDuration)
            .SetEase(bounceEase)
            .SetLoops(-1, LoopType.Yoyo); // Sobe e desce continuamente
    }

    private void ApplyRainbowEffect()
    {
        // Gera um valor aleatório para o matiz (hue) que muda rapidamente
        float hue = Mathf.Repeat(Time.time * rainbowSpeed, 1);
        Color rainbowColor = Color.HSVToRGB(hue, 1, 1); // Cores vibrantes

        objectRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", rainbowColor);
        objectRenderer.SetPropertyBlock(propBlock);
    }
}
