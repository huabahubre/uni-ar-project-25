using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CanvasPage_Settings : CanvasPage
{
    
    [BoxGroup("References"), Header("Buttons")]
    public Button Button_Back;
    
    public override void Initialize()
    {
        Button_Back.onClick.AddListener(OnBackButtonClick);
        
        base.Initialize();
    }


    void OnBackButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Menu");
    }
    

}
