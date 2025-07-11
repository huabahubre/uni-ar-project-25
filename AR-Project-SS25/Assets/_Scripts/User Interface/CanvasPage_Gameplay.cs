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

    [BoxGroup("References")] public Button Button_Cast;


    [BoxGroup("References"), Header("Panels")]
    public GameObject Panel_YourTurn;

    [BoxGroup("References")] public GameObject Panel_OpponentTurn;

    [BoxGroup("References")] public GameObject Panel_ActiveSpellCasting;


    [BoxGroup("References"), Header("Player Info")]
    public TextMeshProUGUI Text_PlayerName;

    [BoxGroup("References")] public Slider Slider_PlayerHealth;

    [BoxGroup("References")] public Slider Slider_PlayerEnergy;

    [BoxGroup("References")] public Image Image_PlayerIcon;

    [BoxGroup("References")] public Image Image_PlayerIconBackground;


    [BoxGroup("References"), Header("Enemy Info")]
    public TextMeshProUGUI Text_EnemyName;

    [BoxGroup("References")] public Slider Slider_EnemyHealth;

    [BoxGroup("References")] public Image Image_EnemyIcon;

    [BoxGroup("References")] public Image Image_EnemyIconBackground;



    private Tuple<SpellType?, ElementType?> currentSpellData = null;
    public Action<Tuple<SpellType?, ElementType?>> onCastSpell;



    private bool isSubscribed = false;
    private bool isFirstTurn = true;

    public override void Initialize()
    {
        Button_Pause.onClick.AddListener(OnPauseButtonClick);
        Button_Cast.onClick.AddListener(OnCastSpell);

        // Subscribe to events
        PlayfieldManagement.Instance.onValidCraftingRecipeFound += OnValidRecipeFound;
        PlayfieldManagement.Instance.onRecipeInvalid += OnInvalidRecipe;

        base.Initialize();
    }


    public override void OnShow()
    {
        // Subscribe to events
        if (!isSubscribed)
        {
            isSubscribed = true;
            PlayerState.OnPlayerStateUpdated += HandlePlayerUpdate;
            GameStateManager.OnLocalTurnChanged += OnLocalTurnChanged;
            GameStateManager.OnActivePlayerConfirmedSpellCast += OnStartSpellCastAnimation;
            
            // Debug.Log( "[UI] Subscribed to PlayerState and GameStateManager events.");
        }

        // Set Panels
        Panel_YourTurn.SetActive(false);
        Panel_OpponentTurn.SetActive(false);
        Panel_ActiveSpellCasting.SetActive(false);

        // Setup Information
        SetupPlayerAndEnemyInfo();

        // Show scan screen
        MainCanvasManagement.Instance.ShowScanScreen("Please scan the playfield marker to start the game!");

        base.OnShow();
    }


    void OnPauseButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Pause");
    }

    #region Init

    void SetupPlayerAndEnemyInfo()
    {
        // Set fixed stuff once
        Image_PlayerIcon.sprite =
            DataManagement.Instance.GetElementVisualData(PlayerState.LocalPlayer.ElementIndex.Value).Icon;
        Image_PlayerIconBackground.color = DataManagement.Instance
            .GetElementVisualData(PlayerState.LocalPlayer.ElementIndex.Value).Color;
        Text_PlayerName.text = PlayerState.LocalPlayer.PlayerName.Value.ToString();

        Image_EnemyIcon.sprite =
            DataManagement.Instance.GetElementVisualData(PlayerState.EnemyPlayer.ElementIndex.Value).Icon;
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
        if (player == null)
        {
            Debug.Log("[UI] PlayerState is null in HandlePlayerUpdate");
            return;
        }

        // Debug.Log("[UI] Handling player update for: " + player.PlayerName.Value);

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


    void OnStartSpellCastAnimation()
    {
        // Debug.Log("[UI] Spell cast animation started.");

        // Set Panels
        Panel_YourTurn.SetActive(false);
        Panel_OpponentTurn.SetActive(false);
        Panel_ActiveSpellCasting.SetActive(true);
    }

    void OnLocalTurnChanged(bool isPlayerTurn)
    {
        if (isFirstTurn)
        {
            isFirstTurn = false;
            MainCanvasManagement.Instance.StopLoading();
            Debug.Log("[UI] First turn initialized, stopping loading screen.");
        }

        // Debug.Log("[UI] Local turn changed: " + isPlayerTurn);


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

        // Debug.Log("Player trying to cast spell: " + currentSpellData);

        // Try to cast spell with Action
        onCastSpell?.Invoke(currentSpellData);


        GameStateManager.Instance.ConfirmSpellCastServerRpc((int)currentSpellData.Item2, (int)currentSpellData.Item1);

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
}
