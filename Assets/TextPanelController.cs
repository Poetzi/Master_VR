using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextPanelController : MonoBehaviour
{
    public TMP_Text textDisplay;
    public Button nextButton;
    private List<string> textsToDisplay;
    private int currentIndex = 0;

    void Start()
    {
        // Initialize your text here
        textsToDisplay = new List<string>()
        {
            "Welcome to the VR Experience!",
            "Use the button to switch texts.",
            "This is the last viewable text, the panel will close after this."
        };

        // Set the first text
        UpdateText();

        // Subscribe to the button's onClick event
        nextButton.onClick.AddListener(HandleButtonClick);
    }

    void UpdateText()
    {
        if (currentIndex < textsToDisplay.Count)
        {
            textDisplay.text = textsToDisplay[currentIndex];
        }
    }

    void HandleButtonClick()
    {
        currentIndex++;
        if (currentIndex < textsToDisplay.Count)
        {
            UpdateText();
        }
        else
        {
            ClosePanel();
        }
    }

    void ClosePanel()
    {
        gameObject.SetActive(false); // Hides the entire canvas or panel
    }
}
