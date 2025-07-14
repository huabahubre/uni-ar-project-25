using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;

public class GridPlacer : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;
    
    [SerializeField, BoxGroup("Marker Prefabs")] private GameObject action_marker_prefab;
    [SerializeField, BoxGroup("Marker Prefabs")] private GameObject element_marker_prefab;
    
    private Dictionary<string, GameObject> spawnedMarkers = new Dictionary<string, GameObject>();
    
    // constant marker names
    private const string air = "ar_marker_air";
    private const string water = "ar_marker_water";
    private const string earth = "ar_marker_earth";
    private const string fire = "ar_marker_fire";
    
    private string playfield = "ar_marker_playfield";
    private string action = "ar_marker_action";
    
    

    // DEBUG
    [SerializeField, BoxGroup("DEBUG")]
    public TextMeshProUGUI debugText;
    private readonly Dictionary<string, string> markerDebugStatus = new();

    #region Debug

    private void SetMarkerDebug(string markerName, string status)
    {
        markerDebugStatus[markerName] = $"[{System.DateTime.Now:HH:mm:ss}] {markerName}: {status}";
        debugText.text = string.Join("\n", markerDebugStatus.Values);
    }



    #endregion
    
    
    
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
    

    
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // ADD MARKER PREFABS
        foreach (var trackedImage in args.added)
        {
            if (trackedImage.referenceImage.name == playfield)
            {
                PlayfieldManagement.Instance.OnPlayfieldTracked();
                
                // Debug text
                SetMarkerDebug(trackedImage.referenceImage.name, "detected");
            }
            else if (trackedImage.referenceImage.name == action)
            {
                GameObject cardInstance = Instantiate(action_marker_prefab, trackedImage.transform.position, trackedImage.transform.rotation);
                cardInstance.GetComponent<TrackedMarkerInfo>().markerType = MarkerType.Action;
                spawnedMarkers[trackedImage.trackableId.ToString()] = cardInstance;
                cardInstance.transform.SetParent(trackedImage.transform);
                
                // Debug text
                SetMarkerDebug(trackedImage.referenceImage.name, "detected");
            }
            else if (trackedImage.referenceImage.name is air or water or earth or fire)
            {
                debugText.text = "Element detected: " + trackedImage.referenceImage.name;
                GameObject cardInstance = Instantiate(element_marker_prefab, trackedImage.transform.position, trackedImage.transform.rotation);
                cardInstance.GetComponent<TrackedMarkerInfo>().markerType = MarkerType.Element;
                switch (trackedImage.referenceImage.name)
                {
                    case air:
                        cardInstance.GetComponent<TrackedMarkerInfo>().elementType = ElementType.Air;
                        break;
                    case water:
                        cardInstance.GetComponent<TrackedMarkerInfo>().elementType = ElementType.Water;
                        break;
                    case earth:
                        cardInstance.GetComponent<TrackedMarkerInfo>().elementType = ElementType.Earth;
                        break;
                    case fire:
                        cardInstance.GetComponent<TrackedMarkerInfo>().elementType = ElementType.Fire;
                        break;
                    default:
                        cardInstance.GetComponent<TrackedMarkerInfo>().elementType = ElementType.Fire;
                        break;
                }
                spawnedMarkers[trackedImage.trackableId.ToString()] = cardInstance;
                cardInstance.transform.SetParent(trackedImage.transform);
                
                // Debug text
                SetMarkerDebug("Unknown marker", "detected");
            }
            else
            {
                // Debug text
                SetMarkerDebug("Unknown marker", "detected");
            }
        }

        // UPDATE MARKER PREFABS
        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                if (trackedImage.referenceImage.name == playfield)
                {
                    PlayfieldManagement.Instance.UpdatePlayfieldPosition(trackedImage.transform.position, trackedImage.transform.rotation);
                }
                else if (trackedImage.referenceImage.name == action || trackedImage.referenceImage.name is air or water or earth or fire)
                {
                    spawnedMarkers[trackedImage.trackableId.ToString()].transform.position = trackedImage.transform.position;
                }
                    
                // Debug text
                SetMarkerDebug(trackedImage.referenceImage.name, "tracking");
            }
            else if (trackedImage.trackingState == TrackingState.None)
            {
                if (trackedImage.referenceImage.name == playfield)
                {
                    PlayfieldManagement.Instance.OnLostPlayfieldTracking();
                }
                    
                // Debug text
                SetMarkerDebug(trackedImage.referenceImage.name, "not in sight");
            }
        }

        // REMOVE MARKER PREFABS
        foreach (var trackedImage in args.removed)
        {
            if (trackedImage.referenceImage.name == playfield)
            {
                PlayfieldManagement.Instance.OnLostPlayfieldTracking();
            }
            else if (trackedImage.referenceImage.name == action)
            {
                if (spawnedMarkers.ContainsKey(trackedImage.trackableId.ToString()))
                {
                    Destroy(spawnedMarkers[trackedImage.trackableId.ToString()]);
                    spawnedMarkers.Remove(trackedImage.trackableId.ToString());
                }
            }
            
            // Debug text
            SetMarkerDebug(trackedImage.referenceImage.name, "lost");
        }
    }
}
