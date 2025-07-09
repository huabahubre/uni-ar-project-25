using System;
using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Services.Matchmaker.Models;
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
    
    [BoxGroup("References")]
    public Image Image_PlayerIconBackground;
    
    
    [BoxGroup("References"), Header("Enemy Info")]
    public TextMeshProUGUI Text_EnemyName;
    
    [BoxGroup("References")]
    public Slider Slider_EnemyHealth;

    [BoxGroup("References")]
    public Image Image_EnemyIcon;
    
    [BoxGroup("References")]
    public Image Image_EnemyIconBackground;



    private Tuple<SpellType?, ElementType?> currentSpellData = null;
    public Action<Tuple<SpellType?, ElementType?>> onCastSpell;
    
    
    
    private bool isSubscribed = false;
    
    public override void Initialize()
    {
        Button_Pause.onClick.AddListener(OnPauseButtonClick);
        Button_Cast.onClick.AddListener(OnCastSpell);
        
        // Subscribe to events
        PlayfieldManagement.Instance.onValidCraftingRecipeFound += OnValidRecipeFound;
        PlayfieldManagement.Instance.onRecipeInvalid += OnInvalidRecipe;
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

        // Setup Information
        SetupPlayerAndEnemyInfo();
        
        base.OnShow();
    }


    void OnPauseButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Pause");
    }

    #region Init

    void SetupPlayerAndEnemyInfo()
    {
        // Subscribe to player updates
        if (!isSubscribed)
        {
            isSubscribed = true;
            PlayerState.OnPlayerStateUpdated += HandlePlayerUpdate;
        }


        // Set fixed stuff once
        Image_PlayerIcon.sprite = DataManagement.Instance.GetElementVisualData(PlayerState.LocalPlayer.ElementIndex.Value).Icon;
        Image_PlayerIconBackground.color = DataManagement.Instance
            .GetElementVisualData(PlayerState.LocalPlayer.ElementIndex.Value).Color;
        Text_PlayerName.text = PlayerState.LocalPlayer.PlayerName.Value.ToString();
        
        Image_EnemyIcon.sprite = DataManagement.Instance.GetElementVisualData(PlayerState.EnemyPlayer.ElementIndex.Value).Icon;
        Image_EnemyIconBackground.color = DataManagement.Instance
            .GetElementVisualData(PlayerState.EnemyPlayer.ElementIndex.Value).Color;
        Text_EnemyName.text = PlayerState.EnemyPlayer.PlayerName.Value.ToString();
        
        // Manually update player states
        HandlePlayerUpdate(PlayerState.EnemyPlayer);
        HandlePlayerUpdate(PlayerState.LocalPlayer);
    }
    
    #endregion
    
    #region Player and Enemy Updates
    
    private void HandlePlayerUpdate(PlayerState player)
    {
        if(player == null)
        {
            Debug.Log("PlayerState is null in HandlePlayerUpdate");
            return;
        }
        
        Debug.Log($"[UI] Player {player.OwnerClientId} ({(player.IsLocalPlayer ? "Local" : "Remote")}) updated ElementIndex to {player.ElementIndex.Value}");
        
        bool isLocalPlayer = player.IsLocalPlayer;
        if (isLocalPlayer)
        {
            // Update local player UI
            Slider_PlayerHealth.value = PlayerState.LocalPlayer.PlayerHealth.Value;
        }
        else
        {
            // Update remote player UI
            Slider_EnemyHealth.value = PlayerState.EnemyPlayer.PlayerHealth.Value;
        }
    }
    
    #endregion
    

    #region Turn funcitonality

    public void OnFinishedTurn()
    {
        bool isPlayerTurn = GameStateManager.Instance.IsCurrentPlayersTurn();
        
        // Update UI
        // UpdateLocalPlayerInfo(GameStateManager.Instance.player1HP.Value);
        // UpdateRemotePlayerInfo(GameStateManager.Instance.player2HP.Value);
        
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
    

}
