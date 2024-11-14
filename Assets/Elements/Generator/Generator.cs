using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] private GameObject element, elementContainer;
    [SerializeField] private float timeToGenerate;
    [SerializeField] private float timeToDestroy;
    [SerializeField] private float minimumXPosition, maximumXPosition;
    [SerializeField] private float minimumZRotation, maximumZRotation;
    [SerializeField] private bool hasDifferentZRotation = false;
    private float timer = 0;

    // Range de variação de posição adicional
    [SerializeField] private float xVariationRange = 50f;
    [SerializeField] private float minYVariation = 30f;
    [SerializeField] private float maxYVariation = 150f;

    // Acumula o deslocamento em Y
    private float cumulativeYOffset = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= timeToGenerate)
        {
            timer = 0;
            // Generate();
        }
    }

    public void Generate()
    {
        Vector3 finalPosition = CalculateFinalPosition();
        GameObject newElement = InstantiateElement(finalPosition);
        
        SetInitialScaleAndPosition(newElement, finalPosition);
        ApplyAnimations(newElement, finalPosition);
        ScheduleDestruction(newElement);
    }

    // Calcula a posição final com base nas variações em X e Y
    private Vector3 CalculateFinalPosition()
    {
        float baseXPosition = Random.Range(minimumXPosition, maximumXPosition) + transform.position.x;
        float randomXOffset = Random.Range(-xVariationRange, xVariationRange);
        float randomYOffset = Random.Range(minYVariation, maxYVariation);

        float finalYPosition = transform.position.y - randomYOffset - cumulativeYOffset;
        cumulativeYOffset += randomYOffset;

        return new Vector3(baseXPosition + randomXOffset, finalYPosition, transform.position.z);
    }

    // Instancia o elemento e define seu parent
    private GameObject InstantiateElement(Vector3 position)
    {
        GameObject newElement = Instantiate(element, position, Quaternion.identity);
        newElement.transform.parent = elementContainer.transform;
        return newElement;
    }

    // Define a escala inicial e a posição de spawn do elemento
    private void SetInitialScaleAndPosition(GameObject element, Vector3 finalPosition)
    {
        Vector3 initialScale = element.transform.localScale;
        element.transform.localScale = initialScale * 8f; // Começa maior
        element.transform.position = finalPosition + new Vector3(Random.Range(-50, 50), 250, 0); // Posição inicial diagonal

        // Desativa o Collider no próprio objeto e em todos os filhos no início
        Collider[] colliders = element.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    // Aplica as animações de escala, movimento e rotação
    private void ApplyAnimations(GameObject element, Vector3 finalPosition)
    {
        element.transform.DOScale(element.transform.localScale / 8f, 3f).SetEase(Ease.OutBack);
        element.transform.DOMove(finalPosition, 1.5f).SetEase(Ease.InQuad);

        float randomRotationZ = Random.Range(-5f, 5f);
        element.transform.DORotate(new Vector3(0, 0, randomRotationZ), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuad);

        // Ativa os Colliders após as animações terminarem
        element.transform.DOMove(finalPosition, 1.5f).OnComplete(() =>
        {
            Collider[] colliders = element.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }
        });
    }

    // Configura o tempo para destruir o objeto após sua animação
    private void ScheduleDestruction(GameObject element)
    {
        Destroy(element, timeToDestroy);
    }
}
