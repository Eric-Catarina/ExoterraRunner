using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public float yOffset = 1.0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Follows the player with an y offset
        transform.position = new Vector3(transform.position.x, player.transform.position.y + yOffset, transform.position.z);
        
    }
}
