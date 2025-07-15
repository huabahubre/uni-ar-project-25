using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayfieldManagement : Singleton<PlayfieldManagement>
{
    [BoxGroup("References")] public GameObject playFieldVisual;
    [BoxGroup("References")] public CraftingGridCell playerElementCell;
    [BoxGroup("References")] public HealthVisualPrefab playerHealthVisual;
    [BoxGroup("References")] public HealthVisualPrefab enemyHealthVisual;
    
    [BoxGroup("Settings")] public Vector3 playerVisualOffset = new Vector3(0, 0, 0);
    [BoxGroup("Settings")] public Vector3 playerElementCellOffset = new Vector3(0, 0, 0);
    [BoxGroup("Settings")] public Vector3 gridOffset = new Vector3(0, 0, 0);

    
    

    
    public Action<Tuple<SpellType?, ElementType?>> onValidCraftingRecipeFound;
    public Action onRecipeInvalid;

    
    [SerializeField, ReadOnly]
    public TrackedMarkerInfo currentElementMarker;
    
    
    
    
    private bool initedPlayfield = false;
    
    
    private void Start()
    {
        // SpawnLocalPlayer();
        // RefreshTrackedMarkersFromScene();
        
        playFieldVisual.SetActive(false);
    }

    private void Update()
    {
        // Update Grid position, when active element marker is set
        if (currentElementMarker != null)
        {
            CraftingGrid.Instance.UpdateGridPosition(currentElementMarker.transform.position, currentElementMarker.transform.rotation);
        }
    }


    #region Playfield

    [Button]
    public void OnPlayfieldTracked()
    {
        // Subscribe to element cell
        if (playerElementCell != null)
        {
            playerElementCell.OnAssignedMarker += OnPlacedElementCard;
            playerElementCell.OnRemovedMarker += OnRemovedElementCard;
        }

        // If already inited, just set the position
        if (initedPlayfield)
        {
            playFieldVisual.SetActive(true);
            SetPlayfieldPosition();
        }
        else
        {
            MainCanvasManagement.Instance.initPlayfieldButton.SetActive(true);
            // InitPlayfield();
        }
        
        Debug.LogError("OnPlayFieldTracked");
    }

    
    [Button]
    public void OnLostPlayfieldTracking()
    {
        if(!initedPlayfield)
            return;
        
        playFieldVisual.SetActive(false);
        
        // Unsubscribe to element cell
        if (playerElementCell != null)
        {
            playerElementCell.OnAssignedMarker -= OnPlacedElementCard;
            playerElementCell.OnRemovedMarker -= OnRemovedElementCard;
        }
        // Show scan screen
        MainCanvasManagement.Instance.ShowScanScreen("You lost the playfield tracking.\nPlease scan the playfield again to continue!");
        
        Debug.LogError("OnLostPlayfield");
    }
    
    
    
    
    
    
    [Button]
    public void InitPlayfield()
    {
        // this is when we loose track and need to rescan the playfield
        if (!initedPlayfield)
        {
            MainCanvasManagement.Instance.StartLoading("Waiting for other players to scan the playfield...");
        }
        
        // This is only the very first time
        initedPlayfield = true;
        
        playFieldVisual.SetActive(true);
        MainCanvasManagement.Instance.StopScanScreen();

        // Init Health Visuals
        playerHealthVisual.Init(true, (ElementType)PlayerState.LocalPlayer.ElementIndex.Value);
        enemyHealthVisual.Init(false, (ElementType)PlayerState.EnemyPlayer.ElementIndex.Value);

        SetPlayfieldPosition();
        
        // tell Server that we are ready
        GameStateManager.Instance.SetPlayerReadyServerRpc();
        
        Debug.LogError("Inited playfield!");
    }
    
    [Button]
    public void SetPlayfieldPosition()
    {
        bool isPlayerOne = GameStateManager.Instance.IsLocalPlayerPlayerOne();
    
        playFieldVisual.SetActive(true);

        // Keep current local position
        Vector3 localPosition = playFieldVisual.transform.localPosition;

        // Set local rotation based on player identity
        Quaternion localRotation = isPlayerOne 
            ? Quaternion.Euler(0f, 0f, 0f) 
            : Quaternion.Euler(0f, 180f, 0f);

        playFieldVisual.transform.localPosition = localPosition;
        playFieldVisual.transform.localRotation = localRotation;
    }

    
    
    #endregion
    
    #region Element checking

    public void OnPlacedElementCard(TrackedMarkerInfo markerInfo)
    {
        if (markerInfo == null)
        {
            Debug.LogError("❌ Marker info is null. Cannot place element card.");
            return;
        }
        
        Debug.LogError("🟢 Placed element card in cell: " + markerInfo.name);
        
        currentElementMarker = markerInfo;
        CraftingGrid.Instance.ShowVisual();
        
        Debug.Log("🔵 Element card placed: " + markerInfo.elementType);
    }

    void OnRemovedElementCard()
    {
        currentElementMarker = null;
        
        CraftingGrid.Instance.HideVisual();
        
        Debug.Log("🔴 Element card removed from cell.");
        Debug.LogError("🔴 Element card removed from cell.");
    }

    
    
    #endregion
    
    #region Check for Recipe
    
      
    [Button]
    public void CheckCraftingResult()
    {
        var elementCell = playerElementCell;
        var elementMarker = elementCell?.assignedMarker;

        string elementText = elementMarker != null
            ? elementMarker.elementType.ToString()
            : "❌ No element marker set!";

        // Debug.Log($"Element Marker: <b>{elementText}</b>");
        
        if (elementMarker == null || elementMarker.markerType != MarkerType.Element)
        {
            Debug.LogError("❌ No valid element marker placed. Crafting requires an element card.");
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
            Debug.LogError($"✅ Valid recipe found!\n" +
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


    private void OnDisable()
    {
        if (playerElementCell != null)
        {
            playerElementCell.OnAssignedMarker -= OnPlacedElementCard;
            playerElementCell.OnRemovedMarker -= OnRemovedElementCard;
        }
    }


    #region OBSOLETE --> Use maybe later?
    
    // /// <summary>
    // /// Fetches all active TrackedMarkerInfo components in the scene and populates the trackedMarkers list.
    // /// </summary>
    // [Button("Refresh All Markers")]
    // public void RefreshTrackedMarkersFromScene()
    // {
    //     trackedMarkers = new List<TrackedMarkerInfo>(FindObjectsOfType<TrackedMarkerInfo>(includeInactive: false));
    //     Debug.Log($"🔄 Refreshed tracked markers list. Found {trackedMarkers.Count} active markers.");
    // }
    //
    // /// <summary>
    // /// Adds a TrackedMarkerInfo to the list if not already present.
    // /// </summary>
    // public void RegisterMarker(TrackedMarkerInfo marker)
    // {
    //     if (marker == null) return;
    //
    //     if (!trackedMarkers.Contains(marker))
    //     {
    //         trackedMarkers.Add(marker);
    //         Debug.Log($"🟢 Registered marker: {marker.name}");
    //     }
    // }
    //
    // /// <summary>
    // /// Removes a TrackedMarkerInfo from the list if it exists.
    // /// </summary>
    // public void UnregisterMarker(TrackedMarkerInfo marker)
    // {
    //     if (marker == null) return;
    //
    //     if (trackedMarkers.Contains(marker))
    //     {
    //         trackedMarkers.Remove(marker);
    //         Debug.Log($"🔴 Unregistered marker: {marker.name}");
    //     }
    // }
    
    #endregion
}
