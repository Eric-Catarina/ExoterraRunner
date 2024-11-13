using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SmurfCatMovement : MonoBehaviour
{
    public float horizontalSpeed = 2.0f, jumpStrength = 10.0f;
    public GameObject loseScreen, fallingVFX, fallExplosionVFX;
    public bool isGrounded;
    public Rigidbody rb;
    public float maxHorizontalSpeed = 15.0f;
    public float maxYSpeed = -20.0f;
    public LevelEnd levelEnd;
    
    private Vector3 targetVelocity;
    private bool hadHighFallSpeed = false, isDead = false;
    private PlayerInput playerInput;
    

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

        #if UNITY_EDITOR
        horizontalSpeed *= 2;
        #endif
    }

    private void FixedUpdate()
    {
        // Aplicando a velocidade desejada suavemente para movimentos horizontais
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
            float moveInputX = moveInput.x;
            moveInputX = Math.Clamp(moveInputX, -maxHorizontalSpeed, maxHorizontalSpeed);
            Debug.Log(moveInputX);
            // Configura a velocidade horizontal alvo, sem alterar o movimento vertical
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
            rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
            isGrounded = false; // Marcar como não no chão imediatamente após o salto
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;

            if (hadHighFallSpeed)
            {
                ActivateGroundExplosion();
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
    
    private void ActivateGroundExplosion()
    {
        // Instancia a explosão de partículas no local do objeto
        GameObject explosion = Instantiate(fallExplosionVFX, transform.position, Quaternion.identity);
        Destroy(explosion, 3.3f);
    }
    
    private void Die()
    {
        isDead = true;
        levelEnd.EndLevel();
    }
}
