using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random; // Especifica para usar UnityEngine.Random

public class ScenerySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BiomeManager biomeManager;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private TrackSpawner trackSpawner; // <<< Reference to Track Spawner
    [Tooltip("Opcional: um objeto pai para organizar os cenários na hierarquia.")]
    [SerializeField] private Transform sceneryParent;
    [Tooltip("Referência ao primeiro cenário (ilha) já posicionado na cena. Sua posição será ajustada se Relative To Track for usado.")]
    [SerializeField] public SpawnableElement initialSceneryReference; // Posição inicial pode ser ignorada se gerada relativamente

    [Header("Spawning Logic - Relative to Track")]
    [Tooltip("Offset base do cenário em relação ao ponto final da última pista gerada.")]
    [SerializeField] private Vector3 sceneryOffsetFromTrack = new Vector3(-150f, 50f, 300f); // Z=300 como base para 200-400
    [Tooltip("Variação aleatória adicionada ao offset X base.")]
    [SerializeField] private float randomRangeX = 50f;
    [Tooltip("Variação aleatória adicionada ao offset Y base.")]
    [SerializeField] private float randomRangeY = 20f;
    [Tooltip("Variação aleatória adicionada ao offset Z base (+/- para dar a faixa 200-400).")]
    [SerializeField] private float randomRangeZ = 100f; // +/- 100 Z para dar 200-400 range

    // Lista para rastrear cenários ativos para cleanup
    private List<GameObject> activeScenery = new List<GameObject>();
    // Referência ao último cenário *deste spawner* que foi gerado (ainda útil para tracking)
    public SpawnableElement lastSpawnedSceneryElement { get; private set; } // Made setter private

    void Start()
    {
        if (!ValidateReferences())
        {
            this.enabled = false;
            return;
        }

        // Define a referência inicial
        lastSpawnedSceneryElement = initialSceneryReference;
        if (lastSpawnedSceneryElement != null)
        {
            // --- REMOVED: Initial repositioning logic removed --- 
            // No longer needed as LevelGenerator handles the first scenery spawn after the first track.

            if (!activeScenery.Contains(initialSceneryReference.gameObject))
            {
                activeScenery.Add(initialSceneryReference.gameObject); // Still track the initial one if provided
            }
            Debug.Log($"ScenerySpawner Initialized. Last Scenery: {lastSpawnedSceneryElement.name} at {lastSpawnedSceneryElement.transform.position}");
        }
        else
        {
            Debug.LogError("ScenerySpawner: Referência inicial de cenário (InitialSceneryReference) não está definida!");
            this.enabled = false;
        }
    }

    private bool ValidateReferences()
    {
        bool isValid = true;
        if (biomeManager == null) { Debug.LogError("ScenerySpawner: BiomeManager não definido!"); isValid = false; }
        if (poolManager == null) { Debug.LogError("ScenerySpawner: PoolManager não definido!"); isValid = false; }
        if (trackSpawner == null) { Debug.LogError("ScenerySpawner: TrackSpawner não definido! Geração relativa falhará."); isValid = false; } // Added check
        if (initialSceneryReference == null) { Debug.LogError("ScenerySpawner: InitialSceneryReference não definido!"); isValid = false; }
        // sceneryParent é opcional
        return isValid;
    }

    /// <summary>
    /// Tenta gerar a próxima peça de cenário relativamente à última pista gerada.
    /// </summary>
    /// <returns>O GameObject do cenário gerado, ou null se falhar.</returns>
    public GameObject SpawnNextScenery()
    {
        // Validações essenciais
        if (biomeManager?.CurrentBiome == null) { /* ... */ return null; }
        if (trackSpawner?.lastSpawnedTrackEndAttachPoint == null) // Check TrackSpawner's state
        {
            Debug.LogWarning("ScenerySpawner: TrackSpawner or its last attach point is null. Cannot determine relative position.");
            return null;
        }

        // Obtém prefabs válidos para o bioma atual
        List<GameObject> validPrefabs = biomeManager.GetValidSceneryPrefabs();
        if (validPrefabs == null || validPrefabs.Count == 0) { /* ... */ return null; }

        // Seleciona um prefab aleatório
        GameObject selectedPrefab = validPrefabs[Random.Range(0, validPrefabs.Count)];

        // --- Cálculo da Posição Relativa à Pista ---
        Vector3 trackPosition = trackSpawner.lastSpawnedTrackEndAttachPoint.position;
        Vector3 randomOffset = new Vector3(
            Random.Range(-randomRangeX, randomRangeX),
            Random.Range(-randomRangeY, randomRangeY),
            Random.Range(-randomRangeZ, randomRangeZ)
        );

        Vector3 spawnPosition = trackPosition + sceneryOffsetFromTrack + randomOffset;

        // Garante que Z nunca seja negativo
        spawnPosition.z = Mathf.Max(0f, spawnPosition.z);
        // --- Fim Cálculo ---

        // Rotação aleatória apenas em Y
        Quaternion spawnRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // Pede o objeto ao PoolManager
        GameObject newSceneryObject = poolManager.Get(selectedPrefab, spawnPosition, spawnRotation);
        if (newSceneryObject == null) { /* ... */ return null; }

        // Verifica se o objeto obtido tem o componente necessário
        SpawnableElement newSpawnableElement = newSceneryObject.GetComponent<SpawnableElement>();
        if (newSpawnableElement == null)
        {
            Debug.LogError($"ScenerySpawner: Prefab '{selectedPrefab.name}' ... missing SpawnableElement! Returning to pool.");
            poolManager.Return(newSceneryObject);
            return null;
        }

        // Atualiza a referência para o último cenário gerado *com sucesso*
        lastSpawnedSceneryElement = newSpawnableElement;

        // Define o pai (opcional)
        if (sceneryParent != null)
        {
            newSceneryObject.transform.SetParent(sceneryParent);
        }

        // Adiciona à lista de rastreamento
        activeScenery.Add(newSceneryObject);
        // Debug.Log($"Scenery Spawned Relative: {newSceneryObject.name} at {spawnPosition}");
        return newSceneryObject;
    }

    /// <summary>
    /// Remove cenários que ficaram para trás do jogador.
    /// </summary>
    /// <param name="cleanupPosZ">A coordenada Z atrás da qual os objetos devem ser removidos.</param>
    public void CleanupActiveScenery(float cleanupPosZ)
    {
        // Itera de trás para frente para remover com segurança durante a iteração
        for (int i = activeScenery.Count - 1; i >= 0; i--)
        {
            GameObject scenery = activeScenery[i];

            if (scenery != null && scenery.activeSelf && scenery.transform.position.z < cleanupPosZ)
            {
                // Don't pool the initial reference if it wasn't from the pool or needs special handling
                if (initialSceneryReference != null && scenery == initialSceneryReference.gameObject)
                {
                    Debug.Log("Cleaning up initial scenery reference (deactivating). Consider if pooling is appropriate.");
                    scenery.SetActive(false); // Deactivate instead of pooling?
                    activeScenery.RemoveAt(i);
                }
                else
                {
                    poolManager.Return(scenery);
                    activeScenery.RemoveAt(i);
                }
            }
            else if (scenery == null)
            {
                 activeScenery.RemoveAt(i); // Remove referências nulas
            }
        }
    }

    // Getter público para a posição Z do último cenário, se necessário pelo LevelGenerator (Can be kept)
    public float GetLastSceneryZPosition()
    {
        return lastSpawnedSceneryElement != null ? lastSpawnedSceneryElement.transform.position.z : -1f; // Return -1 if null
    }
}