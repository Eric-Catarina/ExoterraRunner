using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SmurfCatMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float horizontalSpeed = 2.0f;
    public float jumpStrength = 10.0f;
    public float maxHorizontalSpeed = 15.0f;

    [Header("Game Objects")]
    public GameObject loseScreen;
    public GameObject fallingVFX;
    public GameObject fallExplosionVFX;
    public GameObject jumpSpotText;
    public GameObject grounds;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreMultiplierText;
    public TextMeshProUGUI highScoreText;

    [Header("Hitstop Settings")]
    public float minHitstopDuration = 0.05f;
    public float maxHitstopDuration = 0.3f;
    public float minHitstopIntensity = 0.2f;
    public float maxHitstopIntensity = 0.0f;
    public float maxVelocityForScaling = 120f;

    [Header("Dependencies")]
    public LevelEnd levelEnd;
    public Generator generator;
    public CameraController cameraController;

    [Header("Physics Settings")]
    public Rigidbody rb;
    public float maxYSpeed = -20.0f;
    public GameObject adsManager;

    private float scoreMultiplier = 1.0f;
    private float currentScore = 0.0f;
    private float highScore = 0.0f;
    private Vector3 targetVelocity;
    private bool isGrounded = false;
    private bool isOnJumpSpot = false;
    private bool hadHighFallSpeed = false;
    private bool isDead = false;
    private bool isHitstopActive = false;

    private PlayerInput playerInput;
    private AudioManager audioManager;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        LoadHighScore();
    }

    private void OnEnable()
    {
        playerInput.actions.Enable();
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

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag(Tags.Ground))
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(Tags.Ground))
        {
            isGrounded = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.GroundEnd))
        {
            generator.Generate();
            cameraController.SetAirborne(true);
        }
        else if (other.CompareTag(Tags.JumpSpot))
        {
            isOnJumpSpot = true;
        }
    }

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
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetFloat(PlayerPrefsKeys.HighScore, 0);
        highScoreText.text = highScore.ToString("F0");
    }

    #endregion

    #region Movement

    public void MoveHorizontally(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Canceled)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();
            targetVelocity = new Vector3(moveInput.x * horizontalSpeed, rb.velocity.y, rb.velocity.z);
            targetVelocity.x = Mathf.Clamp(targetVelocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);
        }
    }

    private void HandleMovement()
    {
        Vector3 newVelocity = new Vector3(targetVelocity.x, rb.velocity.y, rb.velocity.z);
        rb.velocity = !isDead
            ? Vector3.Lerp(rb.velocity, newVelocity, horizontalSpeed * Time.fixedDeltaTime)
            : Vector3.zero;

        if (rb.velocity.y < -maxYSpeed)
        {
            Die();
        }
    }

    #endregion

    #region Jump

    public void Jump()
    {
        if (IsPointerOverUI() || !isGrounded) return;

        PerformJump();

        if (isOnJumpSpot)
        {
            ProcessJumpSpot();
        }
    }

    private void PerformJump()
    {
        rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
        isGrounded = false;
        audioManager.PlayJumpSound();
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
    }

    private void PlayImpactAudio()
    {
        audioManager.StopFallingAudio();
        audioManager.PlayRandomImpactSound();
        Handheld.Vibrate();
    }

    #endregion

    #region Falling State

    private void CheckFallingState()
    {
        if (rb.velocity.y < -35)
        {
            fallingVFX.SetActive(true);
            hadHighFallSpeed = true;
            audioManager.PlayFallingAudio();
        }
        else
        {
            fallingVFX.SetActive(false);
        }
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

    public void Die()
    {
        isDead = true;
        SaveHighScore();
        // Play Interstitial Ad
        UnityInterstitialAd.Instace.LoadAd();
        
        levelEnd.EndLevel();
    }

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
    }
}
