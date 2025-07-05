using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasPage_Gameplay : CanvasPage
{
    
    [BoxGroup("References"), Header("Buttons")]
    public Button Button_Pause;
    
    [BoxGroup("References")]
    public Button Button_Cast;

    
    [BoxGroup("References"), Header("Panels")]
    public GameObject Panel_YourTurn;

    [BoxGroup("References")]
    public GameObject Panel_OpponentTurn;
    
    
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
        Button_Cast.onClick.AddListener(OnCastSpell);
        
        base.Initialize();
    }


    public override void OnShow()
    {
        Panel_YourTurn.SetActive(true);
        Panel_OpponentTurn.SetActive(false);
        
        base.OnShow();
    }


    void OnPauseButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Pause");
    }
    


    void OnCastSpell()
    {
        Panel_YourTurn.SetActive(false);
        Panel_OpponentTurn.SetActive(true);

        StartCoroutine(WaitOpponentTurn());
    }
    
    
    
    
    
    
    //TODO: Remove this when functionality is ready
    public void OnManualWin()
    {
        DataManagement.Instance.isWin = true;
        MainCanvasManagement.Instance.ShowPage("GameOver");
    }

    IEnumerator WaitOpponentTurn()
    {
        yield return new WaitForSeconds(3f);
        
        Panel_YourTurn.SetActive(true);
        Panel_OpponentTurn.SetActive(false);
    }
    
    
    
    public void UpdateLocalPlayerInfo(int health)
    {
        Slider_PlayerHealth.value = health;
    }

    
    public void UpdateLocalPlayerInfo(string playerName, int health, int energy, Sprite icon)
    {
        Text_PlayerName.text = playerName;
        Slider_PlayerHealth.value = health;
        Slider_PlayerEnergy.value = energy;
        Image_PlayerIcon.sprite = icon;
    }

    public void UpdateRemotePlayerInfo(string playerName, int health, Sprite icon)
    {
        Text_EnemyName.text = playerName;
        Slider_EnemyHealth.value = health;
        Image_EnemyIcon.sprite = icon;
    }

}
