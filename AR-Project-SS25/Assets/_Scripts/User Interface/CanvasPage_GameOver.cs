using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CanvasPage_GameOver : CanvasPage
{
    [BoxGroup("References"), Header("Buttons")]
    public Button Button_Exit;
    
    [BoxGroup("References")]
    public Button Button_Menu;
    
    public override void Initialize()
    {
        Button_Exit.onClick.AddListener(OnExitButtonClick);
        Button_Menu.onClick.AddListener(OnMenuButtonClick);
        
        base.Initialize();
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
