using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class LogInteraction : MonoBehaviour
{
    private int sceneVisitCount;

    void Start()
    {
        // Assuming this script is attached to a GameObject with an XRBaseInteractable component
        var interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.onSelectEntered.AddListener(HandleInteraction);
        }
        else
        {
            Debug.LogWarning("XRBaseInteractable component not found on the GameObject.");
        }

        // Retrieve the visit count for the current scene
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        sceneVisitCount = PlayerPrefs.GetInt("SceneVisitCount" + currentSceneIndex, 0);
    }

    private void HandleInteraction(XRBaseInteractor interactor)
    {
        // Capture the hand's position at the time of interaction
        Vector3 handPosition = interactor.transform.position;
        Vector3 objectPosition = transform.position; // The position of this interactable object

        // Log the interaction
        Debug.Log($"Object: {gameObject.name}, Hand Position: {handPosition}, Object Position: {objectPosition}, Scene Visit: {sceneVisitCount}");

        // Call the logger to save this data to a file
        InteractionLogger.LogInteraction(gameObject.name, handPosition, objectPosition, sceneVisitCount);
    }

    void OnDestroy()
    {
        // Assuming this script is attached to a GameObject with an XRBaseInteractable component
        var interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.onSelectEntered.RemoveListener(HandleInteraction);
        }
    }
}