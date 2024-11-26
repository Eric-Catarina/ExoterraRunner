using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [Header("Pista Settings")]
    [SerializeField] private List<GameObject> trackPrefabs; // Lista de prefabs de pista
    [SerializeField] private GameObject elementContainer;
    [SerializeField] private float timeToGenerate;
    [SerializeField] private float timeToDestroy;
    [SerializeField] private float minimumXPosition, maximumXPosition;
    [SerializeField] private float minimumZRotation, maximumZRotation;

    [Header("Position Variation Settings")]
    [SerializeField] private float xVariationRange = 50f;
    [SerializeField] private float minYVariation = 30f;
    [SerializeField] private float maxYVariation = 150f;
    private float initialZPosition = 0f;

    private float cumulativeYOffset = 0f; // Acumula o deslocamento em Y
    private int generatedElements = 0;
    [SerializeField]
    private GameObject lastElement, nextElement;
    
    private void Start()
    {
        initialZPosition = transform.position.z;
    }


    public void Generate()
    {
        // Seleciona aleatoriamente um prefab de pista
        GameObject selectedTrack = SelectRandomTrack();
        if (selectedTrack == null)
        {
            Debug.LogWarning("Nenhum prefab de pista disponível na lista.");
            return;
        }
        nextElement = selectedTrack;

      

        // Calcula a posição final e instancia o elemento
        Vector3 finalPosition = CalculateFinalPosition();
        
        GameObject newElement = InstantiateElement(selectedTrack, finalPosition);
        
        SetInitialScaleAndPosition(newElement, finalPosition);
        ApplyAnimations(newElement, finalPosition);
        ScheduleDestruction(newElement);
        transform.position = new Vector3(transform.position.x, transform.position.y, initialZPosition*SpeedManager.relativeSpeed);
        lastElement = newElement;
        
        generatedElements++;
        
        if (generatedElements == 1)
        {
            WaitAndGenerate();
        }
    }

    // Seleciona aleatoriamente um prefab de pista
    private GameObject SelectRandomTrack()
    {
        if (trackPrefabs.Count == 0) return null; // Evita erros se a lista estiver vazia
        int randomIndex = Random.Range(0, trackPrefabs.Count);
        return trackPrefabs[randomIndex];
    }

    private Vector3 CalculateFinalPosition()
    {
        float baseXPosition = Random.Range(minimumXPosition, maximumXPosition) + transform.position.x;
        float randomXOffset = Random.Range(-xVariationRange, xVariationRange);
        float randomYOffset = Random.Range(minYVariation, maxYVariation);

        float relativeYOffset = randomYOffset / minYVariation;

        float finalYPosition = transform.position.y - randomYOffset - cumulativeYOffset;

        float finalZPosition = CalculateZPosition().z;
        cumulativeYOffset += randomYOffset;

        return new Vector3(baseXPosition + randomXOffset, finalYPosition,finalZPosition);
    }
    
    private Vector3 CalculateZPosition()
    {
        float startAttachPointZPosition = nextElement.GetComponent<Ground>().startAttachPoint.transform.position.z;
        float endAttachPointZPosition = lastElement.GetComponent<Ground>().endAttachPoint.transform.position.z;
        float zPositionOffset =  nextElement.transform.position.z - startAttachPointZPosition;
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
        element.transform.position = finalPosition + new Vector3(Random.Range(-100, 100), 250, 100); // Posição inicial diagonal

        // Desativa o Collider no próprio objeto e em todos os filhos no início
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
        element.transform.DORotate(new Vector3(0, 0, randomRotationZ), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuad);

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
    
    // Wait 1 second then generate
    public void WaitAndGenerate()
    {
        StartCoroutine(WaitAndGenerateCoroutine());
    }
    
    private IEnumerator WaitAndGenerateCoroutine()
    {
        yield return new WaitForSeconds(timeToGenerate);
        Generate();
    }
}
