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
    
    [SerializeField] private GridManagement gridManagement;
    [SerializeField] private GameObject cardPrefab;
    
    private Dictionary<string, GameObject> spawnedMarkers = new Dictionary<string, GameObject>();
    
    public TextMeshProUGUI debugText;
    
    // marker names
    private string air = "qr-code_air";
    private string water = "qr-code_water";
    private string earth = "qr-code_earth";
    private string fire = "qr-code_fire";
    private string anchor = "marker0";
    private string card = "marker1";
    
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
            if (trackedImage.referenceImage.name == anchor)
            {
                debugText.text = "Anchor detected: " + trackedImage.referenceImage.name;
                PlaceGrid(trackedImage);
            }
            else if (trackedImage.referenceImage.name == card)
            {
                debugText.text = "Card detected: " + trackedImage.referenceImage.name;
                GameObject cardInstance = Instantiate(cardPrefab, trackedImage.transform.position, trackedImage.transform.rotation);
                spawnedMarkers[trackedImage.referenceImage.name] = cardInstance;
                cardInstance.transform.SetParent(trackedImage.transform);
                debugText.text += " Card instance created.";
                //gridManagement.RegisterMarker(trackedImage);
            }
            else if (trackedImage.referenceImage.name == air || trackedImage.referenceImage.name == water ||
                     trackedImage.referenceImage.name == earth || trackedImage.referenceImage.name == fire)
            {
                debugText.text = "Element detected: " + trackedImage.referenceImage.name;
                //gridManagement.RegisterMarker(trackedImage);
            }
            else
            {
                debugText.text = "Unknown marker detected: " + trackedImage.referenceImage.name;
            }
        }

        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                if (trackedImage.referenceImage.name == anchor)
                {
                    UpdateGridPosition(trackedImage);
                    debugText.text = "Anchor updated: " + trackedImage.referenceImage.name;
                }
                else if (trackedImage.referenceImage.name == card)
                {
                    spawnedMarkers[trackedImage.referenceImage.name].transform.position = trackedImage.transform.position;
                    debugText.text = "Card updated: " + trackedImage.referenceImage.name;
                    //gridManagement.UpdateMarker(trackedImage);
                }
                else if (trackedImage.referenceImage.name == air || 
                         trackedImage.referenceImage.name == water || 
                         trackedImage.referenceImage.name == earth || 
                         trackedImage.referenceImage.name == fire)
                {
                    debugText.text = "Element updated: " + trackedImage.referenceImage.name;
                    //gridManagement.UpdateMarker(trackedImage);
                }
                UpdateGridPosition(trackedImage);
            }
            else if (trackedImage.trackingState == TrackingState.None)
            {
                debugText.text = "Marker lost: " + trackedImage.referenceImage.name;
                if (trackedImage.referenceImage.name == anchor)
                {
                    debugText.text = "Anchor lost, hiding grid.";
                    if (spawnedGrid) spawnedGrid.SetActive(false);
                }
                else if (trackedImage.referenceImage.name == card || 
                         trackedImage.referenceImage.name == air || 
                         trackedImage.referenceImage.name == water || 
                         trackedImage.referenceImage.name == earth || 
                         trackedImage.referenceImage.name == fire)
                {
                    debugText.text = "Element or card marker lost.";
                    //gridManagement.UnregisterMarker(trackedImage);
                }
            }
        }

        foreach (var trackedImage in args.removed)
        {
            if (trackedImage.referenceImage.name == anchor)
            {
                debugText.text = "Anchor removed: " + trackedImage.referenceImage.name;
            }
            else if (trackedImage.referenceImage.name == card)
            {
                debugText.text = "Card removed: " + trackedImage.referenceImage.name;
                if (spawnedMarkers.ContainsKey(trackedImage.referenceImage.name))
                {
                    Destroy(spawnedMarkers[trackedImage.referenceImage.name]);
                    spawnedMarkers.Remove(trackedImage.referenceImage.name);
                }
            }
            else if (trackedImage.referenceImage.name == air || 
                     trackedImage.referenceImage.name == water || 
                     trackedImage.referenceImage.name == earth || 
                     trackedImage.referenceImage.name == fire)
            {
                debugText.text = "Element marker removed: " + trackedImage.referenceImage.name;
                //gridManagement.UnregisterMarker(trackedImage);
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
