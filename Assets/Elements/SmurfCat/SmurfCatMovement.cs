using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class SmurfCatMovement : MonoBehaviour
{
    public float horizontalSpeed = 2.0f, jumpStrenght; // Adjust this value to control the smoothness of horizontal movement.
    public GameObject loseScreen, fallingVFX, fallExplosionVFX;
    public bool isGrounded;
    public Rigidbody rb;
    private Vector3 targetVelocity;
    private bool hadHighFallSpeed = false;
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
        // If is playing on unity editor, double the speed  
        #if UNITY_EDITOR
        horizontalSpeed *= 2;
        #endif
    }

private void FixedUpdate()
{
    // Apply the target velocity, smoothing the horizontal movement.
    rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, horizontalSpeed * Time.fixedDeltaTime);
    
    if (rb.velocity.y < -15){
        fallingVFX.SetActive(true);
        hadHighFallSpeed = true;
    }
    else{
        fallingVFX.SetActive(false);
    }
}


public void MoveHorizontally(InputAction.CallbackContext value)
{
    if (value.phase == InputActionPhase.Performed)
    {
        Vector2 moveInput = value.ReadValue<Vector2>();
        float moveInputX = moveInput.x;

        // Calculate the target horizontal velocity and preserve the existing y velocity to maintain gravity's effect.
        targetVelocity = new Vector3(moveInputX * horizontalSpeed * 3, rb.velocity.y, rb.velocity.z);
    }
    else if (value.phase == InputActionPhase.Canceled)
    {
        // Only reset the horizontal movement while preserving the vertical velocity (gravity).
        targetVelocity = new Vector3(0, rb.velocity.y, rb.velocity.z);
    }
}


    public void ShowLoseScreen()
    {
        loseScreen.SetActive(true);

    }

    public void Jump()
    {
        if (isGrounded == true)
        {
            rb.AddForce(Vector3.up * jumpStrenght, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            
            if (hadHighFallSpeed)
            {
                ActivateGroundExplosion(rb.velocity.y);
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
    
    private void ActivateGroundExplosion(float strength){
        // Instantiate the explosion VFX on the object that the player collided
        GameObject explosion = Instantiate(fallExplosionVFX, transform.position, Quaternion.identity);
        //explosion.transform.parent = collision.gameObject.transform;
        Destroy(explosion, 3.3f);
    }

}



