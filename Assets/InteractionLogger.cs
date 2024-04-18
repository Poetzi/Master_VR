using System.IO;
using UnityEngine;

public static class InteractionLogger
{
    private static string filePath = $"{Application.persistentDataPath}/interactions.csv";
    private static bool sessionStarted = false;

    public static void LogInteraction(string objectName, Vector3 handPosition, Vector3 objectPosition)
    {
        if (!sessionStarted)
        {
            StartNewSession();
            sessionStarted = true;
        }

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            // Write the interaction data along with object position
            string line = $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}, {objectName}, {handPosition.x}, {handPosition.y}, {handPosition.z}, {objectPosition.x}, {objectPosition.y}, {objectPosition.z}";
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
                writer.WriteLine(); // Add a new line to separate sessions
            }

            // Write the session start with a header for the CSV
            string header = "Timestamp, ObjectName, HandPosX, HandPosY, HandPosZ, ObjectPosX, ObjectPosY, ObjectPosZ";
            writer.WriteLine(header);
        }
    }
}
