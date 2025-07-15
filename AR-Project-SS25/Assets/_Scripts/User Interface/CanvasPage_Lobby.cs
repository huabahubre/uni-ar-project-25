using Sirenix.OdinInspector;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;

public class CanvasPage_Lobby : CanvasPage
{
    
    
    [BoxGroup("References"), Header("Local Player Info")]
    public TextMeshProUGUI Text_PlayerNamePlaceholder;
    
    [BoxGroup("References")]
    public TMP_InputField TextInput_PlayerName;
    
    [BoxGroup("References")]
    public Toggle FirstElementToggle;

    
    [BoxGroup("References"), Header("Remote Player Info")]
    public TextMeshProUGUI Text_EnemyName;

    [BoxGroup("References")]
    public Image Image_EnemyElementIcon;
    
    
    
    [BoxGroup("References"), Header("Buttons")]
    public Button Button_Back;
    
    [BoxGroup("References")]
    public Button Button_StartGame;
    
    
    
    [BoxGroup("References"), Header("Panels")]
    public GameObject Panel_Actions;

    [BoxGroup("References")]
    public GameObject Panel_ActiveLobby;
    
    [BoxGroup("References")]
    public GameObject Panel_WaitingForOtherPlayer;


    
    public override void Initialize()
    {
        Button_Back.onClick.AddListener(OnBackButtonClick);
        Button_StartGame.onClick.AddListener(OnStartGame);
        
        // Subscribe to playerName InputField events
        TextInput_PlayerName.onSubmit.AddListener((name) =>
        {
            // Set Player Name on submit
            if (!string.IsNullOrEmpty(TextInput_PlayerName.text))
            {
                if (PlayerState.LocalPlayer != null)
                    PlayerState.LocalPlayer.SetPlayerNameServerRpc(TextInput_PlayerName.text);
            }
        });
        TextInput_PlayerName.onEndEdit.AddListener((name) =>
        {
            // Set Player Name on submit
            if (!string.IsNullOrEmpty(TextInput_PlayerName.text))
            {
                if (PlayerState.LocalPlayer != null)
                    PlayerState.LocalPlayer.SetPlayerNameServerRpc(TextInput_PlayerName.text);
            }
        });
        
        base.Initialize();
    }

    public override void OnShow()
    {
        Panel_Actions.SetActive(!DataManagement.Instance.isRematchLobby);
        Panel_ActiveLobby.SetActive(DataManagement.Instance.isRematchLobby);
        Panel_WaitingForOtherPlayer.SetActive(false);

        // FirstElementToggle.isOn = true;
        
        base.OnShow();
    }
    
    

    #region Joining Lobby
    
    public void OnJoinedLobby()
    {
        Panel_Actions.SetActive(false);
        Panel_ActiveLobby.SetActive(true);
        Button_Back.gameObject.SetActive(false);
        RefreshAllLayoutGroups();
    
        // Check if host
        Button_StartGame.gameObject.SetActive(NetworkManager.Singleton.IsHost);
        Panel_WaitingForOtherPlayer.SetActive(true);
        
        // Subscribe to player updates
        PlayerState.OnEnemyJoined += OnEnemyConnected;
        PlayerState.OnPlayerStateUpdated += HandlePlayerUpdate;
        MainCanvasManagement.Instance.SubscribeToGameStateManager();
        
        MainCanvasManagement.Instance.StopLoading();
    }

    [Button]
    public void OnEnemyConnected(PlayerState playerState)
    {
        Panel_WaitingForOtherPlayer.SetActive(false);
        PlayerState.OnEnemyJoined -= OnEnemyConnected;
    }
    
    
    public void OnStartGame()
    {
        // SyncStartGame.Instance.gameStarted = true;
        GameStateManager.Instance.SetGameState(GameState.Gameplay);
    }
    
    
    #endregion
    
    #region Player State Management
    
    
    [Button]
    public void SetLocalPlayerElementIndex(int index)
    {
        if (PlayerState.LocalPlayer != null)
            PlayerState.LocalPlayer.SetElementIndexServerRpc(index);
    }
    

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
            // e.g., Update element icon, name, etc.
            // Debug.Log($"Local Player Element Index: {player.ElementIndex.Value}");
            
        }
        else
        {
            // Update remote player UI
            // e.g., Update enemy element icon, name, etc.
            // Debug.Log($"Remote Player Element Index: {player.ElementIndex.Value}");
            
            Text_EnemyName.text = player.PlayerName.Value.ToString();
            Image_EnemyElementIcon.sprite = DataManagement.Instance.GetElementVisualData(player.ElementIndex.Value).Icon;
        }
    }

    
    #endregion
    
    
    
    void OnBackButtonClick()
    {
        // TODO: First Leave lobby, then go back to menu
        // 1. Leave Lobby
        // 2. Wait for Callback
        // 3. Show Menu
        MainCanvasManagement.Instance.ShowPage("Menu");
    }
    
    private void OnDisable()
    {
        PlayerState.OnPlayerStateUpdated -= HandlePlayerUpdate;
    }
    
}
