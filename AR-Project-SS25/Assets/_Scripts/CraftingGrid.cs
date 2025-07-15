using System;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class CraftingGrid : Singleton<CraftingGrid>
{
    [Header("Action Cell Rows (Top to Bottom)")]
    public List<CraftingGridCell> topRow = new List<CraftingGridCell>(3);
    public List<CraftingGridCell> middleRow = new List<CraftingGridCell>(3);
    public List<CraftingGridCell> bottomRow = new List<CraftingGridCell>(3);

    [BoxGroup("Settings")] public Vector3 playerCraftingGridOffset = new Vector3(0, 0, 0);

    public GameObject scalerChild;


    private void Start()
    {
        scalerChild.SetActive(false);
        
        Debug.Log("Grid Started");
        
    }

    private void Update()
    {
        // if (currentMarkers == null || currentMarkers.Length == 0)
        //     return;

        // foreach (var cell in GetAllCells())
        // {
        //     cell.CheckForMarkerNearby(currentMarkers);
        // }
    }

    
    #region Update position and rotation / visual
    
    public void UpdateGridPosition(Vector3 position, Quaternion rotation)
    {
        if (scalerChild != null)
        {
            scalerChild.transform.localPosition = position + rotation * playerCraftingGridOffset;
            scalerChild.transform.rotation = rotation;
        }
    }

    public void ShowVisual()
    {
        if (scalerChild != null && GameStateManager.Instance.IsMyTurn())
        {
            scalerChild.SetActive(true);
        }
        else
        {
            scalerChild.SetActive(false);
        }
    }
    
    public void HideVisual()
    {
        if (scalerChild != null)
        {
            scalerChild.SetActive(false);
        }
    }
    
    #endregion
    
    
    public bool[] GetCurrentActionGridState()
    {
        List<CraftingGridCell> ordered = new List<CraftingGridCell>();
        ordered.AddRange(topRow);
        ordered.AddRange(middleRow);
        ordered.AddRange(bottomRow);

        bool[] state = new bool[9];
        for (int i = 0; i < 9 && i < ordered.Count; i++)
        {
            state[i] = ordered[i].assignedMarker != null;
        }
        return state;
    }

    public CraftingRecipe GetValidRecipe()
    {
        bool[] current = GetCurrentActionGridState();

        foreach (var recipe in DataManagement.Instance.craftingRecipes)
        {
            bool[] expected = recipe.GetFlattenedGrid();
            if (expected.Length != 9 || current.Length != 9)
                continue;

            bool match = true;
            for (int i = 0; i < 9; i++)
            {
                if (expected[i] != current[i])
                {
                    match = false;
                    break;
                }
            }

            if (match)
                return recipe;
        }

        return null;
    }
}
