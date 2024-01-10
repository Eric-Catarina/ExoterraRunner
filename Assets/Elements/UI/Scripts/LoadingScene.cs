using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{

    public GameObject loadingScreen;
    public Image loadingBar;

    void Start()
    {
        FakeLoadScene(1);
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    public void FakeLoadScene(int sceneIndex)
    {
        StartCoroutine(FakeLoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        loadingScreen.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingBar.fillAmount = progress;
            yield return null;
        }
    }

    IEnumerator FakeLoadAsynchronously(int sceneIndex)
    {
        loadingScreen.SetActive(true);

        float timeToLoad = 5.0f;  // Total time to load in seconds
        float elapsedTime = 0.0f;
        float progress = 0.0f;
        float pauseTime = 1.0f;
        float pauseTimer = 0.0f;
        bool isPaused = false;

        while (elapsedTime < timeToLoad)
        {
            elapsedTime += Time.deltaTime;

            if (!isPaused)
            {
                // Update progress until it reaches 80%
                if (progress < 0.6f)
                {
                    progress = elapsedTime / timeToLoad;
                }

                // When progress reaches 80%, pause for 1 second
                if (progress >= 0.6f)
                {
                    isPaused = true;
                }
            }
            else
            {
                // Continue to pause for 1 second
                pauseTimer += Time.deltaTime;

                if (pauseTimer >= pauseTime)
                {
                    isPaused = false;
                }
            }

            loadingBar.fillAmount = progress;

            yield return null;
        }

        // Ensure the progress is exactly 100% before loading the scene
        loadingBar.fillAmount = 1.0f;

        // Load the scene after the fake loading is done
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        yield return operation;
    }


}
