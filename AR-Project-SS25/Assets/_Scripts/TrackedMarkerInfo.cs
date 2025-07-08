using System;
using Sirenix.OdinInspector;
using UnityEngine;

public enum MarkerType
{
    Action,
    Element
}

public class TrackedMarkerInfo : MonoBehaviour
{
    [Header("Marker Settings")]
    public MarkerType markerType;

    [ShowIf("markerType", MarkerType.Element)]
    public ElementType elementType = ElementType.None;

    [Tooltip("Optional identifier to link to crafting recipes (e.g. QR code ID or name)")]
    public string markerId;


    // private void OnEnable()
    // {
    //     GridManagement.Instance.RegisterMarker(this);
    // }
    //
    // private void OnDisable()
    // {
    //     GridManagement.Instance.UnregisterMarker(this);
    // }

}