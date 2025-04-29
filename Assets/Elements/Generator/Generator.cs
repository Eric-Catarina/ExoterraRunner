using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class BiomeData
{
    public string biomeName; // Nome do Bioma (apenas para referência)
    public Color biomeColor;  // Cor característica do texto
    public GameObject biomeParticle;
    public Material biomeSkybox; // Skybox característico do bioma
    public int tracksBeforeChange = 10; // Número de pistas antes de mudar para o próximo bioma
    public List<GameObject> tracks; // Lista de pistas para o bioma
}

[System.Serializable]
public class TrackSpawnData
{
    public Vector3 position;
    public bool isCentralTrack;
}

public class Generator : MonoBehaviour
{
    [Header("Biomes Settings")] [SerializeField]
    private List<BiomeData> biomes; // Lista reordenável de biomas

    [SerializeField] private GameObject elementContainer;
    [SerializeField] private float timeToGenerate;
    [SerializeField] private float timeToDestroy;
    [SerializeField] private int tracksToChangeBiome = 10;
    [SerializeField] private TextMeshProUGUI biomeText;
    [SerializeField] private float delayBeforeBiomeChange = 2f;

    [Header("Position Variation Settings")] [SerializeField]
    private float minimumXPosition, maximumXPosition;

    [SerializeField] private float xVariationRange = 50f;
    [SerializeField] private float minYVariation = 30f;
    [SerializeField] private float maxYVariation = 150f;

    [SerializeField] private List<GameObject> modulesList;
    public GameObject currentModule;
    public Transform currentModuleEnd;

    private int currentBiomeIndex = 0; // Índice do bioma atual
    private List<GameObject> currentTracks; // Referência para as pistas do bioma atual
    private int tracksGenerated = 0; // Contador de pistas geradas
    private float cumulativeYOffset = 0f; // Acumula o deslocamento em Y
    private float initialZPosition = 0f;
    
    
    [Header("Parallel Tracks Settings")]
    [SerializeField] private int numberOfParallelTracks = 3;
    
    // Mantenha os campos existentes e adicione:
    [SerializeField] private List<GameObject> activeTracks = new List<GameObject>();
    private float latestEndPointZ;
    
    [Header("Track Generation Settings")]
    [SerializeField] private int parallelTracksCount = 3; // Número ímpar recomendado
    [SerializeField] private float trackSpacing = 50f;
    [SerializeField] private float safetyBuffer = 10f; // Margem antes do fim da pista

    private List<GameObject> activeBiomeModules = new List<GameObject>();
    private float furthestEndPointZ;

    public float FurthestEndPointZ => furthestEndPointZ;


    private void UpdateLatestEndPoint()
    {
        latestEndPointZ = float.MinValue;
        foreach (var track in activeTracks)
        {
            var endPoint = track.GetComponent<Ground>().endAttachPoint.transform.position.z;
            if (endPoint > latestEndPointZ)
            {
                latestEndPointZ = endPoint;
            }
        }
    }

    public float LatestEndPointZ => latestEndPointZ;

    [SerializeField] private GameObject lastElement, nextElement;

    private void OnEnable()
    {
        TutorialManager.onTutorialsFinished += ShowBiomeName;
    }
    
    private void OnDisable()
    {
        TutorialManager.onTutorialsFinished -= ShowBiomeName;
    }

    private void Start()
    {
        initialZPosition = transform.position.z;
        
        if (biomes.Count > 0)
        {
            currentTracks = biomes[currentBiomeIndex].tracks; // Define as pistas iniciais
            // ShowBiomeName();
        }
        else
        {
            Debug.LogError("Nenhum bioma configurado no Generator!");
        }
    }

    public void Generate()
    {
        UpdateBiome();
        GenerateParallelTracks();
        tracksGenerated++;
    }
    
    
    private void GenerateSingleTrack()
    {
        GameObject selectedTrack = SelectRandomGround();
        if (selectedTrack == null) return;

        nextElement = selectedTrack;
        Vector3 finalPosition = CalculateFinalPosition();
        GameObject newElement = InstantiateElement(selectedTrack, finalPosition);

        SetInitialScaleAndPosition(newElement, finalPosition);
        ApplyAnimations(newElement, finalPosition);
        ScheduleDestruction(newElement);

        transform.position = new Vector3(transform.position.x, transform.position.y, initialZPosition);
        lastElement = newElement;
    }
    
    private void GenerateParallelTracks()
    {
        GameObject selectedTrack = SelectRandomSingleTrack();
        if (selectedTrack == null) return;

        Vector3 centralPosition = CalculateTrackSpawnPosition();
        
        for (int i = 0; i < numberOfParallelTracks; i++)
        {
            float xOffset = (i - (numberOfParallelTracks - 1) / 2f) * trackSpacing;
            Vector3 spawnPosition = centralPosition + new Vector3(xOffset, 0, 0);
            
            GameObject newTrack = InstantiateTrack(selectedTrack, spawnPosition);
            activeTracks.Add(newTrack);
            
            UpdateLatestEndPoint();
            CleanupOldTracks();
        }
    }

    private Vector3 CalculateTrackSpawnPosition()
    {
        return new Vector3(
            Random.Range(minimumXPosition, maximumXPosition),
            transform.position.y - Random.Range(minYVariation, maxYVariation),
            latestEndPointZ
        );
    }
    
    private GameObject InstantiateTrack(GameObject prefab, Vector3 position)
    {
        GameObject newTrack = Instantiate(prefab, position, Quaternion.identity);
        newTrack.transform.parent = elementContainer.transform;
        InitializeTrack(newTrack);
        return newTrack;
    }

    private void InitializeTrack(GameObject track)
    {
        // Mantenha a lógica de animação e colliders existente
        SetInitialScaleAndPosition(track, track.transform.position);
        ApplyAnimations(track, track.transform.position);
        ScheduleDestruction(track);
    }

    private void CleanupOldTracks()
    {
        // Remove tracks que já estão muito para trás
        activeTracks.RemoveAll(track => 
            track == null || 
            track.transform.position.z < (latestEndPointZ - 100f));
    }

    private void UpdateBiome()
    {
        if (tracksGenerated > 0 && tracksGenerated % biomes[currentBiomeIndex].tracksBeforeChange == 0)
        {
            // Muda para o próximo bioma
            currentBiomeIndex = (currentBiomeIndex + 1) % biomes.Count;
            currentTracks = biomes[currentBiomeIndex].tracks;

            // Chama a mudança de bioma com um atraso
            StartCoroutine(WaitAndChangeBiome());
        }
    }


    private GameObject SelectRandomGround()
    {
        if (currentTracks.Count == 0) return null; // Evita erros se a lista estiver vazia
        int randomIndex = Random.Range(0, currentTracks.Count);
        return currentTracks[randomIndex];
    }

    private GameObject SelectRandomSingleTrack()
    {
        if (currentTracks.Count == 0) return null; // Evita erros se a lista estiver vazia
        int randomIndex = Random.Range(0, currentTracks.Count);
        return currentTracks[randomIndex].GetComponent<Ground>().track;
    }

    private Vector3 CalculateFinalPosition()
    {
        float baseXPosition = Random.Range(minimumXPosition, maximumXPosition) + transform.position.x;
        float randomXOffset = Random.Range(-xVariationRange, xVariationRange);
        float randomYOffset = Random.Range(minYVariation, maxYVariation);

        float finalYPosition = transform.position.y - randomYOffset - cumulativeYOffset;
        float finalZPosition = CalculateZPosition().z;

        cumulativeYOffset += randomYOffset;
        return new Vector3(baseXPosition + randomXOffset, finalYPosition, finalZPosition);
    }

    private Vector3 CalculateZPosition()
    {
        float startAttachPointZPosition = nextElement.GetComponent<Ground>().startAttachPoint.transform.position.z;
        float endAttachPointZPosition = lastElement.GetComponent<Ground>().endAttachPoint.transform.position.z;
        float zPositionOffset = nextElement.transform.position.z - startAttachPointZPosition;
        float finalZPosition = endAttachPointZPosition + zPositionOffset;

        return new Vector3(transform.position.x, transform.position.y, finalZPosition);
    }

    private GameObject InstantiateElement(GameObject prefab, Vector3 position)
    {
        GameObject newElement = Instantiate(prefab, position, Quaternion.identity);
        newElement.transform.parent = elementContainer.transform;
        return newElement;
    }

    private void SetInitialScaleAndPosition(GameObject element, Vector3 finalPosition)
    {
        Vector3 initialScale = element.transform.localScale;
        element.transform.localScale = initialScale * 4f; // Começa maior
        element.transform.position =
            finalPosition + new Vector3(Random.Range(-100, 100), 250, 100); // Posição inicial diagonal

        Collider[] colliders = element.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private void ApplyAnimations(GameObject element, Vector3 finalPosition)
    {
        element.transform.DOScale(element.transform.localScale / 4f, 1f).SetEase(Ease.OutBack);
        element.transform.DOMove(finalPosition, 1f).SetEase(Ease.InQuad);

        float randomRotationZ = Random.Range(-5f, 5f);
        element.transform.DORotate(new Vector3(0, 0, randomRotationZ), 1f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.OutQuad);

        element.transform.DOMove(finalPosition, 1.5f).OnComplete(() =>
        {
            Collider[] colliders = element.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }
        });
    }

    private void ScheduleDestruction(GameObject element)
    {
        Destroy(element, timeToDestroy);
    }

    public void WaitAndGenerate()
    {
        StartCoroutine(WaitAndGenerateCoroutine());
    }

    private IEnumerator WaitAndGenerateCoroutine()
    {
        yield return new WaitForSeconds(timeToGenerate);
        Generate();
    }

    private void ShowBiomeName()
    {
        
        biomeText.gameObject.SetActive(true);

        // Define o texto para o nome do bioma
        biomeText.text = biomes[currentBiomeIndex].biomeName;

        // Define a cor do texto para a cor do bioma atual
        biomeText.color = biomes[currentBiomeIndex].biomeColor;

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
        for (int i = 0; i < biomes.Count; i++)
        {
            if (biomes[i].biomeParticle == null)
            {
                continue;
            }
            biomes[i].biomeParticle.SetActive(false);
        }
        if (biomes[currentBiomeIndex].biomeParticle == null)
        {
            return;
        }
        biomes[currentBiomeIndex].biomeParticle.SetActive(true);

    }
    
    
    private void ChangeBiomeSkybox()
    {
        RenderSettings.skybox = biomes[currentBiomeIndex].biomeSkybox;
    }
    
    private IEnumerator WaitAndChangeBiome()
    {
        // Aguarda X segundos antes de trocar
        yield return new WaitForSeconds(delayBeforeBiomeChange);

        // Anima o texto
        ShowBiomeName();
        ShowBiomeParticle();

        // Troca a Skybox
        ChangeBiomeSkybox();
    }
    

}
