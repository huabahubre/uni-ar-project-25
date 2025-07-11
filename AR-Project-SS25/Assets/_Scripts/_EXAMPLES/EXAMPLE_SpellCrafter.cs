using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class EXAMPLE_SpellCrafter : MonoBehaviour
{

    public void ExampleCraftRecipe(int spellIdx)
    {
        if (PlayfieldManagement.Instance.currentElementMarker == null)
        {
            Debug.Log("CAN'T CRAFT RECIPE: No Element Marker assigned");
            return;
        }
        
        ElementType elementType = PlayfieldManagement.Instance.currentElementMarker.elementType;
        SpellType spellType = (SpellType)spellIdx;
        
        PlayfieldManagement.Instance.onValidCraftingRecipeFound?.Invoke(new Tuple<SpellType?, ElementType?>(spellType, elementType));
    }


    public void InvalidRecipe()
    {
        PlayfieldManagement.Instance.onRecipeInvalid?.Invoke();
    }
}
