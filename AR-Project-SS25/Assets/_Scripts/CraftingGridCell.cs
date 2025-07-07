using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class CraftingGridCell : MonoBehaviour
{
    public MarkerType cellType;

    [Header("Detection Zone (Cube, scales with transform)")]
    public Vector3 baseDetectionBoxSize = new Vector3(0.05f, 0.05f, 0.05f);

    [ShowInInspector, ReadOnly] public TrackedMarkerInfo assignedMarker;

    public Toggle worldToggle;

    private bool isWorldToggleOn = false;

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

        TrackedMarkerInfo[] allMarkers = FindObjectsOfType<TrackedMarkerInfo>();

        if (assignedMarker == null)
        {
            TrackedMarkerInfo found = FindMatchingMarkerInBox(allMarkers);
            if (found != null)
            {
                assignedMarker = found;
                OnAssignedMarkerChanged();
            }
        }
        else
        {
            // Only check if current marker is still inside
            if (!IsInsideBox(assignedMarker.transform.position))
            {
                assignedMarker = null;
                OnAssignedMarkerChanged();
            }
        }
    }

    private TrackedMarkerInfo FindMatchingMarkerInBox(TrackedMarkerInfo[] allMarkers)
    {
        TrackedMarkerInfo closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var marker in allMarkers)
        {
            if (marker.markerType != cellType) continue;
            if (!IsInsideBox(marker.transform.position)) continue;

            float dist = Vector3.Distance(transform.position, marker.transform.position);
            if (dist < closestDist)
            {
                closest = marker;
                closestDist = dist;
            }
        }

        return closest;
    }

    private bool IsInsideBox(Vector3 worldPoint)
    {
        Vector3 halfSize = Vector3.Scale(baseDetectionBoxSize, transform.lossyScale) * 0.5f;
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        return Mathf.Abs(localPoint.x) <= halfSize.x &&
               Mathf.Abs(localPoint.y) <= halfSize.y &&
               Mathf.Abs(localPoint.z) <= halfSize.z;
    }

    private void OnAssignedMarkerChanged()
    {
        if (assignedMarker != null)
        {
            Debug.Log($"🟢 Marker assigned to cell '{name}': {assignedMarker.name}");
        }
        else
        {
            Debug.Log($"🔴 Marker removed from cell '{name}'");
        }
        
        
            
        // Checking for crafting result
        GridManagement.Instance.CheckCraftingResult();
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
            OnAssignedMarkerChanged();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = cellType == MarkerType.Element ? Color.magenta : Color.cyan;
        Vector3 scaledSize = Vector3.Scale(baseDetectionBoxSize, transform.lossyScale);
        Gizmos.DrawWireCube(transform.position, scaledSize);

        if (assignedMarker != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + Vector3.up * 0.05f, 0.02f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.05f, 0.02f);
        }
    }
}
