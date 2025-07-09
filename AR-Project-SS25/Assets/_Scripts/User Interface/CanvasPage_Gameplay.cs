using System;
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
    
    [BoxGroup("References")]
    public GameObject Panel_ActiveSpellCasting;

    
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



    private Tuple<SpellType?, ElementType?> currentSpellData = null;
    public Action<Tuple<SpellType?, ElementType?>> onCastSpell;
    
    
    public override void Initialize()
    {
        Button_Pause.onClick.AddListener(OnPauseButtonClick);
        Button_Cast.onClick.AddListener(OnCastSpell);
        
        // Subscribe to events
        GridManagement.Instance.onValidCraftingRecipeFound += OnValidRecipeFound;
        GridManagement.Instance.onRecipeInvalid += OnInvalidRecipe;
        GameStateManager.Instance.onFinishedTurn += OnFinishedTurn;
        
        base.Initialize();
    }


    public override void OnShow()
    {
        bool isPlayerTurn = GameStateManager.Instance.IsCurrentPlayersTurn();
        
        // Set Panels
        Panel_YourTurn.SetActive(isPlayerTurn);
        Panel_OpponentTurn.SetActive(!isPlayerTurn);
        Panel_ActiveSpellCasting.SetActive(false);
        
        base.OnShow();
    }


    void OnPauseButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Pause");
    }
    

    #region Turn funcitonality

    public void OnFinishedTurn()
    {
        bool isPlayerTurn = GameStateManager.Instance.IsCurrentPlayersTurn();
        
        // Update UI
        UpdateLocalPlayerInfo(GameStateManager.Instance.player1HP.Value);
        UpdateRemotePlayerInfo(GameStateManager.Instance.player2HP.Value);
        
        // Set Panels
        Panel_YourTurn.SetActive(isPlayerTurn);
        Panel_OpponentTurn.SetActive(!isPlayerTurn);
        Panel_ActiveSpellCasting.SetActive(false);
    }
    
    #endregion
    
    #region Recipe & Spell casting

    void OnCastSpell()
    {
        Panel_YourTurn.SetActive(false);
        Panel_OpponentTurn.SetActive(true);
        
        // Try to cast spell with Action
        onCastSpell?.Invoke(currentSpellData);
        
        // StartCoroutine(WaitOpponentTurn());
    }
    
    public void OnValidRecipeFound(Tuple<SpellType?, ElementType?> recipe)
    {
        // Handle valid crafting recipe found
        
        Button_Cast.interactable = true;
        currentSpellData = recipe;
    }

    public void OnInvalidRecipe()
    {
        // Handle invalid crafting recipe
        
        Button_Cast.interactable = false;
        currentSpellData = null;
    }

    #endregion


    
    
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
    
    
    
    #region Player and Enemy Info
    
    
    // LOCAL PLAYER INFO
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

    
    // REMOTE PLAYER INFO
    public void UpdateRemotePlayerInfo(int health)
    {
        Slider_EnemyHealth.value = health;
    }

    
    public void UpdateRemotePlayerInfo(string playerName, int health, Sprite icon)
    {
        Text_EnemyName.text = playerName;
        Slider_EnemyHealth.value = health;
        Image_EnemyIcon.sprite = icon;
    }
    
    #endregion

}
