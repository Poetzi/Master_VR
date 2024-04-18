using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerInitializer : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 startRotation;

    void OnEnable()
    {
        // Register the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unregister the scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SetPlayerPositionAndRotation());
    }

    private IEnumerator SetPlayerPositionAndRotation()
    {
        // Ensure all scene objects have been properly instantiated and initialized
        yield return null; // Wait for the next frame

        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(startRotation);

        // Optionally, ensure everything is stable (like physics, if needed)
        yield return new WaitForFixedUpdate();

        // Additional initialization can be performed here if needed
    }
}
