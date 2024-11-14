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
    public bool waitForOneFrame = false;       // Waits one frame before starting scale animation
    private Vector3 baseScale;                 // Stores the original scale of the object
    private bool scaleAnimationStarted = false;

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

    [Header("Text Animation Settings")]
    public bool animateText = false;           // Activates text animation settings
    public TextMeshProUGUI textToAnimate;      // Reference to the TextMeshPro component
    public bool textPulseEffect = true;        // Enables "pulse" scaling effect for text
    public bool textVerticalBounce = false;    // Enables vertical bounce for text
    public float textScaleMultiplier = 1.05f;  // Pulse effect scale multiplier
    public float textBounceHeight = 10f;       // Height for vertical bounce effect
    public float textAnimationDuration = 0.8f; // Duration for each text animation loop
    public Ease textAnimationEase = Ease.InOutQuad;

    private Material material;
    private MaterialPropertyBlock propBlock;
    private Renderer objectRenderer;

    private void Start()
    {
        baseScale = transform.localScale; // Stores the original scale of the object

        // Setup for color manipulation in Rainbow Mode
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            material = objectRenderer.material;
            propBlock = new MaterialPropertyBlock();
        }

        // Starts configured animations
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

        if (animateText && textToAnimate != null)
        {
            AnimateTextLoop();
        }
    }

    private void Update()
    {
        // Starts rainbow effect if enabled
        if (rainbowMode && objectRenderer != null)
        {
            ApplyRainbowEffect();
        }
    }

    private IEnumerator CheckAndPlayScaleAnimation()
    {
        if (waitForOneFrame)
        {
            yield return null; // Waits one frame
        }

        // Waits until the game is unpaused
        while (Time.timeScale == 0)
        {
            yield return null;
        }

        // Ensures the scale animation only starts once
        if (!scaleAnimationStarted)
        {
            scaleAnimationStarted = true;
            PlayScaleAnimation();
        }
    }

    private void PlayScaleAnimation()
    {
        Vector3 targetScale = baseScale * scaleMultiplier; // Final scale based on original scale
        transform.localScale = Vector3.zero; // Starts invisible for a "pop-in" effect
        transform.DOScale(targetScale, scaleDuration).SetEase(scaleEase).SetUpdate(true);
    }

    private void PlayRotationAnimation()
    {
        transform.DORotate(rotationAxis, rotationDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental)
            .SetUpdate(true); // Continuous rotation, even if timeScale is 0
    }

    private void PlayVerticalBounceAnimation()
    {
        float startY = transform.position.y;
        transform.DOMoveY(startY + bounceHeight, bounceDuration)
            .SetEase(bounceEase)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true); // Continuous vertical bounce, even if timeScale is 0
    }

    private void AnimateTextLoop()
    {
        if (textPulseEffect)
        {
            // Pulse effect for text (scale animation)
            textToAnimate.transform.localScale = Vector3.one * (1 / textScaleMultiplier); // Start slightly smaller
            textToAnimate.transform.DOScale(textScaleMultiplier, textAnimationDuration)
                .SetEase(textAnimationEase)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true); // Continuous pulse
        }

        if (textVerticalBounce)
        {
            // Vertical bounce for text
            float startY = textToAnimate.transform.localPosition.y;
            textToAnimate.transform.DOLocalMoveY(startY + textBounceHeight, textAnimationDuration)
                .SetEase(textAnimationEase)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true); // Continuous vertical bounce
        }
    }

    private void ApplyRainbowEffect()
    {
        // Generates a quickly-changing hue for rainbow effect
        float hue = Mathf.Repeat(Time.time * rainbowSpeed, 1);
        Color rainbowColor = Color.HSVToRGB(hue, 1, 1); // Vibrant colors

        objectRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", rainbowColor);
        objectRenderer.SetPropertyBlock(propBlock);
    }
}
