using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
public class CraftingGridCell : MonoBehaviour
{
    public MarkerType cellType;
    
    public BoxCollider detectionCollider;
    
    [ShowInInspector, ReadOnly] public TrackedMarkerInfo assignedMarker;
    [ShowInInspector, ReadOnly] public TrackedMarkerInfo previousMarker;

    public Toggle worldToggle;
    public Image visualImage;

    private bool isWorldToggleOn = false;
    private float reassignmentCooldown = 0f;


    public Action<TrackedMarkerInfo> OnAssignedMarker;
    public Action OnRemovedMarker;
    
    
    // DEBUG TODO: Remove when ready!
    private TextMeshProUGUI debugText;
    private bool hasLoggedMarkers = false;

    
    private static readonly Dictionary<TrackedMarkerInfo, CraftingGridCell> markerAssignments = new();

    
    private void Start()
    {
        if (worldToggle != null)
        {
            worldToggle.onValueChanged.AddListener((isOn) =>
            {
                isWorldToggleOn = isOn;
                if (isOn)
                {
                    // Debug.Log("🔵 World toggle enabled for cell: " + name);
                    SpawnMarker();
                }
                else
                {
                    // Debug.Log("🔴 World toggle disabled for cell: " + name);
                    RemoveMarker();
                }
            });
        }

    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        if (reassignmentCooldown > 0f)
        {
            reassignmentCooldown -= Time.deltaTime;
            return;
        }

        TrackedMarkerInfo found = FindMarkerInOverlapBox();

        if (assignedMarker == null && found != null && found != previousMarker)
        {
            // Don't assign if another cell already owns this marker
            if (markerAssignments.TryGetValue(found, out var otherCell) && otherCell != this)
                return;

            assignedMarker = found;
            OnAssignedMarkerChanged();
        }
        else if (assignedMarker != null && (found == null || found != assignedMarker))
        {
            assignedMarker = null;
            OnAssignedMarkerChanged();
        }

        // UpdateDebugBox();
    }


    private TrackedMarkerInfo FindMarkerInOverlapBox()
    {
        if (detectionCollider == null)
            return null;

        Vector3 center = detectionCollider.bounds.center;
        Vector3 halfExtents = detectionCollider.bounds.extents;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, LayerMask.GetMask("Marker"));

        foreach (var hit in hits)
        {
            TrackedMarkerInfo marker = hit.GetComponent<TrackedMarkerInfo>();
            if (marker != null && marker.markerType == cellType)
            {
                return marker;
            }
        }

        return null;
    }



    private void OnAssignedMarkerChanged()
    {
        // Remove this cell from previous assignment if needed
        if (assignedMarker != null)
        {
            // Check if this marker is already claimed by another cell
            if (markerAssignments.TryGetValue(assignedMarker, out var existingCell))
            {
                if (existingCell != this)
                {
                    // Another cell already has this marker — cancel assignment
                    assignedMarker = null;
                    return;
                }
            }

            // Assign this marker to this cell
            markerAssignments[assignedMarker] = this;

            Debug.Log($"🟢 Marker assigned to cell '{name}': {assignedMarker.name}");

            // Set sprite transparency
            Color color = visualImage.color;
            color.a = 0f;
            visualImage.color = color;

            OnAssignedMarker?.Invoke(assignedMarker);
        }
        else
        {
            // Clear assignment
            if (previousMarker != null && markerAssignments.TryGetValue(previousMarker, out var owner) && owner == this)
            {
                markerAssignments.Remove(previousMarker);
            }

            Debug.Log($"🔴 Marker removed from cell '{name}'");

            Color color = visualImage.color;
            color.a = 1f;
            visualImage.color = color;

            OnRemovedMarker?.Invoke();
        }

        previousMarker = assignedMarker;

        PlayfieldManagement.Instance.CheckCraftingResult();
    }


    private void SpawnMarker()
    {
        if (assignedMarker != null || DataManagement.Instance.actionCardPrefab == null)
            return;

        TrackedMarkerInfo markerObj = Instantiate(
            DataManagement.Instance.actionCardPrefab,
            transform.position + Vector3.up * 0.01f,
            Quaternion.identity
        );

        markerObj.markerType = cellType;
        markerObj.gameObject.name = $"{cellType}_Marker_{name}";

        assignedMarker = markerObj;
        OnAssignedMarkerChanged();
    }

    private void RemoveMarker()
    {
        if (assignedMarker != null)
        {
            Destroy(assignedMarker.gameObject);
            assignedMarker = null;
            reassignmentCooldown = 0.1f; // Wait 1/10th second to let Unity clean up
            OnAssignedMarkerChanged();
        }
    }


    private void OnDrawGizmos()
    {
        if (assignedMarker != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.005f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.005f);
        }
    }
    
    
}
