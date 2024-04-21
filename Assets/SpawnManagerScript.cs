using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class SpawnManagerScript : MonoBehaviour
{
    [System.Serializable]
    public class SpawnablePrefab
    {
        public GameObject prefab;
        public string name;

    }

    [SerializeField] private List<SpawnablePrefab> spawnablePrefabs;
    [SerializeField] private GameObject startObjectPrefab;
    [SerializeField] private Vector3 startObjectPosition = new Vector3(0, 0, 0);
    [SerializeField] private TextMeshProUGUI targetNameText;
    [SerializeField] private Vector3 boxCenter = new Vector3(0, 1, 0);
    [SerializeField] private Vector3 boxSize = new Vector3(0.91f, 0.91f, 0.46f);
    [SerializeField] private float minDistance = 0.05f;
    [SerializeField] private int maxCycles = 1;
    [SerializeField] private SceneTransitionManager sceneTransitionManager;

    private List<string[]> nameSets = new List<string[]> { new string[] { "Ant", "Bee", "Cat", "Dog" }, new string[] { "Axe", "Nail", "Rake", "Saw" }, new string[] { "Cup", "Fork", "Pan", "Wok" } };
    private Dictionary<string, GameObject> instantiatedObjects = new Dictionary<string, GameObject>();
    private string[] selectedNameSet;
    private int currentTargetIndex = 0;
    private int currentCycle = 0;
    private bool timerRunning = false;
    private bool buttonReleased = true;
    private float startTime;
    private GameObject startObjectInstance;

    void Start()
    {
        if (!TryGenerateObjects())
        {
            Debug.LogError("Failed to generate enough unique locations. Retrying...");
            Start();
        }
    }

    private bool TryGenerateObjects()
    {
        List<Vector3> targetLocations = GenerateUniqueLocations(4, boxSize, minDistance);
        if (targetLocations.Count < 4) return false;

        selectedNameSet = SelectRandomNameSet();
        AssignNamesAndSpawnPrefabs(targetLocations, selectedNameSet);
        startObjectInstance = InstantiateStartObject();
        UpdateTargetName(currentTargetIndex);
        return true;
    }

    private string[] SelectRandomNameSet()
    {
        var setName = nameSets[Random.Range(0, nameSets.Count)];
        ShuffleArray(setName);
        return setName;
    }

    private GameObject InstantiateStartObject()
    {
        if (startObjectPrefab == null) return null;

        GameObject startObject = Instantiate(startObjectPrefab, startObjectPosition, Quaternion.identity);
        if (startObject.TryGetComponent<XRBaseInteractable>(out var interactable))
        {
            interactable.selectEntered.AddListener(StartInteraction);
            interactable.selectExited.AddListener(EndInteraction);
        }
        return startObject;
    }

    private void StartInteraction(SelectEnterEventArgs args) => buttonReleased = false;

    private void EndInteraction(SelectExitEventArgs args)
    {
        buttonReleased = true;
        if (!timerRunning && currentCycle < maxCycles)
            StartTimer();
    }

    private void StartTimer()
    {
        Debug.Log("Timer Starts");
        startTime = Time.time;
        timerRunning = true;
    }

    void Update()
    {
        if (timerRunning && buttonReleased && TryGetPrimaryButton(out bool primaryButtonValue) && primaryButtonValue)
            StopTimer();
    }

    private void StopTimer()
    {
        Debug.Log("Timer Ends");
        timerRunning = false;
        buttonReleased = false;
        float elapsedTime = Time.time - startTime;
        string targetName = selectedNameSet[currentTargetIndex];
        Vector3 controllerPosition = GetRightControllerPosition();
        Vector3 targetObjectPosition = instantiatedObjects[targetName].transform.position;
        LogTimePositionAndTarget(elapsedTime, controllerPosition, targetObjectPosition, targetName);

        if (++currentTargetIndex >= selectedNameSet.Length)
        {
            currentTargetIndex = 0;
            if (++currentCycle >= maxCycles)
            {
                Debug.Log("Max cycles reached. Initiating scene transition.");
                sceneTransitionManager.GoToSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
                return;
            }
            ShuffleArray(selectedNameSet);
        }
        UpdateTargetName(currentTargetIndex);
    }


    private void LogTimePositionAndTarget(float time, Vector3 controllerPosition, Vector3 targetPosition, string targetName)
    {
        string filePath = Application.persistentDataPath + "/interactionTimes.csv";
        bool fileExists = File.Exists(filePath);

        // Get current scene information
        Scene currentScene = SceneManager.GetActiveScene();
        int sceneIndex = currentScene.buildIndex;
        string sceneName = currentScene.name;

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            if (!fileExists || new FileInfo(filePath).Length == 0)
            {
                writer.WriteLine("Cycle, Scene Index, Scene Name, Timestamp, Elapsed Time (s), Controller X, Controller Y, Controller Z, Target Name, Target X, Target Y, Target Z");
            }
            writer.WriteLine($"{currentCycle}, {sceneIndex}, {sceneName}, {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}, {time:F3}, {controllerPosition.x:F3}, {controllerPosition.y:F3}, {controllerPosition.z:F3}, {targetName}, {targetPosition.x:F3}, {targetPosition.y:F3}, {targetPosition.z:F3}");
        }
    }

    private Vector3 GetRightControllerPosition()
    {
        UnityEngine.XR.InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 position);
        return position;
    }

    private void AssignNamesAndSpawnPrefabs(List<Vector3> locations, string[] nameSet)
    {
        instantiatedObjects.Clear();
        for (int i = 0; i < locations.Count; i++)
        {
            SpawnablePrefab prefab = spawnablePrefabs.Find(p => p.name == nameSet[i]);
            if (prefab != null)
            {
                GameObject instantiatedPrefab = Instantiate(prefab.prefab, locations[i], Quaternion.identity);
                instantiatedObjects.Add(prefab.name, instantiatedPrefab);
            }
        }
    }

    private List<Vector3> GenerateUniqueLocations(int count, Vector3 size, float minDist)
    {
        List<Vector3> locations = new List<Vector3>();
        int maxAttempts = 10000;
        while (locations.Count < count && maxAttempts-- > 0)
        {
            Vector3 potentialLocation = GenerateRandomPoint(size);
            if (IsValidLocation(locations, potentialLocation, minDist))
            {
                locations.Add(potentialLocation);
            }
        }
        return locations;
    }

    private bool IsValidLocation(List<Vector3> locations, Vector3 newLocation, float minDist)
    {
        foreach (Vector3 otherLocation in locations)
        {
            if (Vector3.Distance(newLocation, otherLocation) < minDist)
                return false;
        }
        return true;
    }

    private Vector3 GenerateRandomPoint(Vector3 size)
    {
        return boxCenter + new Vector3(
            Random.Range(-size.x / 2, size.x / 2),
            Random.Range(-size.y / 2, size.y / 2),
            Random.Range(-size.z / 2, size.z / 2)
        );
    }

    private void ShuffleArray<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    private void UpdateTargetName(int targetIndex)
    {
        if (targetIndex < selectedNameSet.Length)
        {
            targetNameText.text = selectedNameSet[targetIndex];
        }
    }

    private bool TryGetPrimaryButton(out bool buttonValue)
    {
        return InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.5f);
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}
