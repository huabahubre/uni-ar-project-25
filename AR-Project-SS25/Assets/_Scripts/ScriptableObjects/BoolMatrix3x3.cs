using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class BoolMatrix3x3
{
    [TableMatrix(SquareCells = true, DrawElementMethod = nameof(DrawBoolCell), ResizableColumns = false)]
    public bool[,] data = new bool[3, 3];

    private static bool DrawBoolCell(Rect rect, bool value)
    {
        return GUI.Toggle(rect, value, GUIContent.none);
    }

    public bool[] ToFlatArray()
    {
        bool[] flat = new bool[9];
        for (int row = 0; row < 3; row++)
        for (int col = 0; col < 3; col++)
            flat[row * 3 + col] = data[row, col];
        return flat;
    }
}