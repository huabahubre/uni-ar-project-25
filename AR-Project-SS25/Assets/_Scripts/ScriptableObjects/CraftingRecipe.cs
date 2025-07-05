using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public SpellType spellType;

    [BoxGroup("3x3 Action Grid")]
    [HorizontalGroup("3x3 Action Grid/Row0")]
    public bool r0c0, r0c1, r0c2;

    [HorizontalGroup("3x3 Action Grid/Row1")]
    public bool r1c0, r1c1, r1c2;

    [HorizontalGroup("3x3 Action Grid/Row2")]
    public bool r2c0, r2c1, r2c2;

    public bool[] GetFlattenedGrid()
    {
        return new bool[]
        {
            r0c0, r0c1, r0c2,
            r1c0, r1c1, r1c2,
            r2c0, r2c1, r2c2
        };
    }
}