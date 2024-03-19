using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class MovementToggle : MonoBehaviour
{
    public bool enableMovement = true; // The boolean toggle
    public DynamicMoveProvider moveProvider; // Reference to the movement component

    public void SetTypeFromBool(bool boolean)
    {
        if (boolean == true)
        {
            moveProvider.enabled = true;
        }
        else
        {
            moveProvider.enabled = false;
        }
    }
}

