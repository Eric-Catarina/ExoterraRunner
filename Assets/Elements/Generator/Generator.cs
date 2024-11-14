using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float xVariationRange = 50f; // De -50 a 50
    
    [SerializeField] private float minYVariation = 30f; // De 0 a -100
    [SerializeField] private float maxYVariation = 150f; // De 0 a -100


    // Acumula o deslocamento em Y
    private float cumulativeYOffset = 0f; // Valor inicial de Y

    // Define o decremento em Y a cada geração
    [SerializeField] private float yDecrementPerGeneration = 0f; // Valor que Y diminui a cada geração

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= timeToGenerate)
        {
            timer = 0;
            Generate();
        }
    }

    public void Generate()
    {
        // Posição base (usando minimum e maximum)
        float baseXPosition = Random.Range(minimumXPosition, maximumXPosition) + transform.position.x;

        // Adiciona uma variação aleatória dentro do range definido
        float randomXOffset = Random.Range(-xVariationRange, xVariationRange); // -50 a 50
        float randomYOffset = Random.Range(minYVariation, maxYVariation); // 0 a -100

        // Aplica o deslocamento acumulado em Y
        float finalYPosition = transform.position.y  - randomYOffset - cumulativeYOffset;

        cumulativeYOffset += randomYOffset;
        // Define a posição final com base na variação
        Vector3 position = new Vector3(baseXPosition + randomXOffset, finalYPosition, transform.position.z);
        
        // Instancia o novo elemento na posição calculada
        GameObject newElement = Instantiate(element, position, Quaternion.identity);
        newElement.transform.parent = elementContainer.transform;

        // Aplica a rotação em Z se necessário
        if (hasDifferentZRotation)
        {
            if (Random.Range(0, 2) == 0)
            {
                newElement.transform.position = new Vector3(minimumXPosition, newElement.transform.position.y, newElement.transform.position.z);
                newElement.transform.Rotate(0, 0, maximumZRotation);
            }
            else
            {
                newElement.transform.position = new Vector3(maximumXPosition, newElement.transform.position.y, newElement.transform.position.z);
                newElement.transform.Rotate(0, 0, minimumZRotation);
            }
        }

        // Atualiza o deslocamento acumulado em Y para a próxima geração

        // Define o tempo para destruir o objeto
        Destroy(newElement, timeToDestroy);
    }
}
