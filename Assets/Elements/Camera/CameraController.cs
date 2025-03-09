using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private Vector3 groundedOffset = new Vector3(0, 5, -10);
    [SerializeField] private Vector3 airborneOffset = new Vector3(0, 8, -12);
    [SerializeField] private float transitionSpeed = 1f;
    [SerializeField] private List<CinemachineVirtualCameraBase> virtualCameras;
    [SerializeField] private GameObject virtualCameraFollow;
    [SerializeField] private GameObject virtualCameraLookAt;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [SerializeField]private CinemachineTransposer transposer;
    private CinemachineBasicMultiChannelPerlin perlinNoise;
    private bool isAirborne = false;
    
    // Deslocamento da câmera para cima e para trás durante a morte
    [SerializeField] private float deathHeightOffset = 10f; // Deslocamento da câmera para cima
    [SerializeField] private float deathBackOffset = 5f; // Deslocamento para trás no eixo Z
    
    [Header("Camera Shake Settings")]
    [SerializeField] private float minShakeDuration = 0.2f;  // Duração mínima do shake
    [SerializeField] private float maxShakeDuration = 0.5f;  // Duração máxima do shake
    [SerializeField] private float minShakeStrength = 1f;     // Intensidade mínima do shake
    [SerializeField] private float maxShakeStrength = 3f;     // Intensidade máxima do shake
    
    [SerializeField] private float bumpStrength = 1f;

    private bool isDead = false;  // Flag para saber se o personagem está morto

    private void Start()
    {
        perlinNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        impulseSource = virtualCamera.GetComponent<CinemachineImpulseSource>(); 
        virtualCameraLookAt = transposer.LookAtTarget.gameObject;
        virtualCameraFollow = transposer.FollowTarget.gameObject;

        groundedOffset = transposer.m_FollowOffset;
        // airborneOffset = new Vector3(groundedOffset.x + airborneOffset.x, groundedOffset.y + airborneOffset.y, groundedOffset.z + airborneOffset.z);
        if (virtualCamera == null)
        {
            Debug.LogError("Cinemachine Virtual Camera is not assigned.");
            return;
        }

        transposer.m_FollowOffset = groundedOffset; // Set initial offset
    }

    void OnEnable()
    {
        SmurfCatMovement.onGroundImpact += CameraBump;
        SmurfCatMovement.onGroundImpact += StopShake;
        SmurfCatMovement.onHighFallSpeed += StartShake;

    }

    void OnDisable()
    {
        SmurfCatMovement.onGroundImpact -= CameraBump;
        SmurfCatMovement.onGroundImpact -= StopShake;
        SmurfCatMovement.onHighFallSpeed -= StartShake;
    }

    public void SetAirborne(bool airborne)
    {
        isAirborne = airborne;
        // virtualCameras[1].Priority = isAirborne ? 11 : 0;
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
    
    public void CameraBump()
    {
        // Gera a duração e intensidade aleatórias dentro dos intervalos definidos
        float shakeDuration = Random.Range(minShakeDuration, maxShakeDuration);
        bumpStrength = Random.Range(bumpStrength * 0.7f, bumpStrength * 1.3f);

        // Aplica o impacto na direção 'up' da câmera com intensidade e duração aleatórias
        impulseSource.GenerateImpulse(Vector3.up * bumpStrength);

        // Pode-se incluir alguma lógica para limitar o tempo ou manipular o efeito de fade out, por exemplo
    }
    public void StartShake()
    {
        // Ativa o Perlin Noise para o shake
        perlinNoise.m_FrequencyGain = 1.5f;
        perlinNoise.m_AmplitudeGain = 1.5f;
    }

    // Para o shake, restaurando os valores
    private void StopShake()
    {
        perlinNoise.m_AmplitudeGain = 0f;  // Desativa o tremor
        perlinNoise.m_FrequencyGain = 0f;
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
