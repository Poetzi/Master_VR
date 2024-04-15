using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public static class InteractionLogger
{
    private static string filePath = $"{Application.persistentDataPath}/interactions.csv";
    private static bool sessionStarted = false;

    public static void LogInteraction(string objectName, Vector3 handPosition, Vector3 objectPosition, int sceneVisitCount)
    {
        if (!sessionStarted)
        {
            StartNewSession();
            sessionStarted = true;
        }

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                // Write the interaction data along with object position and visit count
                string line = $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}, {objectName}, {handPosition.x}, {handPosition.y}, {handPosition.z}, {objectPosition.x}, {objectPosition.y}, {objectPosition.z}, {sceneVisitCount}";
                writer.WriteLine(line);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error when logging interaction: " + ex.Message);
        }
    }

    private static void StartNewSession()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                // Check if the file exists and is not empty
                if (new FileInfo(filePath).Length > 0)
                {
                    writer.WriteLine(); // Add a new line to separate sessions
                }

                // Write the session start with a header for the CSV
                string header = "Timestamp, ObjectName, HandPosX, HandPosY, HandPosZ, ObjectPosX, ObjectPosY, ObjectPosZ, SceneVisitCount";
                writer.WriteLine(header);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error when starting new session: " + ex.Message);
        }
    }
}
