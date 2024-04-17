using UnityEngine;
using System.Collections.Generic;
using TMPro; // Namespace for TextMesh Pro

public class SpawnManagerScript : MonoBehaviour
{
    [System.Serializable]
    public class SpawnablePrefab
    {
        public GameObject prefab;
        public string name;
    }

    public List<SpawnablePrefab> spawnablePrefabs;
    public GameObject startObjectPrefab; // GameObject prefab for the Start object
    public Vector3 startObjectPosition = new Vector3(0, 0, 0); // Position for the Start object settable in the Editor
    public TextMeshProUGUI targetNameText; // TextMesh Pro UI text element to display the target name

    public Vector3 boxCenter = new Vector3(0, 1, 0); // Static position in the scene
    private Vector3 boxSize = new Vector3(0.91f, 0.91f, 0.46f); // Size of the gizmo box in meters
    private float minDistance = 0.05f; // Minimum distance in meters (5 cm)

    private List<string[]> nameSets = new List<string[]>()
    {
        new string[] {"Ant", "Bee", "Cat", "Dog"},
        new string[] {"Axe", "Nail", "Rake", "Saw"},
        new string[] {"Cup", "Fork", "Pan", "Wok"}
    };

    private string[] selectedNameSet; // Holds the selected name set for use throughout spawning and targeting

    void Start()
    {
        List<Vector3> targetLocations = GenerateUniqueLocations(4, boxSize, minDistance);
        if (targetLocations.Count < 4)
        {
            Debug.LogError("Failed to generate enough unique locations. Retrying...");
            Start(); // Retry the generation process if not enough locations are generated.
        }
        else
        {
            selectedNameSet = nameSets[Random.Range(0, nameSets.Count)];
            ShuffleArray(selectedNameSet);
            AssignNamesAndSpawnPrefabs(targetLocations, selectedNameSet);
            InstantiateStartObject();
            UpdateTargetName(0); // Update the target name text at start with the first object's name
        }
    }

    void AssignNamesAndSpawnPrefabs(List<Vector3> locations, string[] nameSet)
    {
        for (int i = 0; i < locations.Count; i++)
        {
            SpawnablePrefab prefab = spawnablePrefabs.Find(p => p.name == nameSet[i]);
            if (prefab != null)
            {
                Instantiate(prefab.prefab, locations[i], Quaternion.identity);
            }
        }
    }

    void InstantiateStartObject()
    {
        // Instantiate the start object at the set position
        if (startObjectPrefab != null)
        {
            Instantiate(startObjectPrefab, startObjectPosition, Quaternion.identity);
        }
    }

    List<Vector3> GenerateUniqueLocations(int count, Vector3 size, float minDist)
    {
        List<Vector3> locations = new List<Vector3>();
        int maxAttempts = 10000;

        while (locations.Count < count && maxAttempts > 0)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(-size.x / 2, size.x / 2),
                Random.Range(-size.y / 2, size.y / 2),
                Random.Range(-size.z / 2, size.z / 2)
            );

            Vector3 potentialLocation = boxCenter + randomPoint;
            if (IsValidLocation(locations, potentialLocation, minDist))
            {
                locations.Add(potentialLocation);
            }
            maxAttempts--;
        }

        return locations;
    }

    bool IsValidLocation(List<Vector3> locations, Vector3 newLocation, float minDist)
    {
        foreach (Vector3 otherLocation in locations)
        {
            if (Vector3.Distance(newLocation, otherLocation) < minDist)
                return false;
        }
        return true;
    }

    void ShuffleArray<T>(T[] array)
    {
        for (int i = 0; i < array.Length - 1; i++)
        {
            int rnd = Random.Range(i, array.Length);
            T temp = array[i];
            array[i] = array[rnd];
            array[rnd] = temp;
        }
    }

    void UpdateTargetName(int targetIndex)
    {
        if (targetIndex < selectedNameSet.Length)
        {
            targetNameText.text = selectedNameSet[targetIndex]; // Update the text field to show the name of the target
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.5f);
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}
