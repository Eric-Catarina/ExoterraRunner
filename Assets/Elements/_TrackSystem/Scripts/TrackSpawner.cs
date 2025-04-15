using UnityEngine;
using System.Collections.Generic;

public class TrackSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BiomeManager biomeManager;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private Transform tracksParent; // Opcional

    [Header("Spawning Logic")]
    [SerializeField] private int parallelTrackCount = 3; // Deve ser ímpar para ter uma pista central
    [SerializeField] private float trackSpacing = 5.0f;  // Espaçamento horizontal entre pistas

    [Header("Descent")]
    [SerializeField] private float trackDescentRate = 0.5f;

    [SerializeField] private List<GameObject> activeTracks = new List<GameObject>();

    private float currentDescent = -10f;

    public void SpawnTracksForScenery(GameObject sceneryModule)
    {
        if (biomeManager.CurrentBiome == null || sceneryModule == null) return;

        List<GameObject> validPrefabs = biomeManager.GetValidTrackPrefabs();
        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning($"Nenhuma pista válida encontrada para o bioma {biomeManager.CurrentBiome.biomeName}");
            return;
        }

        // Posição central do cenário como referência para as pistas paralelas
        Vector3 sceneryCenter = sceneryModule.transform.position;
        Quaternion sceneryRotation = sceneryModule.transform.rotation; // Usa a rotação do cenário

        int centerIndex = parallelTrackCount / 2; // Índice da pista central (funciona para ímpares)

        for (int i = 0; i < parallelTrackCount; i++)
        {
            // Seleciona um prefab de pista aleatório
            GameObject prefabToSpawn = validPrefabs[Random.Range(0, validPrefabs.Count)];

            // Calcula o deslocamento X relativo ao centro do cenário
            float xOffset = (i - centerIndex) * trackSpacing;

            // Calcula a posição final da pista no espaço do mundo
            // Aplica o offset localmente e depois transforma para world space
            Vector3 localOffset = new Vector3(xOffset, -10, 0); // Offset apenas no X local
            Vector3 spawnPosition = sceneryModule.transform.TransformPoint(localOffset);

            // Apply descent
            spawnPosition.y -= currentDescent;

            // Pede ao PoolManager
            GameObject newTrackObject = poolManager.Get(prefabToSpawn, spawnPosition, sceneryRotation);
             if (newTrackObject == null) continue; // Pula se o pool falhar

            // Define o pai (opcional)
            if (tracksParent != null)
                newTrackObject.transform.SetParent(tracksParent);

            activeTracks.Add(newTrackObject); // Adiciona à lista de ativos
        }

        // Update current descent
        currentDescent += trackDescentRate;
    }

    public void CleanupActiveTracks(float cleanupPosZ)
    {
        for (int i = activeTracks.Count - 1; i >= 0; i--)
        {
            GameObject track = activeTracks[i];
            if (track.transform.position.z < cleanupPosZ)
            {
                poolManager.Return(track); // Retorna ao pool
                activeTracks.RemoveAt(i); // Remove da lista
            }
        }
    }
}