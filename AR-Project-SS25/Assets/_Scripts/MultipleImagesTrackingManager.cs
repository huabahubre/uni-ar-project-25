using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MultipleImagesTrackingManager : MonoBehaviour
{
    [SerializeField]
    private List<TrackedObjectMapping> trackedObjects; // Custom struct to define name->object

    private ARTrackedImageManager _trackedImageManager;

    // Internal dictionary: marker name -> scene object
    private Dictionary<string, DynTrackedMarkerParent> _trackedObjectDict = new Dictionary<string, DynTrackedMarkerParent>();


    private void Awake()
    {
        _trackedImageManager = GetComponent<ARTrackedImageManager>();

        foreach (var item in trackedObjects)
        {
            if (item.targetObject != null && !string.IsNullOrEmpty(item.imageName))
            {
                // _trackedObjectDict[item.imageName] = item.targetObject;
                // item.targetObject.SetVisible(false); // Start all as hidden
                item.targetObject.gameObject.SetActive(false);
                _trackedObjectDict.Add(item.imageName, item.targetObject);
            }
        }
    }

    private void OnEnable()
    {
        _trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        _trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    
    
    
    void Update()
    {
        // foreach (var kvp in _trackedObjectDict)
        // {
        //     string imageName = kvp.Key;
        //     var trackedImage = kvp.Value;
        //
        //     if (_trackedObjectDict.TryGetValue(imageName, out var obj))
        //     {
        //         var dyn = obj.GetComponent<DynTrackedMarkerParent>();
        //         if (dyn != null)
        //         {
        //             dyn.UpdateTransform(trackedImage.transform);
        //         }
        //     }
        // }
    }

    

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            UpdateTrackedImages(trackedImage);
        }

        foreach (var trackedImage in args.updated)
        {
            UpdateTrackedImages(trackedImage);
        }

        foreach (var trackedImage in args.removed)
        {
            UpdateTrackedImages(trackedImage);
        }
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



    [Serializable]
    public struct TrackedObjectMapping
    {
        public string imageName;
        public DynTrackedMarkerParent targetObject;
    }
}
