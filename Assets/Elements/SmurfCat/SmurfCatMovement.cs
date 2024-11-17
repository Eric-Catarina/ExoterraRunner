using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SmurfCatMovement : MonoBehaviour
{
    public float horizontalSpeed = 2.0f, jumpStrength = 10.0f;
    public GameObject loseScreen, fallingVFX, fallExplosionVFX;
    public bool isGrounded;
    public Rigidbody rb;
    public float maxHorizontalSpeed = 15.0f;
    public float maxYSpeed = -20.0f;
    public LevelEnd levelEnd;
    public Generator generator;
    public GameObject grounds;
    public CameraController cameraController;

    private Vector3 targetVelocity;
    private bool hadHighFallSpeed = false, isDead = false;
    private PlayerInput playerInput;
    
    // Reference to AudioManager and wind sound
    private AudioManager audioManager;

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
        audioManager = FindObjectOfType<AudioManager>(); // Find the AudioManager in the scene

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
        if (isGrounded)
        {
            cameraController.SetAirborne(true);
            rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
            isGrounded = false; 
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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GroundEnd"))
        {

            generator.Generate();
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
}
