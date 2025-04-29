using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using DG.Tweening;
using TMPro; // Para Action

public class BiomeManager : MonoBehaviour
{
    public List<BiomeDefinition> availableBiomes;
    [SerializeField] private float transitionDelay = 2.0f; // Tempo para animação de UI, etc.

    public BiomeDefinition CurrentBiome;
    public TextMeshProUGUI biomeText; // Texto UI para mostrar o nome do bioma
    private int currentBiomeIndex = -1;
    public int modulesSpawnedInCurrentBiome = 0;

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
        ShowBiomeTransitions();
    }

    // Chamado pelo LevelGenerator quando um módulo de cenário é spawnado
    public void NotifySceneryModuleSpawned()
    {
        modulesSpawnedInCurrentBiome++;
        CheckForBiomeTransition();
    } 
    public void CheckForBiomeTransition()
    {
        if (CurrentBiome == null) return;

        if (modulesSpawnedInCurrentBiome >= CurrentBiome.modulesBeforeTransition)
        {
            // Lógica para decidir se muda (pode ser aleatório, sequencial, etc.)
            int nextBiomeIndex = (currentBiomeIndex + 1) % availableBiomes.Count;
            
            // Notify before changing
            OnBiomeWillChange?.Invoke(availableBiomes[nextBiomeIndex]);
            
            // Change biome immediately
            ForceSetBiome(nextBiomeIndex);
            
            // Notify after changing
            OnBiomeChanged?.Invoke(CurrentBiome);
            StartCoroutine(WaitAndChangeBiome()); // Chama a mudança de bioma com um atraso()
            
            modulesSpawnedInCurrentBiome = 0;
        }
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
    
     private void ShowBiomeName()
    {
        
        biomeText.gameObject.SetActive(true);

        // Define o texto para o nome do bioma
        biomeText.text = CurrentBiome.biomeName;

        // Define a cor do texto para a cor do bioma atual
        biomeText.color = CurrentBiome.biomeDebugColor;

        // Pegar RectTransform do TMP
        RectTransform textRect = biomeText.GetComponent<RectTransform>();

        // Armazena a posição atual em Y (para manter o texto na altura desejada)
        float originalY = textRect.anchoredPosition.y;

        // Posicionar inicialmente fora da tela, à esquerda (apenas no eixo X)
        textRect.anchoredPosition = new Vector2(-Screen.width, originalY);

        // Criar a sequência de animações
        Sequence seq = DOTween.Sequence();

        // 1. Entrar da esquerda até o centro
        seq.Append(
            textRect.DOAnchorPos(new Vector2(0f, originalY), 1f)
                .SetEase(Ease.OutBack)
        );

        // 2. Ficar parado 2 segundos
        seq.AppendInterval(2f);

        // 3. Sair pra direita (fora da tela)
        seq.Append(
            textRect.DOAnchorPos(new Vector2(Screen.width, originalY), 1f)
                .SetEase(Ease.InQuad)
        );

        // 4. Desativar após o fim
        seq.OnComplete(() =>
        {
            biomeText.gameObject.SetActive(false);
        });
    }
    
    private void ShowBiomeParticle()
    {
        
        // Set other particles inactive
        for (int i = 0; i < availableBiomes.Count; i++)
        {
            if (availableBiomes[i].environmentParticlePrefab == null)
            {
                continue;
            }
            availableBiomes[i].environmentParticlePrefab.SetActive(false);
        }
        if (CurrentBiome.environmentParticlePrefab == null)
        {
            return;
        }
        CurrentBiome.environmentParticlePrefab.SetActive(true);

    }
    
    
    private void ChangeBiomeSkybox()
    {
        RenderSettings.skybox = CurrentBiome.skyboxMaterial;
    }
    
    private IEnumerator WaitAndChangeBiome()
    {
        // Aguarda X segundos antes de trocar
        yield return new WaitForSeconds(transitionDelay);

        // Anima o texto
        ShowBiomeTransitions();
    }
    
    private void ShowBiomeTransitions()
    {
        ShowBiomeName();
        ShowBiomeParticle();

        ChangeBiomeSkybox();   
    }
}