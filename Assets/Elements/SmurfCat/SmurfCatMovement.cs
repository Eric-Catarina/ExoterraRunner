using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SmurfCatMovement : MonoBehaviour
{
    public float horizontalSpeed = 2.0f, jumpStrenght; // Adjust this value to control the smoothness of horizontal movement.
    public GameObject loseScreen, fallingVFX;
    public bool isGrounded;
    private Rigidbody rb;
    private Vector3 targetVelocity;

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
    }

private void FixedUpdate()
{
    // Apply the target velocity, smoothing the horizontal movement.
    rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, horizontalSpeed * Time.fixedDeltaTime);
    if (rb.velocity.y < -15){
        fallingVFX.SetActive(true);
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
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

}



