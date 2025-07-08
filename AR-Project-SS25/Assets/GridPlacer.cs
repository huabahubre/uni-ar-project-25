using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class GridPlacer : MonoBehaviour
{
    public GameObject gridPrefab; // Assign a 3x3 grid prefab in the inspector
    private ARTrackedImageManager trackedImageManager;
    private GameObject spawnedGrid;
    
    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            PlaceGrid(trackedImage);
        }

        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                UpdateGridPosition(trackedImage);
            }
            else if (trackedImage.trackingState == TrackingState.None)
            {
                if (spawnedGrid) spawnedGrid.SetActive(false);
            }
        }
    }
    
    void PlaceGrid(ARTrackedImage trackedImage)
    {
        if (gridPrefab == null) return;

        if (spawnedGrid == null)
        {
            spawnedGrid = Instantiate(gridPrefab, trackedImage.transform.position, trackedImage.transform.rotation);
            spawnedGrid.transform.SetParent(trackedImage.transform);
        }
        else
        {
            spawnedGrid.transform.position = trackedImage.transform.position;
            spawnedGrid.transform.rotation = trackedImage.transform.rotation;
            spawnedGrid.SetActive(true);
        }
    }
    
    void UpdateGridPosition(ARTrackedImage trackedImage)
    {
        if (spawnedGrid != null)
        {
            spawnedGrid.transform.position = trackedImage.transform.position;
            spawnedGrid.transform.rotation = trackedImage.transform.rotation;
        }
    }
    
}
