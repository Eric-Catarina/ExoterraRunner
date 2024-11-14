using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private Vector3 groundedOffset = new Vector3(0, 5, -10);
    [SerializeField] private Vector3 airborneOffset = new Vector3(0, 8, -12);
    [SerializeField] private float transitionSpeed = 1f;

    private CinemachineTransposer transposer;
    private bool isAirborne = false;
    
    private void Start()
    {
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        groundedOffset = transposer.m_FollowOffset;
        airborneOffset = new Vector3(groundedOffset.x + airborneOffset.x, groundedOffset.y + airborneOffset.y, groundedOffset.z + airborneOffset.z);
        if (virtualCamera == null)
        {
            Debug.LogError("Cinemachine Virtual Camera is not assigned.");
            return;
        }

        transposer.m_FollowOffset = groundedOffset; // Set initial offset
    }

    public void SetAirborne(bool airborne)
    {
        isAirborne = airborne;
    }

    private void Update()
    {
        if (!transposer) return;

        // Choose the target offset based on whether the player is airborne or grounded
        Vector3 targetOffset = isAirborne ? airborneOffset : groundedOffset;

        // Smoothly interpolate the current offset towards the target offset
        transposer.m_FollowOffset = Vector3.Lerp(
            transposer.m_FollowOffset,
            targetOffset,
            Time.deltaTime * transitionSpeed
        );
    }
}