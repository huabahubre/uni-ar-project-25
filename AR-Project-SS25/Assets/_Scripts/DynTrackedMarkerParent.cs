using System;
using System.Collections.Generic;
using UnityEngine;

public class DynTrackedMarkerParent : MonoBehaviour
{
    [Tooltip("Optional: Add contents that should be toggled independently")]
    public List<GameObject> contents;

    public Action onShow;
    public Action onHide;

    private bool isVisible = false;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    [Header("Smooth Follow Settings")]
    public float positionLerpSpeed = 5f;
    public float rotationLerpSpeed = 5f;

    private void Update()
    {
        if (!isVisible) return;

        // Smoothly interpolate toward the tracked marker's position/rotation
        // transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);
        // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
    }

    public void OnTracked()
    {
        Debug.Log($"[Tracking] {gameObject.name} tracked.");
        if(isVisible) return;
        
        isVisible = true;
        SetVisible(true);
    }

    public void OnLostTracking()
    {
        Debug.Log($"[Tracking] {gameObject.name} lost.");
        if(!isVisible) return;
        
        isVisible = false;
        SetVisible(false);
    }

    public void UpdateTransform(Transform markerTransform)
    {
        if (!isVisible) return;

        // Set target for smooth interpolation
        UpdateTargetPosition(markerTransform.position, markerTransform.rotation);
    }

    public void UpdateTargetPosition(Vector3 targetPosition, Quaternion targetRotation)
    {
        this.targetPosition = targetPosition;
        this.targetRotation = targetRotation;
    }

    public void SetVisible(bool visible)
    {
        foreach (var go in contents)
        {
            if (go != null)
                go.SetActive(visible);
        }
    }
}