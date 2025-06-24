using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CanvasPage_Pause : CanvasPage
{
    [BoxGroup("References"), Header("Buttons")]
    public Button Button_Resume;
    
    [BoxGroup("References")]
    public Button Button_Menu;
    
    [BoxGroup("References")]
    public Button Button_Settings;
    
    
    public override void Initialize()
    {
        Button_Resume.onClick.AddListener(OnResumeButtonClick);
        Button_Menu.onClick.AddListener(OnMenuButtonClick);
        Button_Settings.onClick.AddListener(OnSettingsButtonClick);
        
        base.Initialize();
    }


    void OnResumeButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Gameplay");
    }
    
    void OnSettingsButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Settings");
    }

    void OnMenuButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Menu");
    }

    
    void OnExitButtonClick()
    {
        Application.Quit();
    }
}
