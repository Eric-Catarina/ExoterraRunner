using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TrackSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BiomeManager biomeManager;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private Transform tracksParent; // Opcional
    public SpawnableElement initialTrackReference; // Added field for LevelGenerator

    [Header("Initial Position")]
    [Tooltip("Define a posição exata onde o centro do primeiro conjunto de pistas deve iniciar.")]
    [SerializeField] private Vector3 initialTrackSetPosition = Vector3.zero; // New variable to set start position

    [Header("Spawning Logic")]
    [SerializeField] private int parallelTrackCount = 3; // Deve ser ímpar para ter uma pista central
    [Tooltip("Espaçamento horizontal entre pistas paralelas.")]
    [SerializeField] private float trackSpacing = 20.0f;  // Changed from 5.0f to 20.0f (User range: 10-30)
    [Tooltip("Distância Z adicional entre o ponto final de um conjunto de pistas e o início do próximo.")]
    [SerializeField] private float zDistanceBetweenSets = 150.0f; // New variable (User range: 100-200)

    [Header("Descent")]
    [Tooltip("Quanto cada NOVO CONJUNTO de pistas desce em Y em relação ao conjunto anterior.")]
    [SerializeField] private float trackDescentRate = 75.0f; // Changed from 0.5f to 75.0f (User range: 50-100)

    // Keep track of active tracks and the last attach point
    [SerializeField] private List<GameObject> activeTracks = new List<GameObject>();
    public Transform lastSpawnedTrackEndAttachPoint { get; private set; }
    private float currentDescent = -10f;

    void Start()
    {
        // Initialize lastSpawnedTrackEndAttachPoint from the initial reference
        if (initialTrackReference?.endAttachPoint != null)
        {
            // --- Move initial track to the specified starting position --- START
            initialTrackReference.transform.position = initialTrackSetPosition;
            // --- Move initial track to the specified starting position --- END

            lastSpawnedTrackEndAttachPoint = initialTrackReference.endAttachPoint;
            currentDescent = initialTrackReference.transform.position.y; // Start descent from the *new* initial track height
            if (!activeTracks.Contains(initialTrackReference.gameObject))
            {
                 activeTracks.Add(initialTrackReference.gameObject); // Ensure initial track is managed
            }
        }
        else
        {
             Debug.LogError("TrackSpawner: Initial Track Reference or its EndAttachPoint is not set! Generation might fail.");
             // Attempt a fallback if needed, e.g., create a dummy transform at origin
             // lastSpawnedTrackEndAttachPoint = new GameObject("InitialTrackDummyAnchor").transform;
             // lastSpawnedTrackEndAttachPoint.position = Vector3.zero; // Or player start position
        }
    }

    /// <summary>
    /// Spawns the next set of parallel tracks based on the last spawned track's end point.
    /// </summary>
    /// <returns>The Transform of the furthest endAttachPoint among the newly spawned tracks, or null if failed.</returns>
    public Transform SpawnNextTrackSet() // Renamed and modified method
    {
        if (biomeManager.CurrentBiome == null)
        {
            Debug.LogError("TrackSpawner: Current Biome is null!");
            return null;
        }
        if (lastSpawnedTrackEndAttachPoint == null)
        {
            Debug.LogError("TrackSpawner: lastSpawnedTrackEndAttachPoint is null! Cannot determine where to spawn next set.");
            return null;
        }

        List<GameObject> validPrefabs = biomeManager.GetValidTrackPrefabs();
        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning($"No valid track prefabs found for biome {biomeManager.CurrentBiome.biomeName}");
            return null;
        }

        // Use the last attach point as the reference for the new set
        // Apply the Z offset along the forward direction of the attach point
        Vector3 baseSpawnPosition = lastSpawnedTrackEndAttachPoint.position + lastSpawnedTrackEndAttachPoint.forward * zDistanceBetweenSets;
        Quaternion baseSpawnRotation = lastSpawnedTrackEndAttachPoint.rotation; // Use the rotation of the last attach point

        Transform newFurthestAttachPoint = lastSpawnedTrackEndAttachPoint; // Initialize with the previous point
        float maxZ = (newFurthestAttachPoint != null) ? newFurthestAttachPoint.position.z : float.MinValue;

        // Apply descent relative to the *previous set's* Y level
        currentDescent += trackDescentRate;

        int centerIndex = parallelTrackCount / 2; // Index of the middle track

        // Spawn parallel tracks
        for (int i = 0; i < parallelTrackCount; i++)
        {
            // Calculate horizontal offset from the center track
            // Example: For 3 tracks (indices 0, 1, 2), center is 1.
            // i=0 -> offset = (0 - 1) * spacing = -spacing
            // i=1 -> offset = (1 - 1) * spacing = 0
            // i=2 -> offset = (2 - 1) * spacing = +spacing
            float xOffset = (i - centerIndex) * trackSpacing;
            Vector3 localOffset = new Vector3(xOffset, 0, 0); // Offset relative to attach point's orientation

            // Rotate the local offset by the base rotation and add to the base position
            // This aligns tracks correctly even if the attach point is rotated.
            Vector3 spawnPosition = baseSpawnPosition + baseSpawnRotation * localOffset;

            // Apply consistent descent based on accumulated rate
            // The descent is applied to the *entire set* relative to the previous set's Y
            spawnPosition.y = baseSpawnPosition.y - currentDescent; // Apply consistent descent based on accumulated rate

            // Get from pool
            GameObject newTrackObject = poolManager.Get(validPrefabs[Random.Range(0, validPrefabs.Count)], spawnPosition, baseSpawnRotation);
            if (newTrackObject == null) continue;

            if (tracksParent != null)
                newTrackObject.transform.SetParent(tracksParent);

            // Track the furthest Z endAttachPoint among all successfully spawned tracks in this set
            SpawnableElement newSpawnableElement = newTrackObject.GetComponent<SpawnableElement>();
            if (newSpawnableElement?.endAttachPoint != null && newSpawnableElement.endAttachPoint.position.z > maxZ)
            {
                maxZ = newSpawnableElement.endAttachPoint.position.z;
                newFurthestAttachPoint = newSpawnableElement.endAttachPoint;
            }
            else
            {
                Debug.LogWarning($"Spawned track '{newTrackObject.name}' is missing SpawnableElement or endAttachPoint!");
            }

            // Adiciona à lista de ativos
            activeTracks.Add(newTrackObject);
        }

        // After spawning all tracks in the set, update the member variable
        if (newFurthestAttachPoint != lastSpawnedTrackEndAttachPoint) // Check if it actually changed
        {
             this.lastSpawnedTrackEndAttachPoint = newFurthestAttachPoint;
        }
        else if (parallelTrackCount > 0 && newFurthestAttachPoint == null)
        {   // Handle case where spawning might have failed completely for the set
            Debug.LogError("TrackSpawner: Failed to determine a new attach point after spawning attempt.");
            return null; // Indicate failure
        }

        // Return the actual furthest attach point found in this set
        return newFurthestAttachPoint;
    }

    public void CleanupActiveTracks(float cleanupPosZ)
    {
        // Use Linq for potentially cleaner removal, or keep the loop
        // activeTracks.RemoveAll(track => {
        //     if (track != null && track.transform.position.z < cleanupPosZ)
        //     {
        //         poolManager.Return(track);
        //         return true; // Remove from list
        //     }
        //     return false;
        // });

        // Sticking to the explicit loop for clarity and avoiding potential modification issues during iteration
        for (int i = activeTracks.Count - 1; i >= 0; i--)
        {
            GameObject track = activeTracks[i];
             // Add null check for safety
            if (track == null)
            {
                activeTracks.RemoveAt(i);
                continue;
            }

            // Check if the track's *main position* is behind the cleanup threshold
            // Using the main position is usually sufficient for cleanup
            if (track.transform.position.z < cleanupPosZ)
            {
                // Ensure we don't try to return the initial reference if it's managed outside the pool
                // (Although PoolManager should handle this if configured correctly)
                if (initialTrackReference != null && track == initialTrackReference.gameObject)
                {
                    // Decide how to handle the initial reference - maybe just deactivate?
                    // For now, assume PoolManager handles it or it persists.
                    // activeTracks.RemoveAt(i); // Option: Remove from tracking but don't pool
                     Debug.Log("Skipping pooling for initialTrackReference.");
                     activeTracks.RemoveAt(i);
                }
                else
                {
                    poolManager.Return(track);
                    activeTracks.RemoveAt(i);
                }
            }
        }
    }
}