using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

[System.Serializable]
public class BiomeData
{
    public string biomeName; // Nome do Bioma (apenas para referência)
    public Color biomeColor;  // Cor característica do texto
    public List<GameObject> tracks; // Lista de pistas para o bioma
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

    [Header("Position Variation Settings")] [SerializeField]
    private float minimumXPosition, maximumXPosition;

    [SerializeField] private float xVariationRange = 50f;
    [SerializeField] private float minYVariation = 30f;
    [SerializeField] private float maxYVariation = 150f;

    private int currentBiomeIndex = 0; // Índice do bioma atual
    private List<GameObject> currentTracks; // Referência para as pistas do bioma atual
    private int tracksGenerated = 0; // Contador de pistas geradas
    private float cumulativeYOffset = 0f; // Acumula o deslocamento em Y
    private float initialZPosition = 0f;

    [SerializeField] private GameObject lastElement, nextElement;

    private void Start()
    {
        initialZPosition = transform.position.z;

        if (biomes.Count > 0)
        {
            currentTracks = biomes[currentBiomeIndex].tracks; // Define as pistas iniciais
            ShowBiomeName();
        }
        else
        {
            Debug.LogError("Nenhum bioma configurado no Generator!");
        }
    }

    public void Generate()
    {
        // Atualiza o bioma a cada 20 pistas
        UpdateBiome();

        // Seleciona aleatoriamente um prefab da lista do bioma atual
        GameObject selectedTrack = SelectRandomTrack();
        if (selectedTrack == null)
        {
            Debug.LogWarning("Nenhum prefab de pista disponível na lista atual.");
            return;
        }

        nextElement = selectedTrack;

        // Calcula a posição final e instancia o elemento
        Vector3 finalPosition = CalculateFinalPosition();
        GameObject newElement = InstantiateElement(selectedTrack, finalPosition);

        SetInitialScaleAndPosition(newElement, finalPosition);
        ApplyAnimations(newElement, finalPosition);
        ScheduleDestruction(newElement);

        transform.position = new Vector3(transform.position.x, transform.position.y, initialZPosition);
        lastElement = newElement;

        tracksGenerated++;

        if (tracksGenerated % 20 == 1) // Após mudar o bioma, gere uma pista adicional para suavizar a transição
        {
            WaitAndGenerate();
        }
    }

    private void UpdateBiome()
    {
        if (tracksGenerated > 0 && tracksGenerated % tracksToChangeBiome == 0)
        {
            currentBiomeIndex = (currentBiomeIndex + 1) % biomes.Count;
            currentTracks = biomes[currentBiomeIndex].tracks;

            // Chamar animação do texto
            ShowBiomeName();
        }
    }

    private GameObject SelectRandomTrack()
    {
        if (currentTracks.Count == 0) return null; // Evita erros se a lista estiver vazia
        int randomIndex = Random.Range(0, currentTracks.Count);
        return currentTracks[randomIndex];
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
        // Ativar o texto
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
}
