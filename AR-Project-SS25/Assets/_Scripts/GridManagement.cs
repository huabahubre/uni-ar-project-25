using Sirenix.OdinInspector;
using UnityEngine;

public class GridManagement : Singleton<GridManagement>
{
    
    [BoxGroup("ONLY FOR DEBUGGING!")]
    public Transform trackedMarkerObject;



    [BoxGroup("References")] public Camera mainCamera;
    
    
    
    [BoxGroup("Settings")] public Vector3 playerVisualOffset = new Vector3(0, 0, 0);
    [BoxGroup("Settings")] public Vector3 gridOffset = new Vector3(0, 0, 0);
    
    
    
    
    
    #region Spawn Grid
    
    [Button]
    public void SpawnLocalPlayer(Transform trackedMarkerObject = null)
    {
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
        
    }



    #endregion
    
    
    
    
}
