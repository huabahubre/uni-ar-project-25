using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SpellData", menuName = "Scriptable Objects/SpellData")]
public class SpellData : ScriptableObject
{
    [BoxGroup("Display Info")]
    public string Name;
    
    
    
    [BoxGroup("General")]
    public SpellEffect Effect;
    
    [BoxGroup("General")]
    public SpellType Recipe;

    
    
    [BoxGroup("References")] public List<SpellVisualData> PrefabTupleList;


    
    #region Get
    
    public SpellVisualData GetPrefabTuple(ElementType type)
    {
        foreach (var tuple in PrefabTupleList)
        {
            if (tuple.Element == type)
            {
                return tuple;
            }
        }

        Debug.LogError($"No prefab found for element type: {type}");
        return null;
    }
    
    #endregion
    
}

[Serializable]
public class SpellVisualData
{
    public ElementType Element;
    public GameObject VisualPrefab;
    public Sprite Icon;
}


public enum ElementType
{
    Fire,
    Water,
    Earth,
    Air,
    None
}

public enum SpellEffect
{
    None,
    Damage,
    Heal,
    Shield,
    Buff,
    Debuff
}

public enum SpellType
{
    None,
    Shield,
    Spear,
    GroundPound,
    SingleShot,
    WideShot
}
