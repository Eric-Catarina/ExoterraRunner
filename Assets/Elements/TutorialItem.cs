using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public partial class TutorialItem : MonoBehaviour
{
    [SerializeField]float timeToDestroy = 5.0f;  // Total time to destroy in seconds

    void Start()
    {
        StartCoroutine(DestroyAfterTime());
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(timeToDestroy);
        transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InBack).OnComplete(() => Destroy(gameObject));
        
    }
}
