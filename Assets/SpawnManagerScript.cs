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

    // Manually chosen positions
    public List<Vector3> manualPositions;

    // Namesets
    private List<string[]> nameSets = new List<string[]>()
    {
        new string[] {"Ant", "Bee", "Cat", "Dog"},
        new string[] {"Axe", "Nail", "Rake", "Saw"},
        new string[] {"Cup", "Fork", "Pan", "Wok"}
    };

    void Start()
    {
        SpawnAtManualPositions();
    }

    void SpawnAtManualPositions()
    {
        if (playerObject == null || manualPositions == null || manualPositions.Count == 0) return;

        // Assign names to locations randomly from a randomly selected nameset
        string[] selectedNameSet = nameSets[Random.Range(0, nameSets.Count)];
        ShuffleArray(selectedNameSet); // Randomize names within the selected set

        for (int i = 0; i < manualPositions.Count; i++)
        {
            // Ensure we do not exceed the bounds of our prefab list or name set
            if (i >= spawnablePrefabs.Count || i >= selectedNameSet.Length) break;

            SpawnablePrefab prefab = spawnablePrefabs.Find(p => p.name == selectedNameSet[i]);
            if (prefab != null)
            {
                // Instantiate at the manually set position
                Instantiate(prefab.prefab, manualPositions[i], Quaternion.identity);
            }
        }
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
