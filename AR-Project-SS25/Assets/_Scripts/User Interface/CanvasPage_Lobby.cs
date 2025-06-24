using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CanvasPage_Lobby : CanvasPage
{
    
    [BoxGroup("References"), Header("Buttons")]
    public Button Button_Back;
    
    [BoxGroup("References")]
    public Button Button_Finish;
    
    
    public override void Initialize()
    {
        Button_Back.onClick.AddListener(OnBackButtonClick);
        Button_Finish.onClick.AddListener(OnFinishButtonClick);
        
        base.Initialize();
    }


    void OnBackButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Menu");
    }

    void OnFinishButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Gameplay");
    }

}
