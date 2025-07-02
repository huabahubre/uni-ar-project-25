using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasPage_Gameplay : CanvasPage
{
    
    [BoxGroup("References"), Header("Buttons")]
    public Button Button_Pause;
    
    [BoxGroup("References"), Header("Player Info")]
    public TextMeshProUGUI Text_PlayerName;
    
    [BoxGroup("References")]
    public Slider Slider_PlayerHealth;
    
    [BoxGroup("References")]
    public Slider Slider_PlayerEnergy;

    [BoxGroup("References")]
    public Image Image_PlayerIcon;
    
    
    
    [BoxGroup("References"), Header("Enemy Info")]
    public TextMeshProUGUI Text_EnemyName;
    
    [BoxGroup("References")]
    public Slider Slider_EnemyHealth;

    [BoxGroup("References")]
    public Image Image_EnemyIcon;


    
    
    public override void Initialize()
    {
        Button_Pause.onClick.AddListener(OnPauseButtonClick);
        
        base.Initialize();
    }
    
    

    void OnPauseButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Pause");
    }
    
    
    //TODO: Remove this when functionality is ready
    public void OnManualWin()
    {
        DataManagement.Instance.isWin = true;
        MainCanvasManagement.Instance.ShowPage("GameOver");
    }
    
}
