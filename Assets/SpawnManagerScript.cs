using UnityEngine;

public class SpawnManagerScript : MonoBehaviour
{
    public GameObject objectToSpawn; // The prefab to spawn
    public GameObject playerObject; // Reference to the player object
    public int numberOfObjects = 5; // How many objects to spawn
    public float radius = 5f; // Distance from the player at which to spawn objects
    public float spawnAngle = 180f; // The angle of the arc for spawning objects, adjustable via Unity UI

    void Start()
    {
        SpawnAroundPlayer();
    }

    void SpawnAroundPlayer()
    {
        if (playerObject == null) return; // Safety check to ensure the player object is assigned

        Vector3 playerForward = new Vector3(playerObject.transform.forward.x, 0, playerObject.transform.forward.z).normalized;

        for (int i = 0; i < numberOfObjects; i++)
        {
            // Adjust angle calculation to use the spawnAngle variable
            // Convert spawnAngle from degrees to radians for calculation
            float angleRadians = Mathf.Deg2Rad * spawnAngle;
            float angle = Mathf.Lerp(-angleRadians / 2, angleRadians / 2, (float)i / (numberOfObjects - 1));
            Vector3 spawnDirection = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));

            Vector3 finalSpawnDirection = Quaternion.LookRotation(playerForward) * spawnDirection;
            Vector3 position = playerObject.transform.position + finalSpawnDirection * radius;

            Instantiate(objectToSpawn, position, Quaternion.identity);
        }
    }
}
