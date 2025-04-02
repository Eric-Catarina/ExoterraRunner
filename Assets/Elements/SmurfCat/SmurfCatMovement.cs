using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SmurfCatMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float horizontalSpeed = 2.0f;
    public float jumpStrength = 10.0f;
    public float maxHorizontalSpeed = 15.0f;
    private bool isFallingHighSpeed = false;
    private Animator animator;

    [Header("Game Objects")]
    public GameObject loseScreen;
    public GameObject fallingVFX;
    public GameObject groundScorchVFX;
    public GameObject fallExplosionVFX;
    public GameObject jumpSpotText;
    public GameObject grounds;
    public GameObject baseGround;
    public GameObject tutorials;
    public List<TrailRenderer> fallingTrails;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreMultiplierText;
    // public TextMeshProUGUI highScoreText;
    [FormerlySerializedAs("fadeInDuration")] [SerializeField] private float fallingVfxFadeInDuration;
    [FormerlySerializedAs("fadeOutDuration")] [SerializeField] private float fallingVfxFadeOutDuration;
    public GameObject revivePanel;
    public GameObject halo;
    
    [Header("Hitstop Settings")]
    public float minHitstopDuration = 0.05f;
    public float maxHitstopDuration = 0.3f;
    public float minHitstopIntensity = 0.2f;
    public float maxHitstopIntensity = 0.0f;
    public float maxVelocityForScaling = 120f;
    
    public static event Action onHighFallSpeed;
    public static event Action onGroundImpact;
    public static event Action onPlayerJump;
    public static event Action onPlayerHorizontalSwipe;



    [Header("Dependencies")]
    public LevelEnd levelEnd;
    public Generator generator;
    public CameraController cameraController;

    [Header("Physics Settings")]
    public Rigidbody rb;
    public float maxYSpeed = -20.0f;
    public GameObject adsManager;
    public MoveForward moveForward;

    private float scoreMultiplier = 1.0f;
    private float currentScore = 0.0f;
    private float highScore = 0.0f;
    private Vector3 targetVelocity;
    public bool isGrounded = false;
    private bool isOnJumpSpot = false;
    private bool hadHighFallSpeed = false;
    private bool isDead = false;
    private bool isHitstopActive = false;

    private PlayerInput playerInput;
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool isSwipeDetected = false;
    
    private AudioManager audioManager;
    private bool _isFallingFXActive;
    private bool _isImmortal = false;
    [SerializeField] private bool _canAlrealdyJump = false;


    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        LoadHighScore();
    }
    

    private void OnEnable()
    {
        playerInput.actions.Enable();
        TutorialManager.onFirstTutorialStarted += OnFirstTutorialStarted;
        void OnFirstTutorialStarted()
        {
            _canAlrealdyJump = true;
            TutorialManager.onFirstTutorialStarted -= OnFirstTutorialStarted;
        }
    }

    private void OnDisable()
    {
        playerInput.actions.Disable();
        
    }

    private void FixedUpdate()
    {
        HandleMovement();
        UpdateScore();
        CheckFallingState();
    }
    

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(Tags.Ground))
        {
            HandleGroundCollision(collision);
        }
    }

    // private void OnCollisionStay(Collision other)
    // {
    //     if (other.gameObject.CompareTag(Tags.Ground))
    //     {
    //         return;
    //         isGrounded = true;
    //     }
    // }
    // private void OnCollisionExit(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag(Tags.Ground))
    //     {
    //         isGrounded = false;
    //     }
    // }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.GroundEnd))
        {
            generator.Generate();
            cameraController.SetAirborne(true);
            OnGroundEnd?.Invoke();
            
        }
        else if (other.CompareTag(Tags.JumpSpot))
        {
            isOnJumpSpot = true;
        }
    }

    public UnityEvent OnGroundEnd;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Tags.JumpSpot))
        {
            isOnJumpSpot = false;
        }
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        audioManager = FindObjectOfType<AudioManager>();
        animator = GetComponent<Animator>();
        horizontalSpeed = PlayerPrefs.GetFloat(PlayerPrefsKeys.MovementSensitivity, 2.5f);


    }

    private void LoadHighScore()
    {
        return;
        highScore = PlayerPrefs.GetFloat(PlayerPrefsKeys.HighScore, 0);
        // highScoreText.text = highScore.ToString("F0");
    }

    #endregion

    #region Movement

   public void MoveHorizontally(InputAction.CallbackContext context)
{
    if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Canceled)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        // Aqui, em vez de multiplicar diretamente por horizontalSpeed, calcule a velocidade com base no movimento
        float moveDeltaX = moveInput.x * horizontalSpeed;  // Aqui, moveInput.x representa a distância percorrida
        targetVelocity = new Vector3(moveDeltaX, rb.velocity.y, rb.velocity.z);
        if (Math.Abs(moveInput.x) > 15f) onPlayerHorizontalSwipe?.Invoke();

        // Agora aplicamos a limitação na velocidade para garantir que ela não ultrapasse o máximo
        // targetVelocity.x = Mathf.Clamp(targetVelocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);
    }
}

private void HandleMovement()
{
    Vector3 newVelocity = new Vector3(targetVelocity.x, rb.velocity.y, rb.velocity.z);
    
    // Movimentação suave entre a velocidade atual e a nova velocidade com base na entrada
    rb.velocity = !isDead
        ? Vector3.Lerp(rb.velocity, newVelocity, horizontalSpeed * Time.fixedDeltaTime)
        : Vector3.zero;

    
    // Verificação para morte do personagem se a velocidade no eixo Y for muito negativa
    if (rb.velocity.y < -maxYSpeed)
    {
        if (isDead) return;
        isDead = true;
        cameraController.OnPlayerDeath();
        SaveHighScore();
        ShowLevelEnd();
        UnityInterstitialAd.Instace.LoadAd();

    }
}

public void SetMovementSensitivity(float sensitivity)
{
    horizontalSpeed = sensitivity;
    PlayerPrefs.SetFloat(PlayerPrefsKeys.MovementSensitivity, sensitivity);
}

    #endregion

    #region Jump

    public void OnSwipe(InputAction.CallbackContext context)
    {
        if (isDead) return;
        if (IsPointerOverUI() || !isGrounded) return;
        if (context.phase == InputActionPhase.Started)
        {
            // Registra o início do toque
            startTouchPosition = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Performed)
        {
            // Registra o fim do toque
            endTouchPosition = context.ReadValue<Vector2>();
            DetectSwipe();
        }
    }
    private void DetectSwipe()
    {
        Vector2 swipeDelta = endTouchPosition;

        // Verifica se o swipe é significativo
        if (swipeDelta.magnitude > 15f) // Ajuste o valor conforme necessário
        {

            float verticalSwipe = Mathf.Abs(swipeDelta.y);
            float horizontalSwipe = Mathf.Abs(swipeDelta.x);

            if (verticalSwipe > horizontalSwipe && swipeDelta.y > 0)
            {
                // Swipe para cima detectado
                isSwipeDetected = true;
                PerformJump();
                isSwipeDetected = false;
            }
        }
    }
    

    public void Jump()
    {
        return;
        if (IsPointerOverUI() || !isGrounded) return;

        PerformJump();

        if (isOnJumpSpot)
        {
            ProcessJumpSpot();
        }
    }

    private void PerformJump()
    {
        if (!_canAlrealdyJump) return;
        if (isDead) return;
        if (!isGrounded) return;
        
        onPlayerJump?.Invoke();

        isGrounded = false;
        rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
        audioManager.PlayJumpSound();
        if (isOnJumpSpot)
        {
            animator.SetTrigger("JumpRoll");

            ProcessJumpSpot();
        }
        else
        {
            animator.SetTrigger("Jump");
        }
    }

    private void ProcessJumpSpot()
    {
        currentScore += 10 * scoreMultiplier;
        scoreMultiplier *= 1.1f;
        ShowJumpSpotText(10 * scoreMultiplier);
    }

    private void ShowJumpSpotText(float score)
    {
        jumpSpotText.SetActive(true);
        jumpSpotText.GetComponent<TextMeshProUGUI>().text = $"Jump Spot! +{score:F0}";
        StartCoroutine(HideJumpSpotText());
    }

    private IEnumerator HideJumpSpotText()
    {
        yield return new WaitForSeconds(2f);
        jumpSpotText.GetComponent<Juice>().Deactivate();
    }

    #endregion

    #region Score Management

    private void UpdateScore()
    {
        if (isDead) return;
        currentScore += scoreMultiplier * Time.fixedDeltaTime * 2;
        scoreMultiplier += Time.fixedDeltaTime / 150;

        scoreText.text = Mathf.FloorToInt(currentScore).ToString();
        scoreMultiplierText.text = $"x{scoreMultiplier:F2}";
    }

    private void SaveHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetFloat(PlayerPrefsKeys.HighScore, highScore);
        }
    }

    #endregion

    #region Ground Collision

    private void HandleGroundCollision(Collision collision)
    {
        isGrounded = true;
        cameraController.SetAirborne(false);

        if (hadHighFallSpeed)
        {
            ProcessHighFallImpact(collision);
        }

        hadHighFallSpeed = false;
    }

    private void ProcessHighFallImpact(Collision collision)
    {
        SpawnCoinsOnImpact(collision);
        TriggerGroundExplosion();
        PlayImpactAudio();
        TriggerHitstopEffect();
        animator.SetBool("Falling", false);
        animator.SetTrigger("HeroLand");

        isFallingHighSpeed = false;
        Handheld.Vibrate();
        onGroundImpact?.Invoke();
    }

    private void SpawnCoinsOnImpact(Collision collision)
    {
        var spawnCoins = collision.gameObject.GetComponent<SpawnCoins>();
        spawnCoins?.StartCoroutine("DelayedSpawn");
    }

    private void TriggerGroundExplosion()
    {
        var explosion = Instantiate(fallExplosionVFX, transform.position, Quaternion.identity);
        explosion.transform.SetParent(grounds.transform);
        explosion.transform.position += Vector3.up * 2.5f;
        Destroy(explosion, 3.3f);
        
        var groundExplosion = Instantiate(groundScorchVFX, transform.position, Quaternion.identity);
        groundExplosion.transform.SetParent(grounds.transform);
        groundExplosion.transform.position += Vector3.up * 1.5f;
        Destroy(groundExplosion, 3.3f);
    }

    private void PlayImpactAudio()
    {
        audioManager.StopFallingAudio();
        audioManager.PlayRandomImpactSound();
        Handheld.Vibrate();
    }

    #endregion

    #region Falling State

    private void EnteredHighFallSpeed()
    {
        if (isFallingHighSpeed) return;
        onHighFallSpeed?.Invoke();
    }
    private void CheckFallingState()
    {
        if (rb.velocity.y < -35)
        {
            EnteredHighFallSpeed();
            isFallingHighSpeed = true;
            fallingVFX.SetActive(true);
            hadHighFallSpeed = true;
            audioManager.PlayFallingAudio();
            animator.SetBool("Falling", true);
            ActivateFallingTrails();
        }
        else
        {
            isFallingHighSpeed = false;
            fallingVFX.SetActive(false);
            DeactivateFallingTrails();
        }
    }
    private void ActivateFallingVFX()
    {
        // Se o GO estiver desativado, ative-o antes de iniciar o fade.
        fallingVFX.SetActive(true);

        CanvasGroup cg = fallingVFX.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = fallingVFX.AddComponent<CanvasGroup>();
        }

        cg.alpha = 0f;  // Começa invisível
        cg.DOKill();    // Cancela qualquer tween anterior, caso esteja rolando

        // Fade in
        cg.DOFade(1f, fallingVfxFadeInDuration).SetEase(Ease.OutQuad);

        _isFallingFXActive = true;
    }

    private void DeactivateFallingVFX()
    {
        CanvasGroup cg = fallingVFX.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            // Se não existir, apenas desativa sem transição
            fallingVFX.SetActive(false);
            _isFallingFXActive = false;
            return;
        }

        cg.DOKill();

        // Fade out bem rápido
        cg.DOFade(0f, fallingVfxFadeOutDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                // Depois do fade out, desativa GameObject
                fallingVFX.SetActive(false);
                _isFallingFXActive = false;
            });
    }
    
    private void ActivateFallingTrails()
    {
        fallingTrails.ForEach(trail => trail.emitting = true);
    }
    private void DeactivateFallingTrails()
    {
        fallingTrails.ForEach(trail => trail.emitting = false);
    }

    #endregion

    #region Hitstop Effect

    private void TriggerHitstopEffect()
    {
        float impactVelocity = Mathf.Abs(rb.velocity.y);
        float scaledDuration = Mathf.Lerp(minHitstopDuration, maxHitstopDuration, impactVelocity / maxVelocityForScaling);
        float scaledIntensity = Mathf.Lerp(minHitstopIntensity, maxHitstopIntensity, impactVelocity / maxVelocityForScaling);
        StartCoroutine(HitstopCoroutine(scaledDuration, scaledIntensity));
    }

    private IEnumerator HitstopCoroutine(float duration, float intensity)
    {
        if (isHitstopActive) yield break;

        isHitstopActive = true;
        Time.timeScale = intensity;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1.0f;
        isHitstopActive = false;
    }

    #endregion

    #region Death

    // ReSharper disable Unity.PerformanceAnalysis
    public void Die()
    {
        if (_isImmortal) return;
        isDead = true;
        animator.SetBool("IsDead", true);
        moveForward.enabled = false;

        if (Random.Range(0, 2) == 0)
        {
            animator.SetTrigger("DieNMelt");
        }
        else
        {
            animator.SetTrigger("Die");
        }
        cameraController.OnPlayerDeath();
        SaveHighScore();
        
        // ShowRevivePanel();
        
        // Play Interstitial Ad
        UnityInterstitialAd.Instace.LoadAd();
    }
    
    public void Revive()
    {
        _isImmortal = true;
        Invoke("SetIsImmortalFalse", 5f);
        isDead = false;
        animator.SetBool("IsDead", false);

        moveForward.enabled = true;
        animator.SetTrigger("Jump");
        cameraController.OnRevive();
      
        
        //  Check if the halo parent is self, if not, set it to self
        if (halo.transform.parent != transform)
        {
            halo.transform.parent = transform;
        }
        ShowAndHideHalo();
        OnRevive.Invoke();
    }
    
    // Set isImmortal false after 5 seconds
    private void SetIsImmortalFalse()
    {
        _isImmortal = false;
    }
    
    public void ShowRevivePanel()
    {
        revivePanel.SetActive(true);
    }
    
    // Show the end level panel
    public void ShowLevelEnd()
    {
        levelEnd.EndLevel();
    }
    
    private void HideHalo()
    {
        halo.transform.DOLocalMoveY(10f, .5f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            halo.SetActive(false);
        });
    }
    
    private void ShowAndHideHalo()
    {
        // Set halo position to slightly above the player
        halo.transform.position = transform.position + Vector3.up * 8f;
        // Tween the halo appearing above player head
        halo.transform.DOLocalMoveY(4f, 2f).SetEase(Ease.OutBack);
        halo.SetActive(true);
        Invoke("HideHalo",3f);
    }
    
    public UnityEvent OnRevive;

    #endregion

    #region Utility

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    #endregion

    private static class Tags
    {
        public const string Ground = "Ground";
        public const string GroundEnd = "GroundEnd";
        public const string JumpSpot = "JumpSpot";
    }

    private static class PlayerPrefsKeys
    {
        public const string HighScore = "HighScore";
        public const string MovementSensitivity = "MovementSensitivity";

    }
}
