using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private BiomeManager biomeManager;
    [SerializeField] private ScenerySpawner scenerySpawner;
    [SerializeField] private TrackSpawner trackSpawner;
    [SerializeField] private PoolManager poolManager; // Para cleanup

    [Header("Generation Triggering")]
    [SerializeField] private float generationDistanceThreshold = 150f; // Distância à frente do jogador para gerar
    [SerializeField] private float cleanupDistanceBehind = 200f; // Distância atrás para limpar

    private float furthestPointGeneratedZ = 0f;

    void Start()
    {
        if (!ValidateReferences()) return;
        // Gera o conteúdo inicial
        GenerateInitialContent();
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Verifica se precisa gerar mais conteúdo
        if (furthestPointGeneratedZ - playerTransform.position.z < generationDistanceThreshold)
        {
            GenerateNextSection();
        }

        // Verifica se precisa limpar conteúdo antigo (simplificado)
        // Uma abordagem mais robusta seria rastrear os objetos ativos
        CleanupOldContent();
    }

    private bool ValidateReferences()
    {
        bool valid = true;
        if (playerTransform == null) { Debug.LogError("Player Transform not set!"); valid = false; }
        if (biomeManager == null) { Debug.LogError("Biome Manager not set!"); valid = false; }
        if (scenerySpawner == null) { Debug.LogError("Scenery Spawner not set!"); valid = false; }
        if (trackSpawner == null) { Debug.LogError("Track Spawner not set!"); valid = false; }
        if (poolManager == null) { Debug.LogError("Pool Manager not set!"); valid = false; }
        return valid;
    }

     private void GenerateInitialContent()
    {
        // Gera algumas seções iniciais para preencher o espaço
        for (int i = 0; i < 5; i++) // Gera 5 seções iniciais, por exemplo
        {
            GenerateNextSection();
            // Ajusta artificialmente a posição para forçar a geração sequencial no início
            // (Ou melhore a lógica de `GenerateNextSection` para lidar com o caso inicial)
            if(scenerySpawner.LastSpawnedScenery != null)
                furthestPointGeneratedZ = scenerySpawner.LastSpawnedScenery.endAttachPoint.position.z - generationDistanceThreshold * 0.5f;
        }
         // Após gerar o conteúdo inicial, reseta furthestPointGeneratedZ para o valor correto
        if (scenerySpawner.LastSpawnedScenery != null && scenerySpawner.LastSpawnedScenery.endAttachPoint != null)
        {
            furthestPointGeneratedZ = scenerySpawner.LastSpawnedScenery.endAttachPoint.position.z;
        }
        else
        {
             furthestPointGeneratedZ = 0f; // Valor inicial se nada foi gerado
             Debug.LogWarning("Nenhum cenário inicial gerado ou sem attach point.");
        }
    }

    private void GenerateNextSection()
    {
        // 1. Gera o próximo módulo de cenário
        GameObject newScenery = scenerySpawner.SpawnNextScenery();
        if (newScenery == null)
        {
            Debug.LogError("Falha ao gerar novo cenário!");
            return;
        }

        // 2. Gera as pistas paralelas para *este* cenário
        trackSpawner.SpawnTracksForScenery(newScenery);

        // 3. Atualiza o ponto mais distante gerado
        SpawnableElement sceneryElement = newScenery.GetComponent<SpawnableElement>();
        if (sceneryElement != null && sceneryElement.endAttachPoint != null)
        {
            furthestPointGeneratedZ = sceneryElement.endAttachPoint.position.z;
            // Debug.Log($"Nova seção gerada. Furthest Z: {furthestPointGeneratedZ}");
        }
        else
        {
            // Fallback: estima baseado no tamanho do objeto se não houver attach point
            Renderer rend = newScenery.GetComponentInChildren<Renderer>();
            if(rend != null)
                furthestPointGeneratedZ += rend.bounds.size.z; // Estimativa grosseira
            else
                 furthestPointGeneratedZ += 50f; // Valor padrão

            Debug.LogWarning($"Scenery '{newScenery.name}' não tem SpawnableElement ou endAttachPoint configurado. Estimando Z.");
        }

         // 4. Notifica o BiomeManager sobre o spawn do cenário (para transição)
        biomeManager.NotifySceneryModuleSpawned();
    }

    private void CleanupOldContent()
    {
        // Simplificado: Itera sobre os filhos do PoolManager (onde os objetos retornados ficam)
        // Uma implementação real deveria rastrear objetos ativos e verificar sua posição.
        // Ou usar triggers atrás do jogador.
        foreach (Transform child in poolManager.transform) // Assume que objetos inativos estão no pool
        {
            if (!child.gameObject.activeSelf && child.position.z < playerTransform.position.z - cleanupDistanceBehind)
            {
                 // O objeto já está inativo e no pool, não precisa fazer nada extra aqui
                 // A lógica de retornar ao pool já o desativa e move para cá.
                 // Se você tiver objetos que *não* são gerenciados pelo pool, precisaria destruí-los aqui.
            }
        }

         // Limpeza de objetos *ativos* que ficaram para trás
         // Isso requer que ScenerySpawner e TrackSpawner mantenham listas de objetos ativos
         // Ex: scenerySpawner.CleanupActiveScenery(playerTransform.position.z - cleanupDistanceBehind);
         // Ex: trackSpawner.CleanupActiveTracks(playerTransform.position.z - cleanupDistanceBehind);
    }
}