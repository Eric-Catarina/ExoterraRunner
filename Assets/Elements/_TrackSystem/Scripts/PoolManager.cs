using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public GameObject prefab;
        [HideInInspector] public Queue<GameObject> objects = new Queue<GameObject>();
    }

    public Dictionary<int, Pool> poolDictionary = new Dictionary<int, Pool>();
    [SerializeField]
    private BiomeManager biomeManager;

    [SerializeField] private List<GameObject> validPrefabs;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        if (biomeManager == null) return;

        // Add track prefabs from all biomes
        foreach (var biome in biomeManager.availableBiomes)
        {
            foreach (var prefab in biome.trackPrefabs)
            {
                if (!validPrefabs.Contains(prefab))
                {
                    validPrefabs.Add(prefab);
                    AddPrefabToPool(prefab, 10);
                }
            }
        }
        
        // Add scenery prefabs
        foreach (var prefab in biomeManager.GetValidSceneryPrefabs())
        {
            AddPrefabToPool(prefab, 5);
        }
    }

    private void AddPrefabToPool(GameObject prefab, int initialSize)
    {
        int prefabId = prefab.GetInstanceID();
        if (!poolDictionary.ContainsKey(prefabId))
        {
            var pool = new Pool { prefab = prefab };
            poolDictionary.Add(prefabId, pool);
            for (int i = 0; i < initialSize; i++)
            {
                AddNewObjectToPool(pool);
            }
        }
    }

    private GameObject AddNewObjectToPool(Pool pool)
    {
        GameObject obj = Instantiate(pool.prefab, transform); // Instancia como filho do PoolManager
        obj.SetActive(false); // Começa desativado
        pool.objects.Enqueue(obj);

        // Adiciona/Obtém o componente SpawnableElement
        SpawnableElement spawnable = obj.GetComponent<SpawnableElement>();
        if (spawnable == null)
        {
            spawnable = obj.AddComponent<SpawnableElement>();
        }
        spawnable.Initialize(pool.prefab.GetInstanceID()); // Passa o ID do prefab original

        return obj;
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int id = prefab.GetInstanceID();
        if (!poolDictionary.ContainsKey(id))
        {
            Debug.LogError($"Pool for prefab '{prefab.name}' not found!");
            return null; // Ou talvez instanciar um novo se for preferível?
        }

        Pool pool = poolDictionary[id];
        GameObject objToSpawn;

        if (pool.objects.Count > 0)
        {
            objToSpawn = pool.objects.Dequeue();
        }
        else
        {
            // Pool vazia, cria um novo objeto
            Debug.LogWarning($"Pool for '{prefab.name}' empty. Creating new instance.");
            objToSpawn = AddNewObjectToPool(pool);
            // Dequeue o objeto recém-adicionado (já que AddNewObjectToPool o enfileira)
            if (pool.objects.Contains(objToSpawn)) // Verifica se está na fila antes de tentar desenfileirar
               pool.objects.Dequeue(); // Remove da fila, pois será ativado agora
            else
               Debug.LogError("Inconsistência no pool após adicionar novo objeto.");

        }

        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        // objToSpawn.SetActive(true); // NÃO ativar aqui, deixa o SpawnableElement fazer isso com animação

        // Chama a animação de spawn via SpawnableElement
        SpawnableElement spawnable = objToSpawn.GetComponent<SpawnableElement>();
        spawnable?.PlaySpawnAnimation(); // O SpawnableElement cuidará de ativar o objeto

        return objToSpawn;
    }

     public void Return(GameObject obj)
    {
        SpawnableElement spawnable = obj.GetComponent<SpawnableElement>();
        if (spawnable == null)
        {
            Debug.LogError($"Object '{obj.name}' being returned to pool doesn't have SpawnableElement!");
            obj.SetActive(false); // Desativa de qualquer forma
            // Potencialmente destruir se não for gerenciável?
            return;
        }

        // Usa o ID armazenado para encontrar o pool correto
        int id = spawnable.PrefabID;
        if (poolDictionary.ContainsKey(id))
        {
            Pool pool = poolDictionary[id];

             // Inicia animação de despawn e retorna ao pool no final
            spawnable.PlayDespawnAnimation(() =>
            {
                 if (obj != null) // Verifica se o objeto ainda existe
                 {
                      // Garante que o objeto está realmente desativado antes de enfileirar
                     obj.SetActive(false);
                     obj.transform.SetParent(transform); // Garante que volte a ser filho do PoolManager
                     pool.objects.Enqueue(obj);
                 }
            });
        }
        else
        {
            Debug.LogError($"Trying to return object '{obj.name}' to a pool that doesn't exist (PrefabID: {id})!");
            // Se não pertence a nenhum pool conhecido, apenas desativa ou destrói
             obj.SetActive(false);
            // Destroy(obj); // Descomente se quiser destruir objetos não pertencentes a pools
        }
    }
}