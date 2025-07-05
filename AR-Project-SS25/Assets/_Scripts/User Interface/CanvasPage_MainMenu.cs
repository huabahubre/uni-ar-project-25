using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CanvasPage_MainMenu : CanvasPage
{
    [BoxGroup("References"), Header("Buttons")]
    public Button Button_Play;
    
    [BoxGroup("References")]
    public Button Button_Exit;
    
    [BoxGroup("References")]
    public Button Button_Settings;
    
    
    public override void Initialize()
    {
        Button_Play.onClick.AddListener(OnPlayButtonClick);
        Button_Exit.onClick.AddListener(OnExitButtonClick);
        Button_Settings.onClick.AddListener(OnSettingsButtonClick);
        
        base.Initialize();
    }


    void OnPlayButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Lobby");
    }
    
    void OnSettingsButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Settings");
    }

    void OnExitButtonClick()
    {
        Application.Quit();
    }
    
}
