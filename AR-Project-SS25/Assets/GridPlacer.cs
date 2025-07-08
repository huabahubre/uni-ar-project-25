using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using TMPro;

public class GridPlacer : MonoBehaviour
{
    public GameObject gridPrefab; // Assign a 3x3 grid prefab in the inspector
    private ARTrackedImageManager trackedImageManager;
    private GameObject spawnedGrid;
    
    public TextMeshProUGUI debugText;
    
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
        
        float offset = 5f; // Adjust the offset as needed
        Vector3 offsetPosition = trackedImage.transform.position + Vector3.back * offset; 

        if (spawnedGrid == null)
        {
            spawnedGrid = Instantiate(gridPrefab, offsetPosition, trackedImage.transform.rotation);
            spawnedGrid.transform.SetParent(trackedImage.transform);
            debugText.text = "Grid spawned at: " + spawnedGrid.transform.position.ToString("F2");
            //spawnedGrid.transform.position = offsetPosition;
        }
        else
        {
            Debug.Log("Grid already spawned, updating position.");
            spawnedGrid.transform.position = offsetPosition;
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
