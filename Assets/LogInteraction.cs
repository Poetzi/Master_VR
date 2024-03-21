using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Ensure you have this namespace to access XRIT components

public class LogInteraction : MonoBehaviour
{
    private XRBaseInteractor interactor;

    void Start()
    {
        // Assuming you're using XR Grab Interactable or a similar component
        var interactable = GetComponent<XRBaseInteractable>();
        interactable.onSelectEntered.AddListener(HandleInteraction);
    }

    private void HandleInteraction(XRBaseInteractor interactor)
    {
        // Capture the hand's position at the time of interaction
        Vector3 handPosition = interactor.transform.position;

        // Log the interaction, for example, by printing to the console or calling another method to handle logging
        Debug.Log($"Object: {gameObject.name}, Hand Position: {handPosition}");

        // Optionally, call a method to save this data to a file
        InteractionLogger.LogInteraction(gameObject.name, handPosition);
    }

    void OnDestroy()
    {
        // Clean up the event listener when the object is destroyed
        if (GetComponent<XRBaseInteractable>() != null)
        {
            GetComponent<XRBaseInteractable>().onSelectEntered.RemoveListener(HandleInteraction);
        }
    }
}
