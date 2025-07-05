using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems; 

public class MarkerTracker : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager aRTrackedImageManager;
    [SerializeField] private GameObject aRModelToAnchor;

    [SerializeField] private int trackerCount;

    private Dictionary<string, GameObject> _aRModels = new Dictionary<string, GameObject>();
    private Dictionary<string, bool> _modelState = new Dictionary<string, bool>();
    
    
    
    private void OnEnable()
    {
        aRTrackedImageManager.trackedImagesChanged += ImageFound;
    }

    private void OnDisable()
    {
        aRTrackedImageManager.trackedImagesChanged -= ImageFound;
    }
    
    void Start()
    {
        
        for (int i = 0; i < trackerCount; i++)
        {
            GameObject newARModel = Instantiate(aRModelToAnchor, Vector3.zero, Quaternion.identity);
            newARModel.name = aRModelToAnchor.name + "_" + i;
            _aRModels.Add(newARModel.name, newARModel);
            newARModel.SetActive(false);
            _modelState.Add(newARModel.name, false);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void ImageFound(ARTrackedImagesChangedEventArgs obj)
    {
        foreach (var trackedImage in obj.added)
        {
            SpawnARModel(trackedImage);
        }
        foreach (var trackedImage in obj.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                SpawnARModel(trackedImage);
            }
            else if (trackedImage.trackingState == TrackingState.Limited)
            {
                HideARModel(trackedImage);
            }
        }
    }

    private void SpawnARModel(ARTrackedImage trackedImage)
    {
        bool isActive = _modelState[trackedImage.referenceImage.name];
        if (!isActive)
        {
            GameObject aRModel = _aRModels[trackedImage.referenceImage.name];
            aRModel.transform.position = trackedImage.transform.position;
            aRModel.SetActive(true);
            _modelState[trackedImage.referenceImage.name] = true;
        }
        else
        {
            GameObject aRModel = _aRModels[trackedImage.referenceImage.name];
            aRModel.transform.position = trackedImage.transform.position;
        }
    }
    
    private void HideARModel(ARTrackedImage trackedImage)
    {
        bool isActive = _modelState[trackedImage.referenceImage.name];
        if (isActive)
        {
            GameObject aRModel = _aRModels[trackedImage.referenceImage.name];
            aRModel.SetActive(false);
            _modelState[trackedImage.referenceImage.name] = false;
        }
    }
}
