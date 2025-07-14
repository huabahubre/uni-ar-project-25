using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
public class CraftingGridCell : MonoBehaviour
{
    public MarkerType cellType;

    // [Header("Detection Zone (Cube, scales with transform)")]
    // public Vector3 baseDetectionBoxSize = new Vector3(0.05f, 0.05f, 0.05f);

    [ShowInInspector, ReadOnly] public TrackedMarkerInfo assignedMarker;
    [ShowInInspector, ReadOnly] public TrackedMarkerInfo previousMarker;

    public Toggle worldToggle;
    public Image visualImage;

    private bool isWorldToggleOn = false;
    private float reassignmentCooldown = 0f;


    public Action<TrackedMarkerInfo> OnAssignedMarker;
    public Action OnRemovedMarker;
    
    
    
    
    [BoxGroup("Raycast Settings")]
    public float raycastLength = 10f;

    [BoxGroup("Raycast Settings")]
    public float debugCubeScaleXZ = 0.02f;


    
    // DEBUG TODO: Remove when ready!
    private TextMeshProUGUI debugText;
    private bool hasLoggedMarkers = false;


    
    [BoxGroup("DEBUG")]
    public GameObject debugRaycast;
    
    
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

            
        // Raycast down from the center of the cell to detect a marker below
        // RaycastHit hit;
        // if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, LayerMask.GetMask("Marker")))
        // {
        //     Debug.LogError("Raycast hit 1");
        //     TrackedMarkerInfo markerInfo = hit.collider.GetComponent<TrackedMarkerInfo>();
        //     if (markerInfo != null)
        //     {
        //         assignedMarker = markerInfo;
        //         Debug.LogError($"🟡 Raycast found marker below cell '{name}': {markerInfo.name}");
        //         OnAssignedMarkerChanged();
        //     }
        // } 
        // else if (Physics.Raycast(transform.position, Vector3.up, out hit, 10f, LayerMask.GetMask("Marker")))
        // {
        //     Debug.LogError("Raycast hit 2");
        //     TrackedMarkerInfo markerInfo = hit.collider.GetComponent<TrackedMarkerInfo>();
        //     if (markerInfo != null)
        //     {
        //         assignedMarker = markerInfo;
        //         Debug.LogError($"🟡 Raycast found marker above cell '{name}': {markerInfo.name}");
        //         OnAssignedMarkerChanged();
        //     }
        // }

        
        TrackedMarkerInfo[] allMarkers = FindObjectsOfType<TrackedMarkerInfo>();
        
        // Debug
        if (allMarkers != null && allMarkers.Length > 0)
        {
            if (!hasLoggedMarkers)
            {
                Debug.LogError($"🔍 Found {allMarkers.Length} markers in scene for cell '{name}'");
                hasLoggedMarkers = true;
            }
            
            if (assignedMarker == null)
            {
                TrackedMarkerInfo found = FindMatchingMarkerInBox(allMarkers);

                if (found != null && found != previousMarker)
                {
                    Debug.LogError("🟢 Found matching marker for cell '" + name + "': " + found.name);
                    assignedMarker = found;
                    OnAssignedMarkerChanged();
                }
            }
            else
            {
                if (!IsHitByRaycast(assignedMarker.transform))
                {
                    assignedMarker = null;
                    OnAssignedMarkerChanged(); 
                }
            }
            
        }
        
        
        
        // DEBUG
        UpdateDebugRaycastCube();
    }



    private TrackedMarkerInfo FindMatchingMarkerInBox(TrackedMarkerInfo[] allMarkers)
    {
        TrackedMarkerInfo closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var marker in allMarkers)
        {
            if (marker == null || marker.gameObject == null) continue;
            if (marker.GetInstanceID() == 0) continue; // 💡 Completely destroyed (defensive check)
            if (marker.markerType != cellType) continue;
            if (!IsHitByRaycast(marker.transform)) continue;

            float dist = Vector3.Distance(transform.position, marker.transform.position);
            if (dist < closestDist)
            {
                closest = marker;
                closestDist = dist;
            }
        }

        return closest;
    }


    private bool IsHitByRaycast(Transform toCheckTransform)
    {
        if (toCheckTransform == null) return false;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastLength, LayerMask.GetMask("Marker")) || Physics.Raycast(transform.position, Vector3.up, out hit, raycastLength, LayerMask.GetMask("Marker")) )
        {
            return hit.transform == toCheckTransform;
        }

        return false;
    }

    private void OnAssignedMarkerChanged()
    {
        if (assignedMarker && assignedMarker.gameObject != null)
        {
            Debug.Log($"🟢 Marker assigned to cell '{name}': {assignedMarker.name}");
            
            // Set Color of spriteRenderer
            Color color = visualImage.color;
            color.a = 0.1f;
            visualImage.color = color;
            
            // Make callback
            OnAssignedMarker?.Invoke(assignedMarker);
            

            // DEBUG
            if (debugRaycast != null)
            {
                debugRaycast.GetComponent<Renderer>().material.color = Color.green;
            }
        }
        else
        {
            Debug.Log($"🔴 Marker removed from cell '{name}'");
            
            // Set Color of spriteRenderer
            Color color = visualImage.color;
            color.a = 1f;
            visualImage.color = color;
            
            // Make callback
            OnRemovedMarker?.Invoke();
            

            // DEBUG
            if (debugRaycast != null)
            {
                debugRaycast.GetComponent<Renderer>().material.color = Color.red;
            }
        }
        
        // Checking for crafting result
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
            Gizmos.DrawSphere(transform.position + Vector3.up * 0.05f, 0.02f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + Vector3.up * 0.05f, 0.02f);
        }

        // 🔵 Visualize Raycast Down
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * raycastLength);

        // 🔵 Visualize Raycast Up
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * raycastLength);
    }

    
    #region DEBUG
    
    private void UpdateDebugRaycastCube()
    {
        if (debugRaycast == null) return;

        RaycastHit hit;
        Vector3 cubeScale = new Vector3(debugCubeScaleXZ, raycastLength, debugCubeScaleXZ);

        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastLength, LayerMask.GetMask("Marker")))
        {
            debugRaycast.transform.position = transform.position + Vector3.down * raycastLength * 0.5f;
            debugRaycast.transform.localScale = cubeScale;
            debugRaycast.GetComponent<Renderer>().material.color = Color.yellow;
        }
        else if (Physics.Raycast(transform.position, Vector3.up, out hit, raycastLength, LayerMask.GetMask("Marker")))
        {
            debugRaycast.transform.position = transform.position + Vector3.up * raycastLength * 0.5f;
            debugRaycast.transform.localScale = cubeScale;
            debugRaycast.GetComponent<Renderer>().material.color = Color.cyan;
        }
        else
        {
            debugRaycast.transform.position = transform.position;
            debugRaycast.transform.localScale = cubeScale;
            debugRaycast.GetComponent<Renderer>().material.color = Color.gray;
        }
    }

    
    #endregion
    
}
