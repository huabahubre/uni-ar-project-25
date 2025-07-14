using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class DataManagement : Singleton<DataManagement>
{
    void Start()
    {
        // Load game as tutorial
        // isTutorialActive = PlayerPrefs.GetInt("IsTutorialActive", 1) == 1;
    }
    
    
    #region Player Data

    [BoxGroup("Player Data")]
    public PlayerData playerData;
    
    #endregion
    
    
    #region Scriptable Objects
    
    [BoxGroup("Scriptable Objects")]
    public List<SpellData> spellDataList;
    
    
    [BoxGroup("Scriptable Objects")]
    public List<CraftingRecipe> craftingRecipes;

    
    
    [BoxGroup("Scriptable Objects")]
    public List<ElementVisualData> ElementVisualDataList;


    public ElementVisualData GetElementVisualData(int elementIdx)
    {
        return GetElementVisualData((ElementType)elementIdx);
    }

    public ElementVisualData GetElementVisualData(ElementType elementType)
    {
        foreach (var data in ElementVisualDataList)
        {
            if (data.Element == elementType)
            {
                return data;
            }
        }

        Debug.LogError($"No visual data found for element type: {elementType}");
        return null;
    }
    
    
    #endregion
    
    
    #region Tutorial
    
    // [FoldoutGroup("Tutorial")]
    // public bool isTutorialActive;
    //
    // public void DisableTutorial()
    // {
    //     isTutorialActive = false;
    //     PlayerPrefs.SetInt("IsTutorialActive", 0);
    //     PlayerPrefs.Save();
    // }
    //
    //
    // public void EnableTutorial()
    // {
    //     isTutorialActive = true;
    //     PlayerPrefs.SetInt("IsTutorialActive", 1);
    //     PlayerPrefs.Save();
    // }
    //
    #endregion

    
    #region Prefabs
    
    [BoxGroup("Prefabs"), Header("Grid")]
    public CraftingGrid craftingGridPrefab;
    
    [BoxGroup("Prefabs")]
    public HealthVisualPrefab healthVisualPrefab;
    
    [BoxGroup("Prefabs")]
    public TrackedMarkerInfo actionCardPrefab;

    
    
    
    #endregion
 
    
    
    
    // This is not good, dont do this in your project
    public bool isRematchLobby = false;
}


#region Player Data

[Serializable]
public class PlayerData
{
    public string playerName;
    public PlayerStyle style;
}

[Serializable]
public class PlayerStyle
{
    public Color color;
    public Sprite icon;
}

#endregion


[Serializable]
public class ElementVisualData
{
    public ElementType Element;
    public GameObject CrystalPrefab;
    public Color Color;
    public Sprite Icon;
}


