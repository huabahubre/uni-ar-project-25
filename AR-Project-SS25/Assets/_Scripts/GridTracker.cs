using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class GridTracker : MonoBehaviour
{
    ARTrackedImageManager trackedImageManager;

    // Grid size and thresholds
    float cellSize = 0.1f;
    float threshold = 0.05f; 

    Dictionary<Vector2Int, string> occupiedCells = new();
    
    private Dictionary<string, bool> markerVisibility = new();
    
    // debugging
    [SerializeField] private GameObject cubePrefab;
    private Dictionary<TrackableId, GameObject> spawnedCubes = new();
    
    [SerializeField] private TextMeshProUGUI debugText;
    private int counter = 0;

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnChanged);
    }

    void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnChanged);
    }

    private void Start()
    {
        debugText.text = "Started App";
        
        string[] markerNames = {
            "Cell_0_0", "Cell_0_1", "Cell_0_2",
            "Cell_1_0", "Cell_1_1", "Cell_1_2",
            "Cell_2_0", "Cell_2_1", "Cell_2_2"
        };

        foreach (var _name in markerNames)
        {
            markerVisibility[_name] = false;
        }
    }

    private void Update()
    {
        LogVisibleMarkers();
    }

    void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            //GameObject cube = Instantiate(cubePrefab, newImage.transform);
            //cube.transform.localPosition = Vector3.zero; // center of the image
            //spawnedCubes[newImage.trackableId] = cube;
            counter++;
            
            debugText.text = "Found new image. Count: " + counter + ", Id: " + newImage.trackableId.ToString();
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            string _name = trackedImage.referenceImage.name;

            if (markerVisibility.ContainsKey(_name))
            {
                markerVisibility[_name] = trackedImage.trackingState == TrackingState.Tracking;
            }
        }

        foreach (var removedImage in eventArgs.removed)
        {
            if (spawnedCubes.TryGetValue(removedImage.Key, out var cube))   
            {
                Destroy(cube);
                spawnedCubes.Remove(removedImage.Key);
                --counter;
                
                debugText.text = "Lost image: " + removedImage.Key.ToString();
            }
        }
    }
    
    void LogVisibleMarkers()
    {
        string text = "Visible Markers:\n";
        foreach (var kvp in markerVisibility)
        {
            if (kvp.Value)
                text += kvp.Key + "\n";
        }
        debugText.text = text;
    }
}
