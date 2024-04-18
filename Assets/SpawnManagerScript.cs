using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private List<string[]> nameSets = new List<string[]>()
    {
        new string[] {"Ant", "Bee", "Cat", "Dog"},
        new string[] {"Axe", "Nail", "Rake", "Saw"},
        new string[] {"Cup", "Fork", "Pan", "Wok"}
    };

    private string[] selectedNameSet;
    private bool timerRunning = false;
    private bool buttonReleased = true;
    private float startTime;
    private GameObject startObjectInstance;
    private XRBaseInteractable startInteractable;

    void Start()
    {
        TryGenerateObjects();
    }

    private void TryGenerateObjects()
    {
        List<Vector3> targetLocations = GenerateUniqueLocations(4, boxSize, minDistance);
        if (targetLocations.Count < 4)
        {
            Debug.LogError("Failed to generate enough unique locations. Retrying...");
            Start();
        }
        else
        {
            InitializeObjects(targetLocations);
        }
    }

    private void InitializeObjects(List<Vector3> targetLocations)
    {
        selectedNameSet = nameSets[Random.Range(0, nameSets.Count)];
        ShuffleArray(selectedNameSet);
        AssignNamesAndSpawnPrefabs(targetLocations, selectedNameSet);
        startObjectInstance = InstantiateStartObject();
        UpdateTargetName(0);
    }

    private GameObject InstantiateStartObject()
    {
        if (startObjectPrefab != null)
        {
            GameObject startObject = Instantiate(startObjectPrefab, startObjectPosition, Quaternion.identity);
            startInteractable = startObject.GetComponent<XRBaseInteractable>();
            if (startInteractable != null)
            {
                startInteractable.selectEntered.AddListener(StartInteraction);
                startInteractable.selectExited.AddListener(EndInteraction);
            }
            return startObject;
        }
        return null;
    }

    private void StartInteraction(SelectEnterEventArgs args)
    {
        buttonReleased = false;
    }

    private void EndInteraction(SelectExitEventArgs args)
    {
        buttonReleased = true;
        if (!timerRunning)
        {
            StartTimer();
        }
    }

    private void StartTimer()
    {
        Debug.Log("Timer Starts");
        startTime = Time.time;
        timerRunning = true;
    }

    void Update()
    {
        CheckForTimerStop();
    }

    private void CheckForTimerStop()
    {
        if (timerRunning && buttonReleased && TryGetPrimaryButton(out bool primaryButtonValue) && primaryButtonValue)
        {
            StopTimer();
        }
    }

    private void StopTimer()
    {
        Debug.Log("Timer Ends");
        float elapsedTime = Time.time - startTime;
        timerRunning = false;
        buttonReleased = false;
        Vector3 controllerPosition = GetRightControllerPosition();
        LogTimeAndPosition(elapsedTime, controllerPosition);
    }

    private void LogTimeAndPosition(float time, Vector3 position)
    {
        string filePath = Application.persistentDataPath + "/interactionTimes.csv";
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine($"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}, {time}, {position.x}, {position.y}, {position.z}");
        }
        Debug.Log($"Logged time: {time} and position: {position}");
    }

    private Vector3 GetRightControllerPosition()
    {
        UnityEngine.XR.InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 position);
        return position;
    }

    private void AssignNamesAndSpawnPrefabs(List<Vector3> locations, string[] nameSet)
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
