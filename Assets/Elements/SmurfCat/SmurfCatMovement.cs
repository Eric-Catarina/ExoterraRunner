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
    public GameObject jumpSpotText;

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

    public void MoveHorizontally(InputAction.CallbackContext value)
    {
        if (value.phase == InputActionPhase.Performed || value.phase == InputActionPhase.Canceled)
        {
            Vector2 moveInput = value.ReadValue<Vector2>();
            targetVelocity = new Vector3(moveInput.x * horizontalSpeed, rb.velocity.y, rb.velocity.z);
            targetVelocity.x = Mathf.Clamp(targetVelocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);
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
            audioManager.PlayJumpSound();
            
            if (isOnJumpSpot)
            {
                ShowJumpSpotText();
                
            }
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
            Debug.Log("Hit ground");
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            cameraController.SetAirborne(false);

            if (hadHighFallSpeed)
            {
                // CHeck if gameobject gave component SpawnCoins
                if (collision.gameObject.GetComponent<SpawnCoins>() != null)
                {
                    collision.gameObject.GetComponent<SpawnCoins>().StartCoroutine("DelayedSpawn");
                }

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
            Debug.Log("Left ground");
            isGrounded = false;
    }

    private void OnCollisionStay(Collision other)
    {
        isGrounded = true;
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
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("JumpSpot"))
        {
            isOnJumpSpot = false;
        }
    }

    private void ActivateGroundExplosion()
    {
        GameObject explosion = Instantiate(fallExplosionVFX, transform.position, Quaternion.identity);
        explosion.transform.SetParent(grounds.transform, worldPositionStays: true);
        // Make the y higher in 4 in Y
        explosion.transform.position = new Vector3(explosion.transform.position.x, explosion.transform.position.y + 2.5f, explosion.transform.position.z);
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
        // A verificação da UI é chamada após o processamento do evento, garantindo que o estado da UI esteja atualizado
        if (EventSystem.current != null)
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
        return false;    
    }
    
    // Set JumpSpot text active, then deactivate it after a delay
    public void ShowJumpSpotText()
    {
        jumpSpotText.SetActive(true);
        StartCoroutine(HideJumpSpotText());
    }
    
    private IEnumerator HideJumpSpotText()
    {
        yield return new WaitForSeconds(2f);
        jumpSpotText.GetComponent<Juice>().Deactivate();
    }
}
