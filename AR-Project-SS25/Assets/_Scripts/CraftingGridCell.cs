using UnityEngine;

public class CraftingGridCell : MonoBehaviour
{
    public MarkerType cellType;

    [Header("Detection Zone (Cube)")]
    public Vector3 detectionBoxSize = new Vector3(0.05f, 0.05f, 0.05f);

    [HideInInspector] public TrackedMarkerInfo assignedMarker;

    private TrackedMarkerInfo previousMarker;

    private void OnDrawGizmos()
    {
        Gizmos.color = cellType == MarkerType.Element ? Color.magenta : Color.cyan;
        Gizmos.DrawWireCube(transform.position, detectionBoxSize);

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

    public void CheckForMarkerNearby(TrackedMarkerInfo[] trackedMarkers)
    {
        TrackedMarkerInfo closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var marker in trackedMarkers)
        {
            if (!IsInsideBox(marker.transform.position))
                continue;

            if (marker.markerType != cellType)
            {
                // Optional debug log for mismatches
                // Debug.Log($"❌ Marker {marker.name} of type {marker.markerType} is not valid for cell {name} ({cellType})");
                continue;
            }

            float dist = Vector3.Distance(marker.transform.position, transform.position);
            if (dist < closestDist)
            {
                closest = marker;
                closestDist = dist;
            }
        }

        if (closest != assignedMarker)
        {
            // if (assignedMarker != null)
            //     Debug.Log($"🟥 {cellType} Cell '{name}' lost marker: {assignedMarker.name}");
            //
            // if (closest != null)
            //     Debug.Log($"🟩 {cellType} Cell '{name}' assigned marker: {closest.name}");

            assignedMarker = closest;
        }

        previousMarker = assignedMarker;
    }

    private bool IsInsideBox(Vector3 point)
    {
        Vector3 halfSize = detectionBoxSize * 0.5f;
        Vector3 localPoint = transform.InverseTransformPoint(point);
        return Mathf.Abs(localPoint.x) <= halfSize.x &&
               Mathf.Abs(localPoint.y) <= halfSize.y &&
               Mathf.Abs(localPoint.z) <= halfSize.z;
    }
}
