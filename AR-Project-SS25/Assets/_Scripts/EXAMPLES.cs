using Sirenix.OdinInspector;
using UnityEngine;

public class EXAMPLES : MonoBehaviour
{
    // Here are examples on how to use the "MainCanvasManagement" and the "DataManagement" classes.
    
    #region MainCanvasManagement

    /// <summary>
    ///  This method will show the "Menu" page in the MainCanvasManagement class.
    /// </summary>
    [Button]
    public void ShowMenuPage()
    {
        if (MainCanvasManagement.Instance != null)
        {
            MainCanvasManagement.Instance.ShowPage("Menu");
            Debug.Log("[EXAMPLES] Menu page is now visible.");
        }
        else
        {
            Debug.LogError("[EXAMPLES] MainCanvasManagement instance is not found.");
        }
    }
    
    /// <summary>
    ///  This method will show the "Settings" page in the MainCanvasManagement class.
    /// </summary>
    [Button]
    public void HideAllPages()
    {
        if (MainCanvasManagement.Instance != null)
        {
            MainCanvasManagement.Instance.HideAllPages();
            Debug.Log("[EXAMPLES] Current page is now hidden.");
        }
        else
        {
            Debug.LogError("[EXAMPLES] MainCanvasManagement instance is not found.");
        }
    }
    
    #endregion
    
    #region DataManagement

    /// <summary>
    ///  This method checks if the player name is set in the DataManagement class and will display it in the console.
    /// </summary>
    [Button]
    public void CheckPlayerName()
    {
        if (DataManagement.Instance.playerData.playerName == "")
        {
            Debug.Log("[EXAMPLES] Player name is not set. Please set it in the DataManagement class.");
        }
        else
        {
            Debug.Log("[EXAMPLES] Player name is: " + DataManagement.Instance.playerData.playerName);
        }
    }

    #endregion
    
}
