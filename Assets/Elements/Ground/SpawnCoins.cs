using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCoins : MonoBehaviour
{
    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public int numberOfCoins = 10;
    public float heightOffset = 1.0f;

    [Header("Ground Settings")]
    public Vector3 groundScale = new Vector3(10, 0.2f, 100);
    public float xSpacing = 2.0f;
    public float zSpacing = 5.0f;
    public float zStartOffset = 20.0f; // Start spawning coins further along Z-axis

    [Header("Spawn Timing")]
    public float spawnDelay = 1.0f; // Time to wait before spawning coins

    private List<GameObject> spawnedCoins = new List<GameObject>();

    public enum SpawnLayout
    {
        Ordered,
        Random,
        Lanes
    }

    private void Start()
    {
        // Select a random layout and start the delayed spawn
        SpawnLayout layout = (SpawnLayout)Random.Range(0, 3);
        // StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        
        SpawnLayout layout = (SpawnLayout)Random.Range(0, 3);
        // Wait for the specified delay time
        yield return new WaitForSeconds(spawnDelay);

        // Spawn the coins after the delay
        SpawnCoinsAboveGround(layout);
    }

    public void SpawnCoinsAboveGround(SpawnLayout layout)
    {
        // Reparent coinsHolder to groundsHolder (parent of this GameObject)
        GameObject groundsHolder = transform.parent.gameObject;

        // Calculate the spawn area bounds based on the ground's scale
        float halfWidth = groundScale.x / 2f;
        float halfLength = groundScale.z / 2f;

        int coinsSpawned = 0;

        // Loop to spawn coins within the ground's area
        for (float z = -halfLength + zStartOffset; z <= halfLength && coinsSpawned < numberOfCoins; z += zSpacing)
        {
            for (float x = -halfWidth; x <= halfWidth && coinsSpawned < numberOfCoins; x += xSpacing)
            {
                Vector3 spawnPosition = Vector3.zero;

                // Determine spawn position based on layout
                switch (layout)
                {
                    case SpawnLayout.Ordered:
                        // Ordered layout: coins in grid pattern
                        spawnPosition = new Vector3(
                            transform.position.x + x,
                            transform.position.y + heightOffset,
                            transform.position.z + z
                        );
                        break;

                    case SpawnLayout.Random:
                        // Random layout: randomize x within bounds
                        float randomX = Random.Range(-halfWidth, halfWidth);
                        spawnPosition = new Vector3(
                            transform.position.x + randomX,
                            transform.position.y + heightOffset,
                            transform.position.z + z
                        );
                        break;

                    case SpawnLayout.Lanes:
                        // Lanes layout: coins in three fixed lanes
                        float[] lanePositions = { -halfWidth / 2, 0, halfWidth / 2 };
                        float laneX = lanePositions[Random.Range(0, lanePositions.Length)];
                        spawnPosition = new Vector3(
                            transform.position.x + laneX,
                            transform.position.y + heightOffset,
                            transform.position.z + z
                        );
                        break;
                }

                // Instantiate the coin at the calculated position
                GameObject newCoin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
                newCoin.transform.parent = groundsHolder.transform; // Set the coin's parent to groundsHolder
                spawnedCoins.Add(newCoin); // Track the spawned coin
                coinsSpawned++;
            }
        }
    }

    // Optional: Call this to clear previously spawned coins
    public void ClearSpawnedCoins()
    {
        foreach (var coin in spawnedCoins)
        {
            if (coin != null)
            {
                Destroy(coin);
            }
        }
        spawnedCoins.Clear();
    }
}
