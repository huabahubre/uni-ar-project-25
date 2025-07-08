using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARDebugOverlay : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    public MultipleImagesTrackingManager trackingManager;

    public Transform xrOrigin;
    public Transform mainCamera;

    void Update()
    {
        if (debugText == null || trackingManager == null) return;

        string output = $"[AR DEBUG INFO]\n\n";

        // XR Origin info
        if (xrOrigin != null)
        {
            output += $"XR Origin:\n  Pos: {xrOrigin.position:F2}\n  Rot: {xrOrigin.eulerAngles:F1}\n  Scale: {xrOrigin.localScale}\n\n";
        }

        // Camera info
        if (mainCamera != null)
        {
            output += $"Main Camera:\n  Pos: {mainCamera.position:F2}\n  Rot: {mainCamera.eulerAngles:F1}\n\n";
        }

        // Tracked images and visuals
        foreach (var kvp in trackingManager.GetTrackedImages())
        {
            string name = kvp.Key;
            ARTrackedImage trackedImage = kvp.Value;
            // GameObject visual = trackingManager.GetVisualForImage(name).gameObject;

            output += $"Marker: {name}\n";
            output += $"  State: {trackedImage.trackingState}\n";
            output += $"  Marker Pos: {trackedImage.transform.position:F2}\n";
            output += $"  Marker Rot: {trackedImage.transform.eulerAngles:F1}\n";
            output += $"  Marker Parent: {trackedImage.transform.parent?.name ?? "null"}\n";

            // if (visual != null)
            // {
            //     output += $"  Visual Pos: {visual.transform.position:F2}\n";
            //     output += $"  Visual Rot: {visual.transform.eulerAngles:F1}\n";
            //     output += $"  Visual Parent: {visual.transform.parent?.name ?? "null"}\n";
            // }

            output += "\n";
        }

        debugText.text = output;
    }
}