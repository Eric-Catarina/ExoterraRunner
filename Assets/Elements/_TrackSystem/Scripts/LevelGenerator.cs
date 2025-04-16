using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private BiomeManager biomeManager;
    [SerializeField] private ScenerySpawner scenerySpawner; // Spawner para ilhas distantes
    [SerializeField] private TrackSpawner trackSpawner;     // Spawner para pistas jogáveis
    [SerializeField] private PoolManager poolManager;     // Para cleanup (usado pelos spawners)

    [Header("Generation Triggering")]
    [SerializeField] private float cleanupDistance = 200f; // Distância atrás para limpar ambos
    [SerializeField] private float cleanupCheckInterval = 50f; // Intervalo para verificar limpeza

    // Rastreia o ponto Z mais distante para cada tipo de spawner
    public float furthestTrackGeneratedZ = 0f;
    private float furthestSceneryGeneratedZ = 0f;
    private float lastCleanupZ = 0f;

    void Start()
    {
        if (!ValidateReferences())
        {
            this.enabled = false;
            return;
        }

        // Initialize furthest Z positions based on initial references IF THEY EXIST
        // Otherwise, GenerateInitialContent will set them based on the first *generated* elements
        if (trackSpawner.initialTrackReference?.endAttachPoint != null)
            furthestTrackGeneratedZ = trackSpawner.initialTrackReference.endAttachPoint.position.z;
        else
            furthestTrackGeneratedZ = transform.position.z; // Fallback

        if (scenerySpawner.initialSceneryReference != null)
            furthestSceneryGeneratedZ = scenerySpawner.initialSceneryReference.transform.position.z;
        else
            furthestSceneryGeneratedZ = transform.position.z; // Fallback

        // Spawn the very first elements
        GenerateInitialContent();
    }

    void Update()
    {
        furthestTrackGeneratedZ = trackSpawner.lastSpawnedTrackEndAttachPoint.position.z;

        if (playerTransform.position.z > furthestTrackGeneratedZ - 30f)
        {
            trackSpawner.SpawnNextTrackSet();
        }

        
    }

    /// <summary>
    /// Generates only the first set of tracks and corresponding scenery.
    /// </summary>
    private void GenerateInitialContent()
    {
        Debug.Log("Generating initial content...");
        bool initialTrackGenFailed = false;
        bool initialSceneryGenFailed = false;

        // --- Spawn ONE initial track set ---
        float previousTrackZ = furthestTrackGeneratedZ;
        Transform newTrackAttachPoint = trackSpawner.SpawnNextTrackSet();

        if (newTrackAttachPoint != null)
        {
            furthestTrackGeneratedZ = newTrackAttachPoint.position.z;
            if (furthestTrackGeneratedZ <= previousTrackZ + 0.1f)
            {
                Debug.LogError("LevelGenerator: Initial track generation failed to advance Z.");
                initialTrackGenFailed = true;
            }
            else
            {
                Debug.Log($"Initial track set generated, furthest Z: {furthestTrackGeneratedZ}");
            }
        }
        else
        {
            Debug.LogError("LevelGenerator: Failed to spawn initial track set.");
            initialTrackGenFailed = true;
        }

        // --- Spawn ONE initial scenery IF track succeeded ---
        if (!initialTrackGenFailed)
        {
            float previousSceneryZ = furthestSceneryGeneratedZ;
            GameObject spawnedScenery = scenerySpawner.SpawnNextScenery();
            if (spawnedScenery != null)
            {
                float newSceneryZ = spawnedScenery.transform.position.z;
                furthestSceneryGeneratedZ = newSceneryZ;

                if (furthestSceneryGeneratedZ <= previousSceneryZ + 0.1f)
                {
                    Debug.LogWarning("LevelGenerator: Initial scenery generation did not advance Z significantly.");
                    // This might be okay if scenery offset is large and negative Y/X
                }
                Debug.Log($"Initial scenery generated, furthest Z: {furthestSceneryGeneratedZ}");
            }
            else
            {
                Debug.LogWarning("LevelGenerator: Failed to spawn initial scenery.");
                initialSceneryGenFailed = true;
            }
        }
        else
        {
            Debug.LogWarning("LevelGenerator: Skipping initial scenery generation due to track generation failure.");
            initialSceneryGenFailed = true; // Mark scenery as failed if track failed
        }

        if (initialTrackGenFailed || initialSceneryGenFailed)
        {
            Debug.LogError("LevelGenerator: Failed to generate essential initial content. Check Spawners and Prefabs.");
            // Optionally disable the generator or enter a safe mode
            // this.enabled = false;
        }
        else
        {
            Debug.Log("Initial content generation complete.");
        }
    }

    private bool ValidateReferences()
    {
        bool valid = true;
        // Adiciona verificações robustas para todas as referências
        if (playerTransform == null) { Debug.LogError("LevelGenerator: Player Transform não definido!"); valid = false; }
        if (biomeManager == null) { Debug.LogError("LevelGenerator: Biome Manager não definido!"); valid = false; }
        if (scenerySpawner == null) { Debug.LogError("LevelGenerator: Scenery Spawner não definido!"); valid = false; }
        if (trackSpawner == null) { Debug.LogError("LevelGenerator: Track Spawner não definido!"); valid = false; }
        if (poolManager == null) { Debug.LogError("LevelGenerator: Pool Manager não definido!"); valid = false; }
        // Validação adicional das referências iniciais dentro dos spawners é feita nos Starts deles
        return valid;
    }
}