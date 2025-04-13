using UnityEngine;
using System.Collections.Generic;

public class ScenerySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BiomeManager biomeManager;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private Transform sceneryParent; // Opcional: pai para organizar na hierarquia

    [Header("Spawning Logic")]
    [SerializeField] private GameObject initialSceneryReference; // Um objeto na cena para começar a conectar

    public SpawnableElement LastSpawnedScenery { get; private set; }

    // TODO: Manter lista de cenários ativos para cleanup
    // private List<GameObject> activeScenery = new List<GameObject>();

    void Start()
    {
        if (initialSceneryReference != null)
        {
            LastSpawnedScenery = initialSceneryReference.GetComponent<SpawnableElement>();
             if(LastSpawnedScenery == null)
                  Debug.LogError("Initial Scenery Reference não tem o componente SpawnableElement!");
        }
        else
        {
            Debug.LogError("Referência inicial de cenário não definida!");
        }
    }

    public GameObject SpawnNextScenery()
    {
        if (biomeManager.CurrentBiome == null || LastSpawnedScenery?.endAttachPoint == null)
        {
             Debug.LogError("Não é possível gerar cenário: Bioma atual ou ponto de anexo final do último cenário são nulos.");
            return null;
        }

        List<GameObject> validPrefabs = biomeManager.GetValidSceneryPrefabs();
        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning($"Nenhum prefab de cenário válido encontrado para o bioma {biomeManager.CurrentBiome.biomeName}");
            return null;
        }

        // Seleciona um prefab aleatório da lista válida
        GameObject prefabToSpawn = validPrefabs[Random.Range(0, validPrefabs.Count)];

        // Pede ao PoolManager
        GameObject newSceneryObject = poolManager.Get(prefabToSpawn, Vector3.zero, Quaternion.identity); // Posição/Rotação são definidas abaixo

        if (newSceneryObject == null)
        {
             Debug.LogError("PoolManager retornou null para o prefab de cenário.");
            return null;
        }

        SpawnableElement newSceneryElement = newSceneryObject.GetComponent<SpawnableElement>();
        if (newSceneryElement == null)
        {
             Debug.LogError($"Prefab de cenário '{prefabToSpawn.name}' não tem o componente SpawnableElement!");
             poolManager.Return(newSceneryObject); // Devolve se estiver incorreto
            return null;
        }

        // Calcula a posição e rotação para conectar
        Transform currentEndAttach = LastSpawnedScenery.endAttachPoint;
        // Assume que os attach points têm a orientação correta.
        // O ponto inicial do *novo* cenário deve coincidir com o ponto final do *anterior*.
        // Se os prefabs tiverem um ponto "StartAttachPoint" na origem, o cálculo é mais simples.
        // Vamos assumir que a origem do novo prefab deve alinhar com o endAttachPoint do anterior.
        newSceneryObject.transform.position = currentEndAttach.position;
        newSceneryObject.transform.rotation = currentEndAttach.rotation;

        // Define o pai (opcional)
        if (sceneryParent != null)
            newSceneryObject.transform.SetParent(sceneryParent);

        // Atualiza a referência para o último spawnado
        LastSpawnedScenery = newSceneryElement;

        // TODO: Adicionar à lista de ativos
        // activeScenery.Add(newSceneryObject);

        // A animação de spawn é chamada dentro do PoolManager.Get -> SpawnableElement.PlaySpawnAnimation

         Debug.Log($"Spawned Scenery: {newSceneryObject.name}");
        return newSceneryObject;
    }

    // TODO: Implementar CleanupActiveScenery(float cleanupPosZ)
    // Iterar sobre 'activeScenery', verificar Z position, chamar poolManager.Return() e remover da lista.
}