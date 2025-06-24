using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CanvasPage_Gameplay : CanvasPage
{
    
    [BoxGroup("References"), Header("Buttons")]
    public Button Button_Pause;
    
    [BoxGroup("References")]
    public Button Button_GameOver;
    
    
    public override void Initialize()
    {
        Button_Pause.onClick.AddListener(OnPauseButtonClick);
        Button_GameOver.onClick.AddListener(OnGameOverButtonClick);
        
        base.Initialize();
    }


    void OnPauseButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Pause");
    }
    
    void OnGameOverButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("GameOver");
    }

}
