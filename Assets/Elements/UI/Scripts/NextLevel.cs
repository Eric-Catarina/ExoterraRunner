using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public void LoadNextLevel(int nextLevelId){
        SceneManager.LoadSceneAsync(nextLevelId);
    }

}
