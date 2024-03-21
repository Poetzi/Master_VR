using System.Collections.Generic;
using UnityEngine;

public class SpawnManagerScript : MonoBehaviour
{
    [System.Serializable]
    public class SpawnablePrefab
    {
        public GameObject prefab; // The prefab to spawn
        public Vector3 offset = Vector3.zero; // Offset from the player's position on the floor
    }

    public List<SpawnablePrefab> spawnablePrefabs; // List of prefabs and their specified offsets
    public GameObject playerObject; // Reference to the player object
    private float playerHeightOffset = 1.15f; // Y offset to adjust for player's height above the floor

    void Start()
    {
        SpawnAroundPlayer();
    }

    void SpawnAroundPlayer()
    {
        if (playerObject == null) return;

        // Adjust the reference position to the floor level by subtracting the player's height offset
        Vector3 floorPosition = playerObject.transform.position - new Vector3(0, playerHeightOffset, 0);

        foreach (var spawnablePrefab in spawnablePrefabs)
        {
            // Calculate the spawn position using the provided offset from the floor position
            Vector3 spawnPosition = floorPosition + spawnablePrefab.offset;
            Instantiate(spawnablePrefab.prefab, spawnPosition, Quaternion.identity);
        }
    }
}
