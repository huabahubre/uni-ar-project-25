using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CanvasPage_Pause : CanvasPage
{
    [BoxGroup("References"), Header("Buttons")]
    public Button Button_Resume;
    
    
    [BoxGroup("References")]
    public Button Button_Settings;
    
    [BoxGroup("References")]
    public Button Button_GiveUp;
    
    
    
    [BoxGroup("References"), Header("Give Up")]
    public GameObject Panel_GiveUp;
    
    
    public override void Initialize()
    {
        Button_Resume.onClick.AddListener(OnResumeButtonClick);
        Button_Settings.onClick.AddListener(OnSettingsButtonClick);
        Button_GiveUp.onClick.AddListener(OnGiveUpButtonClick);
        
        base.Initialize();
    }


    public override void OnShow()
    {
        Panel_GiveUp.SetActive(false);
        
        base.OnShow();
    }


    void OnResumeButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Gameplay");
    }
    
    void OnSettingsButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Settings");
    }

    
    #region Give Up
    
    void OnGiveUpButtonClick()
    {
        Panel_GiveUp.SetActive(true);
    }


    public void ConfirmGiveUp()
    {
        Debug.Log("Player confirmed GiveUp!");
        GameStateManager.Instance.SurrenderServerRpc();
    }
    
    
    #endregion
    
    void OnExitButtonClick()
    {
        Application.Quit();
    }
}
