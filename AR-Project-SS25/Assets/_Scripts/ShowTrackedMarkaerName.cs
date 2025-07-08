using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ShowTrackedMarkerName : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI;

    private ARTrackedImageManager trackedImageManager;

    // Keeps track of currently tracked image names
    private readonly HashSet<string> currentlyTrackedImages = new HashSet<string>();

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

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                currentlyTrackedImages.Add(trackedImage.referenceImage.name);
            }
            Debug.Log($"[AR] Added image: {trackedImage.referenceImage.name}");
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            Debug.Log($"[AR] Updated image: {trackedImage.referenceImage.name}, tracking: {trackedImage.trackingState}");

            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                currentlyTrackedImages.Add(trackedImage.referenceImage.name);
            }
            else
            {
                currentlyTrackedImages.Remove(trackedImage.referenceImage.name);
            }
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            currentlyTrackedImages.Remove(trackedImage.referenceImage.name);
            Debug.Log($"[AR] Removed image: {trackedImage.referenceImage.name}");
        }

        UpdateUIText();
    }

    void UpdateUIText()
    {
        if (textMeshProUGUI != null)
        {
            if (currentlyTrackedImages.Count == 0)
            {
                textMeshProUGUI.text = "No markers tracked";
            }
            else
            {
                textMeshProUGUI.text = "Tracked markers:\n" + string.Join("\n", currentlyTrackedImages);
            }
        }
    }
}
