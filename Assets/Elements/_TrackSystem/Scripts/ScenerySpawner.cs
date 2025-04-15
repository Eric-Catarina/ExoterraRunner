using UnityEngine;
using System.Collections.Generic;

public class ScenerySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BiomeManager biomeManager;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private Transform sceneryParent; // Opcional: pai para organizar na hierarquia

    [Header("Spawning Logic")]
    [SerializeField] private Vector2 xSpawnRange = new Vector2(-20f, 20f); // Faixa de spawn em X
    [SerializeField] private Vector2 zSpawnOffsetRange = new Vector2(50f, 150f); // Offset em Z
    [SerializeField] private Vector2 ySpawnRange = new Vector2(-2f, 2f); // Variação em Y

    [Header("Distance from Track")]
    [SerializeField] private float minDistanceFromTrack = 100f;
    [SerializeField] private float maxDistanceFromTrack = 5000f;

    private List<GameObject> activeScenery = new List<GameObject>();

    public SpawnableElement LastSpawnedScenery { get; private set; }

    public void TrySpawnScenery(Vector3 referencePosition)
    {
        if (biomeManager.CurrentBiome == null) return;

        List<GameObject> validPrefabs = biomeManager.GetValidSceneryPrefabs();
        if (validPrefabs.Count == 0) return;

        // Decide aleatoriamente se deve spawnar um cenário decorativo
        if (Random.value > 0.5f) return;

        // Seleciona um prefab aleatório
        GameObject prefabToSpawn = validPrefabs[Random.Range(0, validPrefabs.Count)];

        // Calcula a posição de spawn
        float xOffset = Random.Range(xSpawnRange.x, xSpawnRange.y);
        float zOffset = Random.Range(zSpawnOffsetRange.x, zSpawnOffsetRange.y);
        float yOffset = Random.Range(ySpawnRange.x, ySpawnRange.y);

        Vector3 spawnPosition = referencePosition + new Vector3(xOffset, yOffset, zOffset);

        // Pede ao PoolManager
        GameObject newSceneryObject = poolManager.Get(prefabToSpawn, spawnPosition, Quaternion.identity);
        if (newSceneryObject == null) return;

        if (sceneryParent != null)
            newSceneryObject.transform.SetParent(sceneryParent);

        activeScenery.Add(newSceneryObject);
    }

    public void CleanupActiveScenery(float cleanupPosZ)
    {
        for (int i = activeScenery.Count - 1; i >= 0; i--)
        {
            GameObject scenery = activeScenery[i];
            if (scenery != null && scenery.activeSelf && scenery.transform.position.z < cleanupPosZ)
            {
                poolManager.Return(scenery);
                activeScenery.RemoveAt(i);
            }
            else if (scenery == null)
            {
                activeScenery.RemoveAt(i);
            }
        }
    }

    public GameObject SpawnNextScenery()
    {
        if (biomeManager == null || poolManager == null)
        {
            Debug.LogError("BiomeManager ou PoolManager não estão definidos!");
            return null;
        }

        if (biomeManager.CurrentBiome == null)
        {
            Debug.LogWarning("Nenhum bioma atual definido.");
            return null;
        }

        List<GameObject> validPrefabs = biomeManager.GetValidSceneryPrefabs();
        if (validPrefabs == null || validPrefabs.Count == 0)
        {
            Debug.LogWarning("Nenhum prefab de cenário válido encontrado.");
            return null;
        }

        GameObject selectedPrefab = validPrefabs[Random.Range(0, validPrefabs.Count)];
        Vector3 spawnPosition = Vector3.zero;

        if (LastSpawnedScenery != null && LastSpawnedScenery.endAttachPoint != null)
        {
            spawnPosition = LastSpawnedScenery.endAttachPoint.position;

            // Calculate random distance from track
            float distanceFromTrack = Random.Range(minDistanceFromTrack, maxDistanceFromTrack);

            // Calculate random angle
            float angle = Random.Range(0f, 360f);

            // Convert angle and distance to x and z offsets
            float xOffset = Mathf.Cos(angle * Mathf.Deg2Rad) * distanceFromTrack;
            float zOffset = Mathf.Sin(angle * Mathf.Deg2Rad) * distanceFromTrack;

            // Apply the offset to the spawn position
            spawnPosition += new Vector3(xOffset, 0f, zOffset);
        }

        GameObject newSceneryObject = poolManager.Get(selectedPrefab, spawnPosition, Quaternion.identity);
        if (newSceneryObject == null)
        {
            Debug.LogError("Falha ao obter objeto do PoolManager!");
            return null;
        }

        SpawnableElement spawnable = newSceneryObject.GetComponent<SpawnableElement>();
        LastSpawnedScenery = spawnable;

        if (sceneryParent != null)
        {
            newSceneryObject.transform.SetParent(sceneryParent);
        }

        activeScenery.Add(newSceneryObject);
        return newSceneryObject;
    }
}