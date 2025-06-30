using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GridTracker : MonoBehaviour
{
    ARTrackedImageManager trackedImageManager;

    // Grid size and thresholds
    float cellSize = 0.1f;
    float threshold = 0.05f; 

    Dictionary<Vector2Int, string> occupiedCells = new();
    
    // debugging
    [SerializeField] private GameObject cubePrefab;
    private Dictionary<TrackableId, GameObject> spawnedCubes = new();

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

    void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            GameObject cube = Instantiate(cubePrefab, newImage.transform);
            cube.transform.localPosition = Vector3.zero; // center of the image
            spawnedCubes[newImage.trackableId] = cube;
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            // Handle updated event
        }

        foreach (var removedImage in eventArgs.removed)
        {
            if (spawnedCubes.TryGetValue(removedImage.Key, out var cube))
            {
                Destroy(cube);
                spawnedCubes.Remove(removedImage.Key);
            }
        }
    }
}
