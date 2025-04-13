using UnityEngine;

[SelectionBase] // Facilita seleção no Editor
public class BiomeModule : MonoBehaviour
{
    [Header("Module Settings")]
    [SerializeField] private Ground _ground;
    [SerializeField] private GameObject moduleEnd;
    [SerializeField] private Transform[] obstacleSpawnPoints;
    [SerializeField] private Transform[] decorationSpawnPoints;

    [Header("Biome Properties")]
    [SerializeField] private string biomeName;
    [SerializeField] private float difficulty = 1f;
    [SerializeField] private bool allowObstacles = true;

    public GameObject ModuleEnd => moduleEnd;
    public Transform[] ObstacleSpawnPoints => obstacleSpawnPoints;
    public Transform[] DecorationSpawnPoints => decorationSpawnPoints;
    public float Difficulty => difficulty;
    public bool AllowObstacles => allowObstacles;
    public Ground Ground => _ground;

    private void OnValidate()
    {
        if (moduleEnd == null)
        {
            Debug.LogError($"ModuleEnd is not set on BiomeModule {gameObject.name}");
        }
    }

    private void OnDrawGizmos()
    {
        // Draw spawn points for easier visualization in editor
        if (obstacleSpawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform point in obstacleSpawnPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.5f);
            }
        }

        if (decorationSpawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in decorationSpawnPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.3f);
            }
        }
    }
}