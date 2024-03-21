using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class InteractionLogger
{
    private static string filePath = $"{Application.persistentDataPath}/interactions.csv";
    private static bool sessionStarted = false;

    public static void LogInteraction(string objectName, Vector3 handPosition)
    {
        if (!sessionStarted)
        {
            StartNewSession();
            sessionStarted = true;
        }

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            // Write the interaction data
            string line = $"{objectName}, {handPosition.x}, {handPosition.y}, {handPosition.z}";
            writer.WriteLine(line);
        }
    }

    private static void StartNewSession()
    {
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            // Check if the file exists and is not empty
            if (new FileInfo(filePath).Length > 0)
            {
                writer.WriteLine("\n\n"); // Add a gap between sessions
            }

            // Write the session start timestamp
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            writer.WriteLine($"Session Start, {timestamp}");
            writer.WriteLine("ObjectName, X, Y, Z"); // Optional: Repeat the header for clarity
        }
    }
}

