using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SmurfCatMovement : MonoBehaviour
{
    public float moveSpeed = 5;
    public float horizontalSpeed = 2.0f; // Adjust this value to control the smoothness of horizontal movement.
    public GameObject godModeVFX, loseScreen;

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
        Sugar.OnSugarCollected -= TurnOnGodMode;
        Spike.OnSpikeHit -= ShowLoseScreen;
    }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        Sugar.OnSugarCollected += TurnOnGodMode;
        Spike.OnSpikeHit += ShowLoseScreen;

    }


    private void Update(){
        if(Input.touchCount > 0){
            Jump();
        }
    }
    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + targetVelocity * Time.fixedDeltaTime);
        targetVelocity = Vector3.zero;

    }

    public void MoveHorizontally(InputAction.CallbackContext value)
    {
        if (value.phase == InputActionPhase.Performed)
        {
            Vector2 moveInput = value.ReadValue<Vector2>();
            float moveInputX = moveInput.x;

            // Calculate the target velocity based on the input.
            Vector3 targetVelocity = new Vector3(moveInputX * moveSpeed, rb.velocity.y, 10);

            // Apply the target velocity directly to the rigidbody.
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, horizontalSpeed * Time.fixedDeltaTime);
        }
        else if (value.phase == InputActionPhase.Canceled)
        {
            // If input is released, set velocity to zero to stop movement.
            rb.velocity = Vector3.zero;
        }
    }

    public void TurnOnGodMode( )
    {
        StartCoroutine(DeactivateGodModeAfter());
        godModeVFX.SetActive(true);
    }
    public void TurnOffGodMode( )
    {
        godModeVFX.SetActive(false);
    }
    public void SwitchGodMode( )
    {
        if (godModeVFX.activeSelf == false){
        godModeVFX.SetActive(true);
        }
        else{
        godModeVFX.SetActive(false);
        }
    }

    public void ShowLoseScreen()
    {
        if (godModeVFX.activeSelf == false){
            loseScreen.SetActive(true);
        }
    }
    // Deactivate godmode after 10 seconds
    public IEnumerator DeactivateGodModeAfter()
    {
        yield return new WaitForSeconds(30);
        TurnOffGodMode();
    }
    public void Jump()
    {
        if (IsCollidingWithGround() == true)
        {
            rb.AddForce(Vector3.up * 2, ForceMode.Impulse);
        }
    }
    public bool IsCollidingWithGround()
    {
        return true;
    }

}



