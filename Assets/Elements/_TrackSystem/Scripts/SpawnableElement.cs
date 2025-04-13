using UnityEngine;
using DG.Tweening;
using System; // Para Action

public class SpawnableElement : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private bool animateSpawn = true;
    [SerializeField] private Vector3 initialScaleMultiplier = Vector3.one * 1.5f; // Começa um pouco maior
    [SerializeField] private float spawnDuration = 0.6f;
    [SerializeField] private Ease spawnEase = Ease.OutBack;

    [SerializeField] private bool animateDespawn = true;
    [SerializeField] private float despawnDuration = 0.4f;
    [SerializeField] private Ease despawnEase = Ease.InBack;

    [Header("Attach Points (Scenery Only)")]
    public Transform endAttachPoint; // Ponto onde o próximo cenário se conecta

    private Vector3 baseScale;
    private Tween currentTween;
    private Collider[] colliders; // Cache dos colliders

    public int PrefabID { get; private set; } // ID do prefab original (para pooling)

    void Awake()
    {
        baseScale = transform.localScale;
        colliders = GetComponentsInChildren<Collider>(true); // Inclui inativos
    }

    // Chamado pelo PoolManager para identificar a qual pool pertence
    public void Initialize(int prefabID)
    {
        PrefabID = prefabID;
    }

    public void PlaySpawnAnimation()
    {
        // Garante que está desativado se já estiver ativo (reinício)
        gameObject.SetActive(false);

        // Cancela tweens anteriores
        currentTween?.Kill();

        // Prepara para spawn
        SetCollidersEnabled(false); // Desativa colliders durante a animação
        transform.localScale = baseScale * initialScaleMultiplier.x; // Aplica multiplicador inicial (ou Vector3.zero se preferir pop-in)

        gameObject.SetActive(true); // Ativa o GameObject ANTES de iniciar o DOTween

        if (animateSpawn)
        {
             currentTween = transform.DOScale(baseScale, spawnDuration)
                .SetEase(spawnEase)
                .SetUpdate(true) // Garante que funcione mesmo se o Time.timeScale for 0 (útil para hitstop?)
                .OnComplete(OnSpawnComplete);
        }
        else
        {
            transform.localScale = baseScale;
            OnSpawnComplete();
        }
    }

     public void PlayDespawnAnimation(Action onComplete)
    {
        // Cancela tweens anteriores
        currentTween?.Kill();
        SetCollidersEnabled(false); // Desativa colliders

        if (animateDespawn)
        {
            currentTween = transform.DOScale(Vector3.zero, despawnDuration)
                .SetEase(despawnEase)
                .SetUpdate(true)
                .OnComplete(() => {
                    // Reset scale for the pool before calling completion callback
                    transform.localScale = baseScale;
                    onComplete?.Invoke();
                 });
        }
        else
        {
             // Reset scale for the pool before calling completion callback
            transform.localScale = baseScale;
            onComplete?.Invoke();
        }
    }

    private void OnSpawnComplete()
    {
        SetCollidersEnabled(true); // Reativa colliders após a animação
        currentTween = null;
        // Debug.Log($"{gameObject.name} spawn complete.");
    }

     // Helper para ativar/desativar colliders
    private void SetCollidersEnabled(bool enabled)
    {
        if(colliders == null) return;
        foreach (var col in colliders)
        {
            if (col != null) col.enabled = enabled;
        }
    }

    // Garante que o tween seja cancelado se o objeto for destruído abruptamente
    void OnDestroy()
    {
        currentTween?.Kill();
    }
}