using UnityEngine;
using System.Collections.Generic;

public class CraftingGrid : Singleton<CraftingGrid>
{
    [Header("Action Cell Rows (Top to Bottom)")]
    public List<CraftingGridCell> topRow = new List<CraftingGridCell>(3);
    public List<CraftingGridCell> middleRow = new List<CraftingGridCell>(3);
    public List<CraftingGridCell> bottomRow = new List<CraftingGridCell>(3);

    [Header("Element Cell")]
    public CraftingGridCell elementCell;

    [Header("Runtime Markers")]
    public TrackedMarkerInfo[] currentMarkers;

    private void Update()
    {
        if (currentMarkers == null || currentMarkers.Length == 0)
            return;

        // foreach (var cell in GetAllCells())
        // {
        //     cell.CheckForMarkerNearby(currentMarkers);
        // }
    }

    public CraftingGridCell GetElementCell()
    {
        return elementCell;
    }

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

    private IEnumerable<CraftingGridCell> GetAllCells()
    {
        foreach (var cell in topRow) yield return cell;
        foreach (var cell in middleRow) yield return cell;
        foreach (var cell in bottomRow) yield return cell;
        if (elementCell != null) yield return elementCell;
    }
}
