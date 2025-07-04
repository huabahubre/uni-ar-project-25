using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class GridManagement : Singleton<GridManagement>
{
    
    [BoxGroup("ONLY FOR DEBUGGING!")]
    public Transform trackedMarkerObject;

    [BoxGroup("ONLY FOR DEBUGGING!")] public List<TrackedMarkerInfo> trackedMarkers;


    [BoxGroup("References")] public Camera mainCamera;
    
    
    
    [BoxGroup("Settings")] public Vector3 playerVisualOffset = new Vector3(0, 0, 0);
    [BoxGroup("Settings")] public Vector3 gridOffset = new Vector3(0, 0, 0);


    private void Start()
    {
        // SpawnLocalPlayer();
    }


    #region Spawn Grid
    
    [Button]
    public void SpawnLocalPlayer(Transform trackedMarkerObject = null)
    {
        // TODO: ONLY FOR DEBUGGING
        if(trackedMarkerObject == null)
        {
            trackedMarkerObject = this.trackedMarkerObject;
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
        HealthVisualPrefab playerVisual = Instantiate(DataManagement.Instance.healthVisualPrefab, playerPos, markerRotation);
        playerVisual.name = "PlayerHealthVisual";
        playerVisual.Init(true, ElementType.Fire);

        // Instantiate Enemy Visual (opposite position)
        Vector3 enemyOffset = -playerVisualOffset;
        Vector3 enemyPos = markerPosition + markerRotation * enemyOffset;
        HealthVisualPrefab enemyVisual = Instantiate(DataManagement.Instance.healthVisualPrefab, enemyPos, markerRotation);
        enemyVisual.name = "EnemyHealthVisual";
        enemyVisual.Init(false, ElementType.Water);
        

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

        Debug.Log($"Element Marker: <b>{elementText}</b>");

        
        if (elementMarker == null || elementMarker.markerType != MarkerType.Element)
        {
            Debug.Log("❌ No valid element marker placed. Crafting requires an element card.");
            return;
        }

        bool[] actionGrid = CraftingGrid.Instance.GetCurrentActionGridState();

        string gridVisual =
            $"{BoolToX(actionGrid[0])} {BoolToX(actionGrid[1])} {BoolToX(actionGrid[2])}\n" +
            $"{BoolToX(actionGrid[3])} {BoolToX(actionGrid[4])} {BoolToX(actionGrid[5])}\n" +
            $"{BoolToX(actionGrid[6])} {BoolToX(actionGrid[7])} {BoolToX(actionGrid[8])}";

        Debug.Log($"Action Grid State:\n{gridVisual}");

        var recipe = CraftingGrid.Instance.GetValidRecipe();

        if (recipe != null)
        {
            Debug.Log($"✅ Valid recipe found!\n" +
                      $"Spell Type: <b>{recipe.spellType}</b>\n" +
                      $"Element Used: <b>{elementText}</b>");
        }
        else
        {
            Debug.Log("❌ No matching recipe for current grid layout.");
        }
    }

    private string BoolToX(bool b) => b ? "1" : "0";




    
    #endregion
    
    
}
