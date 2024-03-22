using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManagerScript : MonoBehaviour
{
    [System.Serializable]
    public class SpawnablePrefab
    {
        public GameObject prefab;
        public string name;
    }

    public List<SpawnablePrefab> spawnablePrefabs;
    public GameObject playerObject;
    private float playerHeightOffset = 1.15f;

    // Define the pointing space dimensions
    private Vector3 pointingSpaceSize = new Vector3(91, 91, 46);
    private float minDistance = 5f; // Minimum distance between targets

    // Namesets
    private List<string[]> nameSets = new List<string[]>()
    {
        new string[] {"Ant", "Bee", "Cat", "Dog"},
        new string[] {"Axe", "Nail", "Rake", "Saw"},
        new string[] {"Cup", "Fork", "Pan", "Wok"}
    };

    void Start()
    {
        SpawnAroundPlayer();
    }

    void SpawnAroundPlayer()
    {
        if (playerObject == null) return;

        Vector3 floorPosition = playerObject.transform.position - new Vector3(0, playerHeightOffset, 0);

        // Generate unique locations
        List<Vector3> locations = GenerateUniqueLocations(4, pointingSpaceSize, minDistance, floorPosition);

        // Assign names to locations randomly from a randomly selected nameset
        string[] selectedNameSet = nameSets[Random.Range(0, nameSets.Count)];
        ShuffleArray(selectedNameSet); // Randomize names within the selected set

        for (int i = 0; i < locations.Count; i++)
        {
            SpawnablePrefab prefab = spawnablePrefabs.Find(p => p.name == selectedNameSet[i]);
            if (prefab != null)
            {
                Instantiate(prefab.prefab, locations[i], Quaternion.identity);
            }
        }
    }

    List<Vector3> GenerateUniqueLocations(int count, Vector3 size, float minDist, Vector3 basePosition)
    {
        List<Vector3> locations = new List<Vector3>();
        int attempts = 0;

        while (locations.Count < count && attempts < 1000)
        {
            Vector3 potentialLocation = new Vector3(
                Random.Range(-size.x / 2, size.x / 2),
                Random.Range(-size.y / 2, size.y / 2),
                Random.Range(-size.z / 2, size.z / 2)
            ) + basePosition;

            bool tooClose = false;
            foreach (var otherLocation in locations)
            {
                if (Vector3.Distance(potentialLocation, otherLocation) < minDist)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                locations.Add(potentialLocation);
            }

            attempts++;
        }

        return locations;
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
}
