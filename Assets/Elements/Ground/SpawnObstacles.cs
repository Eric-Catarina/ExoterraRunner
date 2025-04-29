using UnityEngine;
using System.Collections.Generic;

public class SpawnObstacles : MonoBehaviour
{
    [Header("Track Generation")]
    [SerializeField] private BiomeModule[] biomeModules;
    [SerializeField] private int initialTrackCount = 5;
    [SerializeField] private float spawnTriggerDistance = 20f;
    [SerializeField] private float despawnDistance = -10f;
    
    [Header("Obstacle Settings")]
    [SerializeField] private GameObject[] obstacles;
    [SerializeField] private float minObstacleSpacing = 5f;
    [SerializeField] private float maxObstacleSpacing = 15f;
    [SerializeField] private float obstacleSpawnProbability = 0.7f;

    private List<BiomeModule> activeModules = new List<BiomeModule>();
    private Transform playerTransform;
    private Vector3 lastSpawnPosition;
    private float trackLength;

    private void Start()
    {
        if (biomeModules == null || biomeModules.Length == 0)
        {
            Debug.LogError("No biome modules assigned to track spawner!");
            return;
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (!playerTransform)
        {
            Debug.LogError("Player not found! Make sure it's tagged as 'Player'");
            return;
        }

        // Initialize first track piece
        SpawnInitialTrack();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Check if we need to spawn new track
        BiomeModule lastModule = activeModules[activeModules.Count - 1];
        if (Vector3.Distance(playerTransform.position, lastModule.ModuleEnd.transform.position) < spawnTriggerDistance)
        {
            SpawnNextTrackPiece();
        }

        // Check if we need to despawn old track
        if (activeModules.Count > 0)
        {
            BiomeModule firstModule = activeModules[0];
            if (firstModule.transform.position.x < despawnDistance)
            {
                DespawnTrackPiece(firstModule);
            }
        }
    }

    private void SpawnInitialTrack()
    {
        Vector3 spawnPos = transform.position;
        for (int i = 0; i < initialTrackCount; i++)
        {
            SpawnTrackPiece(spawnPos);
        }
    }

    private void SpawnNextTrackPiece()
    {
        BiomeModule lastModule = activeModules[activeModules.Count - 1];
        Vector3 spawnPos = lastModule.ModuleEnd.transform.position;
        SpawnTrackPiece(spawnPos);
    }

    private void SpawnTrackPiece(Vector3 position)
    {
        BiomeModule prefab = biomeModules[Random.Range(0, biomeModules.Length)];
        BiomeModule newModule = Instantiate(prefab, position, Quaternion.identity);
        activeModules.Add(newModule);

        // Spawn obstacles on the new track piece
        SpawnObstaclesOnModule(newModule);
    }

    private void SpawnObstaclesOnModule(BiomeModule module)
    {
        if (obstacles == null || obstacles.Length == 0) return;

        float currentPos = 0f;
        float moduleLength = Vector3.Distance(module.transform.position, module.ModuleEnd.transform.position);

        while (currentPos < moduleLength)
        {
            if (Random.value < obstacleSpawnProbability)
            {
                GameObject obstacle = obstacles[Random.Range(0, obstacles.Length)];
                Vector3 spawnPos = Vector3.Lerp(module.transform.position, module.ModuleEnd.transform.position, currentPos / moduleLength);
                Instantiate(obstacle, spawnPos, Quaternion.identity, module.transform);
            }
            currentPos += Random.Range(minObstacleSpacing, maxObstacleSpacing);
        }
    }

    private void DespawnTrackPiece(BiomeModule module)
    {
        activeModules.Remove(module);
        Destroy(module.gameObject);
    }
}
