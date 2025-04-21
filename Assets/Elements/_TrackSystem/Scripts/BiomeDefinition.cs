using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

[CreateAssetMenu(fileName = "NewBiomeDefinition", menuName = "Gameplay/Biome Definition")]
public class BiomeDefinition : ScriptableObject
{
    [Header("Identification")]
    public string biomeName = "Unnamed Biome";
    public Color biomeDebugColor = Color.white; // Para Gizmos ou UI

    [Header("Track Settings")]
    [Tooltip("Espaçamento horizontal entre pistas paralelas neste bioma.")]
    public float trackSpacing = 20.0f;
    [Tooltip("Espaçamento em Z entre conjuntos de pistas neste bioma.")]
    public float trackSetZSpacing = 50.0f;

    [Header("Visuals")]
    public Material skyboxMaterial;
    [CanBeNull] public GameObject environmentParticlePrefab; // Partículas de ambiente do bioma

    [Header("Spawning Rules")]
    public int modulesBeforeTransition = 4; // Quantos módulos de *cenário* gerar antes de talvez mudar

    [Header("Spawnable Prefabs")]
    // Lista de prefabs de *Cenário* que podem aparecer neste bioma
    public List<GameObject> sceneryPrefabs;

    // Lista de prefabs de *Pistas* que podem aparecer neste bioma
    public List<GameObject> trackPrefabs;

    // --- Opcional: Usar SOs para Cenários/Pistas ---
    // public List<SpawnableItemDefinition> sceneryDefinitions;
    // public List<SpawnableItemDefinition> trackDefinitions;
    // --- Fim Opcional ---

    [Header("Gameplay Modifiers (Optional)")]
    public float speedMultiplier = 1.0f;
    // Outros modificadores: dificuldade, tipos de inimigos/obstáculos permitidos, etc.
}