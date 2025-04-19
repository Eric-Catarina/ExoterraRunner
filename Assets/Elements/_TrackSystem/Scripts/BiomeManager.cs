using UnityEngine;
using System.Collections.Generic;
using System; // Para Action

public class BiomeManager : MonoBehaviour
{
    public List<BiomeDefinition> availableBiomes;
    [SerializeField] private float transitionDelay = 2.0f; // Tempo para animação de UI, etc.

    public BiomeDefinition CurrentBiome;
    private int currentBiomeIndex = -1;
    private int modulesSpawnedInCurrentBiome = 0;

    public static event Action<BiomeDefinition> OnBiomeWillChange; // Avisa *antes* da transição (para UI/FX)
    public static event Action<BiomeDefinition> OnBiomeChanged; // Avisa *depois* da transição

    void Start()
    {
        if (availableBiomes == null || availableBiomes.Count == 0)
        {
            Debug.LogError("Nenhum bioma disponível configurado no BiomeManager!");
            this.enabled = false;
            return;
        }
        // Começa com o primeiro bioma sem transição visual imediata
        ForceSetBiome(0);
        ApplyBiomeSettings(CurrentBiome); // Aplica configurações iniciais
    }

    // Chamado pelo LevelGenerator quando um módulo de cenário é spawnado
    public void NotifySceneryModuleSpawned()
    {
        modulesSpawnedInCurrentBiome++;
        CheckForBiomeTransition();
    }

    private void CheckForBiomeTransition()
    {
        if (CurrentBiome == null) return;

        if (modulesSpawnedInCurrentBiome >= CurrentBiome.modulesBeforeTransition)
        {
            // Lógica para decidir se muda (pode ser aleatório, sequencial, etc.)
            // Exemplo simples: sempre muda sequencialmente
            int nextBiomeIndex = (currentBiomeIndex + 1) % availableBiomes.Count;
            StartCoroutine(TransitionToBiomeCoroutine(nextBiomeIndex));
        }
    }

    private System.Collections.IEnumerator TransitionToBiomeCoroutine(int nextBiomeIndex)
    {
        if (nextBiomeIndex == currentBiomeIndex) yield break; // Já está no bioma

        BiomeDefinition nextBiome = availableBiomes[nextBiomeIndex];

        OnBiomeWillChange?.Invoke(nextBiome); // Dispara evento *antes* da mudança

        // --- Aqui você pode adicionar a animação de UI do nome do bioma ---
        // Ex: biomeTextAnimator.Show(nextBiome.biomeName, nextBiome.biomeColor);
        Debug.Log($"Transitioning to Biome: {nextBiome.biomeName}");
        yield return new WaitForSeconds(transitionDelay); // Espera a animação/delay

        ForceSetBiome(nextBiomeIndex);
        ApplyBiomeSettings(CurrentBiome);

        OnBiomeChanged?.Invoke(CurrentBiome); // Dispara evento *depois* da mudança
        modulesSpawnedInCurrentBiome = 0; // Reseta contador para o novo bioma
    }

    private void ForceSetBiome(int index)
    {
        if (index < 0 || index >= availableBiomes.Count) return;
        currentBiomeIndex = index;
        CurrentBiome = availableBiomes[currentBiomeIndex];
        Debug.Log($"Biome set to: {CurrentBiome.biomeName}");
    }

    // Aplica as configurações visuais/gameplay do bioma
    private void ApplyBiomeSettings(BiomeDefinition biome)
    {
        if (biome == null) return;

        // Skybox
        if (biome.skyboxMaterial != null)
        {
            RenderSettings.skybox = biome.skyboxMaterial;
            DynamicGI.UpdateEnvironment(); // Atualiza iluminação global se necessário
        }
        else
        {
            RenderSettings.skybox = null; // Ou um skybox padrão
        }

        // Partículas (Você precisará de um sistema para gerenciar/ativar/desativar)
        // Ex: EnvironmentParticleManager.Instance.SetBiomeParticles(biome.environmentParticlePrefab);

        // Modificadores de Gameplay (ex: velocidade global)
        // Ex: SpeedManager.SetBiomeSpeedMultiplier(biome.speedMultiplier);
    }

    // Método para obter uma lista de prefabs de cenário válidos para o bioma atual
    public List<GameObject> GetValidSceneryPrefabs()
    {
        return CurrentBiome?.sceneryPrefabs ?? new List<GameObject>();
    }

    // Método para obter uma lista de prefabs de pista válidos para o bioma atual
    public List<GameObject> GetValidTrackPrefabs()
    {
        return CurrentBiome?.trackPrefabs ?? new List<GameObject>();
    }
}