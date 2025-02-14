using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private Vector3 groundedOffset = new Vector3(0, 5, -10);
    [SerializeField] private Vector3 airborneOffset = new Vector3(0, 8, -12);
    [SerializeField] private float transitionSpeed = 1f;
    [SerializeField] private GameObject virtualCameraFollow;
    [SerializeField] private GameObject virtualCameraLookAt;

    [SerializeField]private CinemachineTransposer transposer;
    private bool isAirborne = false;

    // Deslocamento da câmera para cima e para trás durante a morte
    [SerializeField] private float deathHeightOffset = 10f; // Deslocamento da câmera para cima
    [SerializeField] private float deathBackOffset = 5f; // Deslocamento para trás no eixo Z

    private bool isDead = false;  // Flag para saber se o personagem está morto

    private void Start()
    {
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        virtualCameraFollow = transposer.FollowTarget.gameObject;
        virtualCameraLookAt = transposer.LookAtTarget.gameObject;
        
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

    // Função chamada quando o personagem morre
    public void OnPlayerDeath()
    {
        isDead = true;

        // Desativa temporariamente o Follow e o LookAt da câmera
        virtualCamera.Follow = null;
        virtualCamera.LookAt = null;  // Remove o LookAt do jogador

        // Calcula a nova posição da câmera (deslocando para cima a posição atual e para trás no eixo Z)
        Vector3 newCameraPosition = virtualCamera.transform.position + new Vector3(0, deathHeightOffset, -deathBackOffset);

        // Transição da câmera para a nova posição calculada
        virtualCamera.transform.DOMove(newCameraPosition, 0.5f).SetEase(Ease.InOutSine); // Acelerando a transição com um valor de tempo menor

        // Ajustando a rotação para olhar para baixo (verticalmente)
        virtualCamera.transform.DORotate(new Vector3(90, 0, 0), 0.5f)
            .SetEase(Ease.InSine); // Rotaciona para olhar para baixo
        // .OnComplete(() => RestoreCameraFollow());  // Quando a transição terminar, restaura o Follow.
    }

    // Função para restaurar a posição e o Follow após a transição
    private void RestoreCameraFollow()
    {
        // Reconecta o Follow ao jogador
        virtualCamera.Follow = virtualCameraFollow.transform;
        virtualCamera.LookAt = virtualCameraLookAt.transform;
        

        // Transição suave de volta para o offset normal
        transposer.m_FollowOffset = groundedOffset;

        // Restaura a rotação normal da câmera
        virtualCamera.transform.DORotate(Vector3.zero, 0.5f).SetEase(Ease.InOutSine); // Restaura a rotação para normal (olhando para o jogador)
    }

    // Função para reviver o personagem e restaurar a posição da câmera
    public void OnRevive()
    {
        RestoreCameraFollow();
        isDead = false;
        // Transição da câmera de volta para a posição normal
        virtualCamera.transform.DOMove(groundedOffset, 0.5f).SetEase(Ease.InOutSine); // Pode ajustar para o valor de offset desejado
        virtualCamera.transform.DORotate(Vector3.zero, 0.5f).SetEase(Ease.InOutSine); // Restaura a rotação para normal (olhando para o jogador)
    }

    private void Update()
    {
        if (isDead) return; // Se o personagem está morto, não faz sentido mover a câmera com base na posição do jogador

        if (!transposer) return;

        // Escolhe o target offset baseado se o personagem está no ar ou no chão
        Vector3 targetOffset = isAirborne ? airborneOffset : groundedOffset;

        // Interpola suavemente o offset atual para o offset alvo
        transposer.m_FollowOffset = Vector3.Lerp(
            transposer.m_FollowOffset,
            targetOffset,
            Time.deltaTime * transitionSpeed
        );
    }
}
