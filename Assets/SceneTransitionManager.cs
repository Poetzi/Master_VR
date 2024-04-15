using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SceneTextEntry
{
    public int exactVisitsRequired; // Exact number of visits required to show this text
    public string text; // Text to display
}

public class SceneTransitionManager : MonoBehaviour
{
    public FadeScreen fadeScreen;
    public TextMeshProUGUI displayText;   // Display Text component to show dynamic texts
    public float initialDelay = 10f;      // Delay time for the first scene visit

    [SerializeField]
    private List<SceneTextEntry> sceneTextEntries; // List of text entries based on exact visit counts

    private static bool applicationStarted = false; // Static flag to check application start

    private void Awake()
    {      
        if (!applicationStarted)
        {
            // Reset counters at the first launch of the application
            ResetCounters();
            applicationStarted = true;
        }
    }

    private void Start()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // Increment the visit count for the scene
        int visitCount = PlayerPrefs.GetInt("SceneVisitCount" + currentSceneIndex, 0);
        PlayerPrefs.SetInt("SceneVisitCount" + currentSceneIndex, visitCount + 1);
        PlayerPrefs.Save();

        // Determine which text to display based on the exact visit count
        SceneTextEntry matchingEntry = sceneTextEntries.Find(entry => entry.exactVisitsRequired == visitCount);
        if (matchingEntry != null)
        {
            displayText.text = matchingEntry.text;
        }
        else
        {
            // If no matching entry is found, you can set a default message or leave it blank
            displayText.text = "";
        }

        // Start countdown only on the first visit to scene 1
        if (currentSceneIndex == 1 && visitCount == 0)
        {
            StartCoroutine(StartInitialCountdown());
        }
    }

    private void ResetCounters()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            PlayerPrefs.SetInt("SceneVisitCount" + i, 0);
        }
        PlayerPrefs.Save();
    }

    IEnumerator StartInitialCountdown()
    {
        float timer = initialDelay;
        displayText.gameObject.SetActive(true);

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateTimerDisplay(timer);
            yield return null;
        }

        displayText.gameObject.SetActive(false);
        GoToSceneAsync(SceneManager.GetActiveScene().buildIndex - 1); // Adjust if different scene index is desired
    }

    void UpdateTimerDisplay(float currentTime)
    {
        displayText.text = "Transition in: " + Mathf.Ceil(currentTime).ToString() + "s";
    }

    public void GoToScene(int sceneIndex)
    {
        StartCoroutine(GoToSceneRoutine(sceneIndex));
    }

    IEnumerator GoToSceneRoutine(int sceneIndex)
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(fadeScreen.fadeDuration);
        SceneManager.LoadScene(sceneIndex);
    }

    public void GoToSceneAsync(int sceneIndex)
    {
        StartCoroutine(GoToSceneAsyncRoutine(sceneIndex));
    }

    IEnumerator GoToSceneAsyncRoutine(int sceneIndex)
    {
        fadeScreen.FadeOut();
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        float timer = 0;
        while (timer < fadeScreen.fadeDuration && !operation.isDone)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        operation.allowSceneActivation = true;
    }
}
