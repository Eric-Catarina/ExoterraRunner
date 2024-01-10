using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] private GameObject element, elementContainer;
    [SerializeField] private float timeToGenerate;
    [SerializeField] private float timeToDestroy;
    [SerializeField]
    private float minimumXPosition, maximumXPosition , minimumZRotation, maximumZRotation;
    [SerializeField]
    public bool hasDifferentZRotation = false;
    private float timer = 0;

    void Start()
    {

    }

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
        float randomXPosition = Random.Range(minimumXPosition, maximumXPosition);
        randomXPosition = randomXPosition + transform.position.x;
        Vector3 position = new Vector3(randomXPosition, transform.position.y, transform.position.z);
        GameObject newElement =  Instantiate(element, position, Quaternion.identity);
        newElement.transform.parent = elementContainer.transform;
        if(hasDifferentZRotation){
            if (Random.Range(0, 2) == 0)
            {
                newElement.transform.position = new Vector3(minimumXPosition, newElement.transform.position.y, newElement.transform.position.z);
                newElement.transform.Rotate(0, 0, maximumZRotation);
            }
            else{
                newElement.transform.position = new Vector3(maximumXPosition, newElement.transform.position.y, newElement.transform.position.z);
                newElement.transform.Rotate(0, 0, minimumZRotation);

            }
        }
        Destroy(newElement, timeToDestroy);
    }

    private float getMedianBetweenTwoNumbers(float number1, float number2)
    {
        return (number1 + number2) / 2;
    }

}
