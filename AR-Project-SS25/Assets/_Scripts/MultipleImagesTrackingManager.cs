using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MultipleImagesTrackingManager : MonoBehaviour
{
    
    public TextMeshProUGUI textMeshProUGUI;
    private readonly HashSet<string> currentlyTrackedImages = new HashSet<string>();


    
    
    [SerializeField]
    private List<TrackedObjectMapping> spawnTrackedPrefabs;
    
    private ARTrackedImageManager _trackedImageManager;
    
    private Dictionary<string, GameObject> _trackedObjectDict;
    private Dictionary<string, ARTrackedImage> _trackedImageData = new();

    public Dictionary<string, ARTrackedImage> GetTrackedImages() => _trackedImageData;
    public GameObject GetVisualForImage(string name) => _trackedObjectDict.ContainsKey(name) ? _trackedObjectDict[name] : null;


    private void Awake()
    {
        _trackedImageManager = GetComponent<ARTrackedImageManager>();
        if (_trackedImageManager == null) return;
        
        // Setup Tracked Object Dictionary
        _trackedObjectDict = new Dictionary<string, GameObject>();
        
        // Spawn scene objects
        SpawnSceneTrackedObjects();
            
        // _trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
        _trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }


    private void OnDisable()
    {
        // _trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        _trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }



    
    void SpawnSceneTrackedObjects()
    {
        foreach (var entry in spawnTrackedPrefabs)
        {
            var newARPrefab = Instantiate(entry.prefab, Vector3.zero, Quaternion.identity);
            newARPrefab.name = "TrackedImageObject-" + entry.imageName;
            newARPrefab.gameObject.SetActive(false);
            // newARPrefab.SetVisible(false);
            _trackedObjectDict.Add(newARPrefab.name, newARPrefab);
        }
    }
    

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        return;
        
        // Added
        foreach (var trackedImage in args.added)
        {
            UpdateTrackedImages(trackedImage);
        }

        // Updated
        foreach (var trackedImage in args.updated)
        {
            UpdateTrackedImages(trackedImage);
        }

        // Removed
        foreach (var trackedImage in args.removed)
        {
            UpdateTrackedImages(trackedImage.Value);
        }
    }
    
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                currentlyTrackedImages.Add(trackedImage.referenceImage.name);
                _trackedImageData[trackedImage.referenceImage.name] = trackedImage;
            }
            else
            {
                _trackedImageData.Remove(trackedImage.referenceImage.name);
            }

            Debug.Log($"[AR] Added image: {trackedImage.referenceImage.name}");
            
            UpdateTrackedImages(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            Debug.Log($"[AR] Updated image: {trackedImage.referenceImage.name}, tracking: {trackedImage.trackingState}");

            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                currentlyTrackedImages.Add(trackedImage.referenceImage.name);
                _trackedImageData[trackedImage.referenceImage.name] = trackedImage;
            }
            else
            {
                currentlyTrackedImages.Remove(trackedImage.referenceImage.name);
                _trackedImageData.Remove(trackedImage.referenceImage.name);
            }
            
            
            UpdateTrackedImages(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            currentlyTrackedImages.Remove(trackedImage.referenceImage.name);
            _trackedImageData.Remove(trackedImage.referenceImage.name);

            Debug.Log($"[AR] Removed image: {trackedImage.referenceImage.name}");
            
            UpdateTrackedImages(trackedImage);
        }

        UpdateUIText();
    }
    

    private void UpdateTrackedImages(ARTrackedImage trackedImage)
    {
        if(trackedImage == null) return;
        
        // Skip if the image is not in our dictionary
        if (!_trackedObjectDict.ContainsKey(trackedImage.referenceImage.name))
        {
            Debug.Log($"Tracked image {trackedImage.referenceImage.name} not found in dictionary.");
            return;
        }

        // Disable object if tracking state is limited or none
        if (trackedImage.trackingState is TrackingState.Limited or TrackingState.None)
        {
            // _trackedObjectDict[trackedImage.referenceImage.name].OnLostTracking();
            _trackedObjectDict[trackedImage.referenceImage.name].gameObject.SetActive(false);
            return;
        }
        
        // Enable and sync position/rotation if tracking is good
        // _trackedObjectDict[trackedImage.referenceImage.name].OnTracked();
        // _trackedObjectDict[trackedImage.referenceImage.name].UpdateTargetPosition(trackedImage.transform.position, trackedImage.transform.rotation);
        
        _trackedObjectDict[trackedImage.referenceImage.name].gameObject.SetActive(true);
        _trackedObjectDict[trackedImage.referenceImage.name].transform.position = trackedImage.transform.position;
        _trackedObjectDict[trackedImage.referenceImage.name].transform.rotation = trackedImage.transform.rotation;
    }

    
    
    
    private void UpdateUIText()
    {
        if (textMeshProUGUI == null) return;

        if (_trackedImageData.Count == 0)
        {
            textMeshProUGUI.text = "No markers tracked.";
            return;
        }

        string output = "Tracked markers:\n";

        foreach (var kvp in _trackedImageData)
        {
            var name = kvp.Key;
            var image = kvp.Value;

            Vector3 trackedPos = image.transform.position;
            Vector3 trackedRot = image.transform.eulerAngles;

            output += $"- {name}\n";
            output += $"  Pos: {trackedPos:F2}\n";
            output += $"  Rot: {trackedRot:F1}\n";

            if (_trackedObjectDict.TryGetValue(name, out var visualObj))
            {
                Vector3 visualPos = visualObj.transform.position;
                Vector3 visualRot = visualObj.transform.eulerAngles;

                output += $"  Visual Pos: {visualPos:F2}\n";
                output += $"  Visual Rot: {visualRot:F1}\n";

                Debug.Log($"[AR] {name} -> Marker Pos: {trackedPos}, Visual Pos: {visualPos}");
                Debug.Log($"[AR] {name} -> Marker Rot: {trackedRot}, Visual Rot: {visualRot}");
            }
        }

        textMeshProUGUI.text = output;
    }




    [Serializable]
    public struct TrackedObjectMapping
    {
        public string imageName;
        public GameObject prefab;
        // public GameObject targetScript;
    }
}
