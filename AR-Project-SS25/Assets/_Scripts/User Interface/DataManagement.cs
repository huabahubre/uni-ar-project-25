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
    public Material material;
}

#endregion


