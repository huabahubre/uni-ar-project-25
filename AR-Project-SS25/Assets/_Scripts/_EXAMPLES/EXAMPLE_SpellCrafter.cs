using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class EXAMPLE_SpellCrafter : MonoBehaviour
{

    public TrackedMarkerInfo markerInfo;

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.F6))
        {
            ExampleCraftRecipe(0);
        }
        
        if (Input.GetKeyDown(KeyCode.F7))
        {
            ExampleCraftRecipe(2);
        }
        
        if (Input.GetKeyDown(KeyCode.F8))
        {
            ExampleCraftRecipe(3);
        }
    }

    [Button]
    public void ExampleCraftRecipe(int spellIdx)
    {
        PlayfieldManagement.Instance.OnPlacedElementCard(markerInfo);
        
        ElementType elementType = PlayfieldManagement.Instance.currentElementMarker.elementType;
        SpellType spellType = (SpellType)spellIdx;
        
        PlayfieldManagement.Instance.onValidCraftingRecipeFound?.Invoke(new Tuple<SpellType?, ElementType?>(spellType, elementType));
    }


    public void InvalidRecipe()
    {
        PlayfieldManagement.Instance.onRecipeInvalid?.Invoke();
    }
}
