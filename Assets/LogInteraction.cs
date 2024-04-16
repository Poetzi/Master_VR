using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LogInteraction : MonoBehaviour
{
    void Start()
    {
        var interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.onSelectEntered.AddListener(HandleInteraction);
        }
    }

    private void HandleInteraction(XRBaseInteractor interactor)
    {
        Vector3 handPosition = interactor.transform.position;
        Vector3 objectPosition = transform.position; // The position of this interactable object

        // Call the logger to save this data to a file
        InteractionLogger.LogInteraction(gameObject.name, handPosition, objectPosition);
    }

    void OnDestroy()
    {
        var interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.onSelectEntered.RemoveListener(HandleInteraction);
        }
    }
}
