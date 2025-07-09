using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class GridManagement : Singleton<GridManagement>
{
    public Transform playfieldTrackedMarker;
    public List<TrackedMarkerInfo> trackedMarkers;


    [BoxGroup("References")] public Camera mainCamera;
    
    
    [BoxGroup("References")] public HealthVisualPrefab playerHealthVisual;
    [BoxGroup("References")] public HealthVisualPrefab enemyHealthVisual;

    
    [BoxGroup("Settings")] public Vector3 playerVisualOffset = new Vector3(0, 0, 0);
    [BoxGroup("Settings")] public Vector3 gridOffset = new Vector3(0, 0, 0);


    
    public Action<Tuple<SpellType?, ElementType?>> onValidCraftingRecipeFound;
    public Action onRecipeInvalid;
    
    
    
    private void Start()
    {
        // SpawnLocalPlayer();
        RefreshTrackedMarkersFromScene();
    }


    #region Spawn Grid
    
    [Button]
    public void SpawnLocalPlayer(Transform trackedMarkerObject = null)
    {
        // TODO: ONLY FOR DEBUGGING
        if(trackedMarkerObject == null)
        {
            trackedMarkerObject = this.playfieldTrackedMarker;
        }
        // if (trackedMarkerObject == null)
        // {
        //     Debug.LogError("Tracked Marker Object is not assigned.");
        //     return;
        // }

        Vector3 markerPosition = trackedMarkerObject.position;
        Quaternion markerRotation = trackedMarkerObject.rotation;

        // Instantiate Player Visual
        Vector3 playerPos = markerPosition + markerRotation * playerVisualOffset;
        playerHealthVisual = Instantiate(DataManagement.Instance.healthVisualPrefab, playerPos, markerRotation);
        playerHealthVisual.name = "PlayerHealthVisual";
        playerHealthVisual.Init(true, ElementType.Fire);

        // Instantiate Enemy Visual (opposite position)
        Vector3 enemyOffset = -playerVisualOffset;
        Vector3 enemyPos = markerPosition + markerRotation * enemyOffset;
        enemyHealthVisual = Instantiate(DataManagement.Instance.healthVisualPrefab, enemyPos, markerRotation);
        enemyHealthVisual.name = "EnemyHealthVisual";
        enemyHealthVisual.Init(false, ElementType.Water);
        

        // Instantiate Player Grid
        Vector3 gridPos = markerPosition + markerRotation * gridOffset;
        CraftingGrid playerGrid = Instantiate(DataManagement.Instance.craftingGridPrefab, gridPos, markerRotation);
        playerGrid.gameObject.name = "PlayerCraftingGrid";
        
        // TODO: ONLY FOR DEBUGGING
        playerGrid.currentMarkers = trackedMarkers.ToArray();
    }



    #endregion
    
    #region Check for Recipe
    
      
    [Button]
    public void CheckCraftingResult()
    {
        var elementCell = CraftingGrid.Instance.GetElementCell();
        var elementMarker = elementCell?.assignedMarker;

        string elementText = elementMarker != null
            ? elementMarker.elementType.ToString()
            : "❌ No element marker set!";

        // Debug.Log($"Element Marker: <b>{elementText}</b>");
        
        if (elementMarker == null || elementMarker.markerType != MarkerType.Element)
        {
            Debug.Log("❌ No valid element marker placed. Crafting requires an element card.");
            onRecipeInvalid?.Invoke();
            return;
        }

        bool[] actionGrid = CraftingGrid.Instance.GetCurrentActionGridState();

        string gridVisual =
            $"{BoolToX(actionGrid[0])} {BoolToX(actionGrid[1])} {BoolToX(actionGrid[2])}\n" +
            $"{BoolToX(actionGrid[3])} {BoolToX(actionGrid[4])} {BoolToX(actionGrid[5])}\n" +
            $"{BoolToX(actionGrid[6])} {BoolToX(actionGrid[7])} {BoolToX(actionGrid[8])}";

        Debug.Log($"Action Grid State:\n{gridVisual}");

        var recipe = CraftingGrid.Instance.GetValidRecipe();

        if (recipe != null 
            && elementMarker != null 
            && elementMarker.markerType == MarkerType.Element)
        {
            Debug.Log($"✅ Valid recipe found!\n" +
                      $"Spell Type: <b>{recipe.spellType}</b>\n" +
                      $"Element Used: <b>{elementText}</b>");
                      
            onValidCraftingRecipeFound?.Invoke(new Tuple<SpellType?, ElementType?>(recipe.spellType, elementMarker.elementType));
        }
        else
        {
            Debug.Log("❌ No matching recipe for current grid layout.");
            onRecipeInvalid?.Invoke();
        }
    }

    private string BoolToX(bool b) => b ? "1" : "0";




    
    #endregion
    
    
    /// <summary>
    /// Fetches all active TrackedMarkerInfo components in the scene and populates the trackedMarkers list.
    /// </summary>
    [Button("Refresh All Markers")]
    public void RefreshTrackedMarkersFromScene()
    {
        trackedMarkers = new List<TrackedMarkerInfo>(FindObjectsOfType<TrackedMarkerInfo>(includeInactive: false));
        Debug.Log($"🔄 Refreshed tracked markers list. Found {trackedMarkers.Count} active markers.");
    }
    
    /// <summary>
    /// Adds a TrackedMarkerInfo to the list if not already present.
    /// </summary>
    public void RegisterMarker(TrackedMarkerInfo marker)
    {
        if (marker == null) return;

        if (!trackedMarkers.Contains(marker))
        {
            trackedMarkers.Add(marker);
            Debug.Log($"🟢 Registered marker: {marker.name}");
        }
    }

    /// <summary>
    /// Removes a TrackedMarkerInfo from the list if it exists.
    /// </summary>
    public void UnregisterMarker(TrackedMarkerInfo marker)
    {
        if (marker == null) return;

        if (trackedMarkers.Contains(marker))
        {
            trackedMarkers.Remove(marker);
            Debug.Log($"🔴 Unregistered marker: {marker.name}");
        }
    }
}
