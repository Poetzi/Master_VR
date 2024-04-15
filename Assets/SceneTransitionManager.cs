using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public FadeScreen fadeScreen;
    public TextMeshProUGUI timerText;   // UI Text component to display the countdown
    public float initialDelay = 10f;    // Delay time for the first scene visit

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
        // Check if the current scene is scene index 1 and if it's the first visit
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            bool isFirstVisit = PlayerPrefs.GetInt("FirstVisit" + SceneManager.GetActiveScene().buildIndex, 0) == 0;
            if (isFirstVisit)
            {
                PlayerPrefs.SetInt("FirstVisit" + SceneManager.GetActiveScene().buildIndex, 1);
                StartCoroutine(StartInitialCountdown());
            }
        }
    }

    private void ResetCounters()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            PlayerPrefs.SetInt("FirstVisit" + i, 0);
        }
        PlayerPrefs.Save();
    }

    IEnumerator StartInitialCountdown()
    {
        float timer = initialDelay;
        timerText.gameObject.SetActive(true);

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateTimerDisplay(timer);
            yield return null;
        }

        timerText.gameObject.SetActive(false);
        GoToSceneAsync(SceneManager.GetActiveScene().buildIndex - 1); // Adjust if different scene index is desired
    }

    void UpdateTimerDisplay(float currentTime)
    {
        timerText.text = "Transition in: " + Mathf.Ceil(currentTime).ToString() + "s";
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
