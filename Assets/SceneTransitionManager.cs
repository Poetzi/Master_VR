using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public FadeScreen fadeScreen;
    public TextMeshProUGUI displayText;   // Display Text component to show dynamic texts

    // Set these in the script for each instance of the manager
    private int targetSceneIndex = 1;     // The scene index where the timer should be active
    private float timerDuration = 30f;    // Duration for the timer

    private void Start()
    {
        // Check if this manager is in the target scene to activate the timer
        if (SceneManager.GetActiveScene().buildIndex == targetSceneIndex)
        {
            StartCoroutine(StartSceneTransitionCountdown());
        }
    }

    IEnumerator StartSceneTransitionCountdown()
    {
        displayText.gameObject.SetActive(true);
        float timer = timerDuration;
        while (timer > 0)
        {
            UpdateTimerDisplay(timer);
            timer -= Time.deltaTime;
            yield return null;
        }

        displayText.gameObject.SetActive(false);
        // Proceed to the next scene or handle as needed
        GoToSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
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
