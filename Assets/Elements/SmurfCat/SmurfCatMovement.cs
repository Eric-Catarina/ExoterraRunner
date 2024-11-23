using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SmurfCatMovement : MonoBehaviour
{
    public float horizontalSpeed = 2.0f, jumpStrength = 10.0f;
    public float maxHorizontalSpeed = 15.0f;
    public GameObject loseScreen, fallingVFX, fallExplosionVFX;
    public bool isGrounded;
    public Rigidbody rb;
    public float maxYSpeed = -20.0f;
    public LevelEnd levelEnd;
    public Generator generator;
    public GameObject grounds;
    public CameraController cameraController;

    private Vector3 targetVelocity;
    private bool hadHighFallSpeed = false, isDead = false, isOnJumpSpot = false;
    private PlayerInput playerInput;
    private AudioManager audioManager;

    [Header("Hitstop Settings")]
    public float minHitstopDuration = 0.05f; // Minimum duration of hitstop
    public float maxHitstopDuration = 0.3f; // Maximum duration of hitstop
    public float minHitstopIntensity = 0.2f; // Minimum slowdown intensity
    public float maxHitstopIntensity = 0.0f; // Maximum slowdown intensity (complete stop)
    public float maxVelocityForScaling = 120f; // Velocity threshold for maximum hitstop
    private bool isHitstopActive = false; // Prevents overlapping hitstops

    private void OnEnable()
    {
        playerInput.actions.Enable();
    }

    private void OnDisable()
    {
        playerInput.actions.Disable();
    }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        audioManager = FindObjectOfType<AudioManager>();

#if UNITY_EDITOR
        horizontalSpeed *= 2;
#endif
    }

    private void FixedUpdate()
    {
        Vector3 newVelocity = new Vector3(targetVelocity.x, rb.velocity.y, rb.velocity.z);
        if (!isDead)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, newVelocity, horizontalSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.velocity = Vector3.zero;
        }

        if (rb.velocity.y < -maxYSpeed)
        {
            Die();
        }

        if (rb.velocity.y < -15)
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

    public void MoveHorizontally(InputAction.CallbackContext value)
    {
        if (value.phase == InputActionPhase.Performed || value.phase == InputActionPhase.Canceled)
        {
            Vector2 moveInput = value.ReadValue<Vector2>();
            float moveInputX = Math.Clamp(moveInput.x, -maxHorizontalSpeed, maxHorizontalSpeed);
            targetVelocity = new Vector3(moveInputX * horizontalSpeed, rb.velocity.y, rb.velocity.z);
        }
    }

    public void ShowLoseScreen()
    {
        loseScreen.SetActive(true);
    }

    public void Jump()
    {
        // Checks if game is paused
        if (IsPointerOverUI()) return;
        
        
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
            isGrounded = false;
            
            if (isOnJumpSpot)
            {
                Debug.Log("Jumped on JumpSpot");
            }
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            cameraController.SetAirborne(false);

            if (hadHighFallSpeed)
            {
                collision.gameObject.GetComponent<SpawnCoins>().StartCoroutine("DelayedSpawn");
                ActivateGroundExplosion();
                audioManager.StopFallingAudio();
                audioManager.PlayRandomImpactSound();

                Handheld.Vibrate();
                // Calculate scaled hitstop time and intensity
                float impactVelocity = Mathf.Abs(rb.velocity.y);
                float scaledDuration = Mathf.Lerp(minHitstopDuration, maxHitstopDuration, impactVelocity / maxVelocityForScaling);
                float scaledIntensity = Mathf.Lerp(minHitstopIntensity, maxHitstopIntensity, impactVelocity / maxVelocityForScaling);

                // Trigger hitstop
                StartCoroutine(HitstopCoroutine(scaledDuration, scaledIntensity));
            }

            hadHighFallSpeed = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
        
        if (collision.gameObject.CompareTag("JumpSpot"))
        {
            isOnJumpSpot = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GroundEnd"))
        {
            generator.Generate();

            cameraController.SetAirborne(true);
            
        }
        
        if (other.CompareTag("JumpSpot"))
        {
            isOnJumpSpot = true;
        }
    }

    private void ActivateGroundExplosion()
    {
        GameObject explosion = Instantiate(fallExplosionVFX, transform.position, Quaternion.identity);
        explosion.transform.SetParent(grounds.transform, worldPositionStays: true);
        Destroy(explosion, 3.3f);
    }

    public void Die()
    {
        isDead = true;
        levelEnd.EndLevel();
    }

    private IEnumerator HitstopCoroutine(float duration, float intensity)
    {
        if (isHitstopActive) yield break;

        isHitstopActive = true;
        float originalTimeScale = Time.timeScale;
        Time.timeScale = intensity;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
        isHitstopActive = false;
    }
    
    private bool IsPointerOverUI()
    {
        // Detecta se o cursor ou toque está sobre um elemento da interface
        return EventSystem.current.IsPointerOverGameObject();
    }
}
