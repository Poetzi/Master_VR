using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Globalization;

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
    [SerializeField] private int participant;
    [SerializeField] private bool automaticBoxCenter;
    [SerializeField] private bool showGizmosInVR = false;
    [SerializeField] private List<Vector3> manualLocations;


    [SerializeField] private string[] nameSet; // Names set through the Unity Inspector
    private Dictionary<string, GameObject> instantiatedObjects = new Dictionary<string, GameObject>();
    private int currentTargetIndex = 0;
    private int currentCycle = 0;
    private bool timerRunning = false;
    private bool buttonReleased = true;
    private float startTime;
    private GameObject startObjectInstance;
    private int entryNumber = 1;
    private Vector3 firstClickControllerPosition;
    private Dictionary<string, Vector3> subBoxIndices = new Dictionary<string, Vector3>();
    private bool isInitialized = false;
    private GameObject gizmoContainer;
    private Renderer startObjectRenderer; // Renderer for the start object

    void Start()
    {
        
    }

    private bool TryGenerateObjects()
    {
        int numberOfObjectsToSpawn = nameSet.Length;
        List<Vector3> targetLocations;

        if (automaticBoxCenter)
        {
            targetLocations = GenerateUniqueLocations(numberOfObjectsToSpawn, boxSize, minDistance);
        }
        else
        {
            targetLocations = new List<Vector3>(manualLocations);
        }

        if (targetLocations.Count < numberOfObjectsToSpawn)
        {
            Debug.LogError("Not enough locations generated or provided.");
            return false;
        }

        ShuffleNameSet();
        AssignNamesAndSpawnPrefabs(targetLocations, nameSet);
        startObjectInstance = InstantiateStartObject();
        UpdateTargetName(currentTargetIndex);
        return true;
    }

    private void ShuffleNameSet()
    {
        ShuffleArray(nameSet);
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

        // Get the Renderer component of the instantiated start object
        startObjectRenderer = startObject.GetComponent<Renderer>();
        Debug.Log(startObjectRenderer);
        // Initialize the color to blue at instantiation
        UpdateStartObjectColor();

        return startObject;
    }

    private void UpdateStartObjectColor()
    {
        if (startObjectRenderer != null)
        {
            Debug.Log("Renderer is true");
            Debug.Log(startObjectRenderer.material.color);
           startObjectRenderer.material.SetColor("_BaseColor", timerRunning ? Color.red : Color.blue);
        }
    }

    private void StartInteraction(SelectEnterEventArgs args)
    {
        buttonReleased = false;
        firstClickControllerPosition = GetRightControllerPosition(); // Capture the controller position at the moment of interaction
    }

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
        UpdateStartObjectColor(); // Update color when the timer starts
    }

    void Update()
    {
        if (!isInitialized && TryGetPrimaryButton(out bool primaryButtonPressed) && primaryButtonPressed)
        {
            if(automaticBoxCenter) { 
                boxCenter = GetRightControllerPosition();
            }

            if (showGizmosInVR)
                CreateGizmoLines();

            InitializeEnvironment();
            isInitialized = true;
        }

        if (timerRunning && buttonReleased && TryGetPrimaryButton(out bool primaryButtonValue) && primaryButtonValue)
         StopTimer(); 
    }

    private void InitializeEnvironment()
    {
        if (!TryGenerateObjects())
        {
            Debug.LogError("Failed to generate enough unique locations. Retrying...");
            InitializeEnvironment();
        }
    }

    private void StopTimer()
    {
        Debug.Log("Timer Ends");
        timerRunning = false;
        UpdateStartObjectColor(); // Update color when the timer stops
        buttonReleased = false;
        float elapsedTime = Time.time - startTime;
        string targetName = nameSet[currentTargetIndex];
        Vector3 controllerPosition = GetRightControllerPosition();
        Vector3 targetObjectPosition = instantiatedObjects[targetName].transform.position;
        Vector3 startObjectPosition = startObjectInstance.transform.position;
        LogTimePositionAndTarget(elapsedTime, controllerPosition, targetObjectPosition, targetName, startObjectPosition, firstClickControllerPosition);

        if (++currentTargetIndex >= nameSet.Length)
        {
            currentTargetIndex = 0;
            if (++currentCycle >= maxCycles)
            {
                Debug.Log("Max cycles reached. Initiating scene transition.");
                sceneTransitionManager.GoToSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
                return;
            }
            ShuffleNameSet();
        }
        UpdateTargetName(currentTargetIndex);
    }

    private void LogTimePositionAndTarget(float time, Vector3 controllerPosition, Vector3 targetPosition, string targetName, Vector3 startObjectPosition, Vector3 firstInteractionControllerPosition)
    {
        string date = System.DateTime.Now.ToString("yyyyMMdd");
        Scene currentScene = SceneManager.GetActiveScene();
        int sceneIndex = currentScene.buildIndex;
        string sceneName = currentScene.name;
        string filename = $"Participant_{participant}_{sceneName}_{date}_log.csv";
        string filePath = Path.Combine(Application.persistentDataPath, filename);
        bool fileExists = File.Exists(filePath);
        Vector3 subBoxIndex = subBoxIndices[targetName];

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            if (!fileExists || new FileInfo(filePath).Length == 0)
            {
                writer.WriteLine("Entry Number, Participant, Block, SceneIndex, SceneName, Timestamp, ElapsedTime(s), ControllerX, ControllerY, ControllerZ, TargetName, TargetX, TargetY, TargetZ, StartObjectX, StartObjectY, StartObjectZ, FirstInteractionControllerX, FirstInteractionControllerY, FirstInteractionControllerZ, BoxCenterX, BoxCenterY, BoxCenterZ, SubBoxX, SubBoxY, SubBoxZ");
            }
            writer.WriteLine($"{entryNumber}; {participant}; {currentCycle}; {sceneIndex}; {sceneName}; {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}; {time.ToString("F3", CultureInfo.InvariantCulture)}; {controllerPosition.x.ToString("F3", CultureInfo.InvariantCulture)}; {controllerPosition.y.ToString("F3", CultureInfo.InvariantCulture)}; {controllerPosition.z.ToString("F3", CultureInfo.InvariantCulture)}; {targetName}; {targetPosition.x.ToString("F3", CultureInfo.InvariantCulture)}; {targetPosition.y.ToString("F3", CultureInfo.InvariantCulture)}; {targetPosition.z.ToString("F3", CultureInfo.InvariantCulture)}; {startObjectPosition.x.ToString("F3", CultureInfo.InvariantCulture)}; {startObjectPosition.y.ToString("F3", CultureInfo.InvariantCulture)}; {startObjectPosition.z.ToString("F3", CultureInfo.InvariantCulture)}; {firstInteractionControllerPosition.x.ToString("F3", CultureInfo.InvariantCulture)}; {firstInteractionControllerPosition.y.ToString("F3", CultureInfo.InvariantCulture)}; {firstInteractionControllerPosition.z.ToString("F3", CultureInfo.InvariantCulture)}; {boxCenter.x.ToString("F3", CultureInfo.InvariantCulture)}; {boxCenter.y.ToString("F3", CultureInfo.InvariantCulture)}; {boxCenter.z.ToString("F3", CultureInfo.InvariantCulture)};  {subBoxIndex.x}; {subBoxIndex.y}; {subBoxIndex.z}");
            entryNumber++;
        }
    }

    private Vector3 CalculateSubBoxIndex(Vector3 location)
    {
        Vector3 relativePosition = location - (boxCenter - boxSize / 2);
        int xIndex = Mathf.FloorToInt(relativePosition.x / (boxSize.x / 3));
        int yIndex = Mathf.FloorToInt(relativePosition.y / (boxSize.y / 3));
        int zIndex = Mathf.FloorToInt(relativePosition.z / (boxSize.z / 3));
        return new Vector3(xIndex, yIndex, zIndex);
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
        // Iterate through all the names in the nameSet
        for (int i = 0; i < nameSet.Length; i++)
        {
            SpawnablePrefab prefab = spawnablePrefabs.Find(p => p.name == nameSet[i]);
            if (prefab != null && i < locations.Count) // Check if the location exists for the object
            {
                GameObject instantiatedPrefab = Instantiate(prefab.prefab, locations[i], Quaternion.identity);
                instantiatedObjects.Add(prefab.name, instantiatedPrefab);

                // Calculate subbox index here (example for storing purposes)
                Vector3 subBoxIndex = CalculateSubBoxIndex(locations[i]);
                subBoxIndices.Add(prefab.name, subBoxIndex);
            }
            else
            {
                Debug.LogError($"Prefab for {nameSet[i]} could not be found or there is no corresponding location.");
            }
        }
    }

    private List<Vector3> GenerateUniqueLocations(int count, Vector3 size, float minDist)
    {
        List<Vector3> locations = new List<Vector3>();
        int maxAttempts = 10000;
        for (int i = 0; i < count && maxAttempts > 0; --maxAttempts)
        {
            Vector3 potentialLocation = GenerateRandomPoint(size);
            if (IsValidLocation(locations, potentialLocation, minDist))
            {
                locations.Add(potentialLocation);
                i++; // Increment only if a valid location is added
            }
        }

        // Check if we have enough locations, if not, log an error
        if (locations.Count < count)
        {
            Debug.LogError("Couldn't generate the required number of unique locations.");
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
        if (targetIndex < nameSet.Length)
        {
            targetNameText.text = nameSet[targetIndex];
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

        // Calculate the step size for each grid cell in each dimension
        float stepX = boxSize.x / 3;
        float stepY = boxSize.y / 3;
        float stepZ = boxSize.z / 3;

        // Starting corner of the grid
        Vector3 gridStart = boxCenter - boxSize / 2;

        // Draw lines along the x-axis
        for (int x = 0; x <= 3; x++)
        {
            for (int y = 0; y <= 3; y++)
            {
                Vector3 start = gridStart + new Vector3(x * stepX, y * stepY, 0);
                Vector3 end = start + new Vector3(0, 0, boxSize.z);
                Gizmos.DrawLine(start, end);
            }
        }

        // Draw lines along the y-axis
        for (int y = 0; y <= 3; y++)
        {
            for (int z = 0; z <= 3; z++)
            {
                Vector3 start = gridStart + new Vector3(0, y * stepY, z * stepZ);
                Vector3 end = start + new Vector3(boxSize.x, 0, 0);
                Gizmos.DrawLine(start, end);
            }
        }

        // Draw lines along the z-axis
        for (int z = 0; z <= 3; z++)
        {
            for (int x = 0; x <= 3; x++)
            {
                Vector3 start = gridStart + new Vector3(x * stepX, 0, z * stepZ);
                Vector3 end = start + new Vector3(0, boxSize.y, 0);
                Gizmos.DrawLine(start, end);
            }
        }
    }

    private void CreateGizmoLines()
    {
        gizmoContainer = new GameObject("GizmoContainer");

        // Calculate steps for grid
        float stepX = boxSize.x / 3;
        float stepY = boxSize.y / 3;
        float stepZ = boxSize.z / 3;

        // Draw lines for the box grid
        DrawGrid(stepX, stepY, stepZ);
    }

    private void DrawGrid(float stepX, float stepY, float stepZ)
    {
        Vector3 gridStart = boxCenter - boxSize / 2;
        for (int i = 0; i <= 3; i++)
        {
            for (int j = 0; j <= 3; j++)
            {
                CreateLineRenderer(new Vector3(gridStart.x + i * stepX, gridStart.y + j * stepY, gridStart.z),
                                   new Vector3(gridStart.x + i * stepX, gridStart.y + j * stepY, gridStart.z + boxSize.z));

                CreateLineRenderer(new Vector3(gridStart.x, gridStart.y + i * stepY, gridStart.z + j * stepZ),
                                   new Vector3(gridStart.x + boxSize.x, gridStart.y + i * stepY, gridStart.z + j * stepZ));

                CreateLineRenderer(new Vector3(gridStart.x + i * stepX, gridStart.y, gridStart.z + j * stepZ),
                                   new Vector3(gridStart.x + i * stepX, gridStart.y + boxSize.y, gridStart.z + j * stepZ));
            }
        }
    }

    private void CreateLineRenderer(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.parent = gizmoContainer.transform;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPositions(new Vector3[] { start, end });
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.white;
        lr.endColor = Color.white;
    }
}
