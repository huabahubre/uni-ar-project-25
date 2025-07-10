using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public enum GameState
{
    Lobby,
    Gameplay,
    GameOver
}

public enum TurnPhase
{
    None,
    Start,
    Casting,
    End
}

public class GameStateManager : NetworkBehaviour
{
    
    public static GameStateManager Instance;
    
    #region Awake and OnDestroy
    
    void Awake()
    {
        Debug.Log("Initializing GameStateManager");
        
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    
    private void OnDestroy()
    {
        CurrentGameState.OnValueChanged -= OnGameStateChanged;
        PlayerState.OnPlayerStateUpdated -= HandlePlayerUpdate;
    }
    
    
    #endregion
    
    
    
    // Synced Game State
    public NetworkVariable<GameState> CurrentGameState = new NetworkVariable<GameState>(
        GameState.Lobby,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    public static event Action<GameState> GameStateChanged;
    
    
    
    // Synced Turn Phase
    private TurnPhase currentTurnPhase = TurnPhase.None;

    // The client ID of the active player (already declared as: activePlayerClientId)
    public NetworkVariable<ulong> activePlayerClientId = new NetworkVariable<ulong>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    public static event Action<bool> OnLocalTurnChanged;
    
    
    
    // Track player readiness --> both have to scan the playfield before the game starts
    private NetworkVariable<bool> player1Ready = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> player2Ready = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    
    
    
    
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log("[GameStateManager] OnNetworkSpawn called");

        // Subscribe to game state changes
        CurrentGameState.OnValueChanged += OnGameStateChanged;

        // Set initial state on server if not already set
        if (IsServer && CurrentGameState.Value == GameState.Lobby)
        {
            SetGameState(GameState.Lobby);
        }

        
        // OBSOLETE
        // PlayerState.OnPlayerStateUpdated += HandlePlayerUpdate;
        // AssignFirstPlayerServer();
    }

    
    
    
    
    
    #region GameState
    
    /// <summary>
    /// Server-only: Changes the game state and notifies clients and local subscribers.
    /// </summary>
    public void SetGameState(GameState newState)
    {
        if (!IsServer) return;

        Debug.Log($"[GameState] changed to: {newState}");
        
        CurrentGameState.Value = newState;
        GameStateChanged?.Invoke(newState);
    }

    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        Debug.Log($"[Client] GameState changed from {oldState} to {newState}");
        GameStateChanged?.Invoke(newState);
    }
    
    #endregion
    
    #region Turn Logic

    // Called by both players when they are ready
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        var ids = NetworkManager.Singleton.ConnectedClientsIds;

        if (ids.Count < 2) return;

        if (senderId == ids[0])
        {
            player1Ready.Value = true;
            Debug.Log("Player 1 is ready.");
        }
        else if (senderId == ids[1])
        {
            player2Ready.Value = true;
            Debug.Log("Player 2 is ready.");
        }

        // Check if both are ready
        if (player1Ready.Value && player2Ready.Value)
        {
            Debug.Log("[Server] Both players are ready. Starting game...");
            SelectRandomFirstPlayerAndStart();
        }
    }

    private void SelectRandomFirstPlayerAndStart()
    {
        var ids = NetworkManager.Singleton.ConnectedClientsIds;
        if (ids.Count < 2) return;

        // Randomly pick who goes first
        int randomIndex = UnityEngine.Random.Range(0, 2);
        activePlayerClientId.Value = ids[randomIndex];

        Debug.Log($"[Server] Randomly selected starting player: ClientId {activePlayerClientId.Value}");

        BeginTurn();
    }
    

    private void BeginTurn()
    {
        if (!IsServer) return;

        currentTurnPhase = TurnPhase.Start;
        Debug.Log($"[Server] Turn started for ClientId: {activePlayerClientId.Value}");

        // Notify both clients whose turn it is
        NotifyTurnClientRpc(activePlayerClientId.Value);
    }

    
    // Called from client once they try to cast their spell
    [ServerRpc(RequireOwnership = false)]
    public void ConfirmSpellCastServerRpc(int spellTypeId, int elementId, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        ulong sender = rpcParams.Receive.SenderClientId;
        if (sender != activePlayerClientId.Value)
        {
            Debug.LogWarning($"[Server] Client {sender} tried to act out of turn.");
            return;
        }

        Debug.Log($"[Server] Spell cast confirmed by ClientId: {sender}, Element: {(ElementType)elementId}, SpellType: {(SpellType)spellTypeId}");
        
        HandleSpellCast(sender, spellTypeId, elementId);
    }

    
    private void HandleSpellCast(ulong casterClientId, int spellTypeId, int elementId)
    {
        currentTurnPhase = TurnPhase.Casting;

        int damage = UnityEngine.Random.Range(10, 25); // Example fixed spell damage

        // Determine who gets damaged
        bool casterIsPlayer1 = (casterClientId == NetworkManager.Singleton.ConnectedClientsIds[0]);

        if (casterIsPlayer1)
        {
            player2HP.Value = Mathf.Max(0, player2HP.Value - damage);
            PlayerState.EnemyPlayer.UpdatePlayerHealthServerRpc(-damage);
        }
        else
        {
            player1HP.Value = Mathf.Max(0, player1HP.Value - damage);
            PlayerState.LocalPlayer.UpdatePlayerHealthServerRpc(-damage);
        }

        Debug.Log($"[Server] Player {(casterIsPlayer1 ? "2" : "1")} took {damage} damage");

        EndTurn();
    }

    private void EndTurn()
    {
        currentTurnPhase = TurnPhase.End;

        CheckWinCondition();

        if (player1HP.Value <= 0 || player2HP.Value <= 0)
        {
            Debug.Log("[Server] Game over.");
            SetGameState(GameState.GameOver);
            return;
        }

        // Switch active player
        ulong current = activePlayerClientId.Value;
        ulong next = NetworkManager.Singleton.ConnectedClientsIds[0] == current
            ? NetworkManager.Singleton.ConnectedClientsIds[1]
            : NetworkManager.Singleton.ConnectedClientsIds[0];

        activePlayerClientId.Value = next;

        Debug.Log($"[Server] Turn switched to ClientId: {next}");

        BeginTurn();
    }

    [ClientRpc]
    private void NotifyTurnClientRpc(ulong currentPlayerId)
    {
        bool isMyTurn = (currentPlayerId == NetworkManager.Singleton.LocalClientId);
        
        OnLocalTurnChanged?.Invoke(isMyTurn);
        
        Debug.Log($"[Client] It is {(isMyTurn ? "my" : "their")} turn.");
    }

    // Optional helper
    public bool IsMyTurn()
    {
        return activePlayerClientId.Value == NetworkManager.Singleton.LocalClientId;
    }

#endregion

    
    
    
    

    public NetworkVariable<int> player1HP = new NetworkVariable<int>(100, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    public NetworkVariable<int> player2HP = new NetworkVariable<int>(100, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    // public NetworkVariable<ulong> activePlayerClientId = new NetworkVariable<ulong>(
    //     0, 
    //     NetworkVariableReadPermission.Everyone,
    //     NetworkVariableWritePermission.Server);


    public Action onFinishedTurn;
    
    
    
    // TODO: create shield network variable with type and health


    public bool IsCurrentPlayersTurn()
    {
        return activePlayerClientId.Value == NetworkManager.Singleton.LocalClientId;
    }
    
    #region OnNetworkSpawn
    
    private void AssignFirstPlayerServer()
    {
        Debug.Log("Player ids: " + string.Join(", ", NetworkManager.Singleton.ConnectedClientsIds));
        Debug.Log("Assigning first player: IsServer: " + IsServer + ", playerCount: " + NetworkManager.Singleton.ConnectedClientsIds.Count);
        
        if (IsServer && activePlayerClientId.Value == 0 && NetworkManager.Singleton.ConnectedClientsIds.Count > 0)
        {
            activePlayerClientId.Value = NetworkManager.Singleton.ConnectedClientsIds[0];
            Debug.Log($"Assigned Player 1 to clientId: {activePlayerClientId.Value}");
        }
        else if (IsServer && activePlayerClientId.Value == 0)
        {
            Invoke(nameof(AssignFirstPlayerServer), 2f); // Retry if not yet connected
        }
    }
    
    private void OnClientConnected(ulong clientId)
    {
        if (activePlayerClientId.Value == 0) // not yet set
        {
            activePlayerClientId.Value = clientId;
            Debug.Log($"Assigned Active player to clientId: {clientId}");
        }
    }
    
    
    
    #endregion

    #region NetworkVariable Updates
    
    [ServerRpc(RequireOwnership = false)] // From client to the server
    public void EndTurnRequestServerRpc(int damage, ServerRpcParams rpcParams = default)
    {
        ulong requestingClientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"RPC EndTurnRequestServerRpc: {requestingClientId}");

        if (requestingClientId != activePlayerClientId.Value)
        {
            Debug.LogWarning($"Client {requestingClientId} tried to act out of turn. It is {activePlayerClientId.Value}s turn.");
            return;
        }

        if (IsServer)
        {
            UpdateHealth(damage, requestingClientId);
            UpdateTurn();
            CheckWinCondition();
        }
    }

    [Button]
    private void UpdateHealth(int damage, ulong requestingClientId)
    {
        bool playerOneAttacks = NetworkManager.Singleton.ConnectedClientsIds[0] == requestingClientId;
        if (playerOneAttacks)
        {
            // Player 1 is attacking → damage Player 2
            string x = $"Player {player2HP.Value} health reduced by {damage}";
            player2HP.Value = Mathf.Max(0, player2HP.Value - damage);
            
            // Update PlayerState of LocalPlayer
            PlayerState.LocalPlayer.UpdatePlayerHealthServerRpc(-damage);
            
            
            Debug.Log($"{x}. New health {player2HP.Value}");
        }
        else
        {
            // Player 2 is attacking → damage Player 1
            string x = $"Player {player1HP.Value} health reduced by {damage}";
            player1HP.Value = Mathf.Max(0, player1HP.Value - damage);
            
            // Update PlayerState of Enemy
            PlayerState.EnemyPlayer.UpdatePlayerHealthServerRpc(-damage);
            
            Debug.Log($"{x}. New health {player1HP.Value}");
        }
    }
    
    private void UpdateTurn()
    {
        // Switch active player
        if (activePlayerClientId.Value == NetworkManager.Singleton.ConnectedClientsIds[0])
        {
            activePlayerClientId.Value = NetworkManager.Singleton.ConnectedClientsIds[1];
            Debug.Log($"Turn changed to Player 2 (ClientId: {activePlayerClientId.Value})");
        }
        else
        {
            activePlayerClientId.Value = NetworkManager.Singleton.ConnectedClientsIds[0];
            Debug.Log($"Turn changed to Player 1 (ClientId: {activePlayerClientId.Value})");
        }
        
        onFinishedTurn?.Invoke();
    }
    
    private void CheckWinCondition()
    {
        if (player1HP.Value <= 0)
            Debug.Log("Player 2 wins!");
            // Update UI, etc.
        else if (player2HP.Value <= 0)
            Debug.Log("Player 1 wins!");
            // Update UI, etc.
    }
    
    
    #endregion
    
    #region NetworkVariable Subscriptions


    private void HandlePlayerUpdate(PlayerState player)
    {
        
    }
    
    private void OnIsServerTurnChanged(bool oldValue, bool newValue)
    {
        // Update UI, etc.
    }
    
    
    #endregion
    
    #region Debug checks

    private void CheckValues()
    {
        base.OnNetworkSpawn();

        Debug.Log($"--- GameStateManager OnNetworkSpawn ---");
        Debug.Log($"NetworkBehaviour.IsServer: {IsServer}"); // Will likely be False for host, False for client
        Debug.Log($"NetworkBehaviour.IsOwner: {IsOwner}");   // Will likely be True for host, False for client
        Debug.Log($"NetworkBehaviour.IsSpawned: {IsSpawned}"); // Will be True for both

        Debug.Log($"NetworkManager.Singleton.IsServer: {NetworkManager.Singleton.IsServer}"); // Will likely be False for host, False for client *at this exact point*
        Debug.Log($"NetworkManager.Singleton.IsHost: {NetworkManager.Singleton.IsHost}");     // Will be True for host, False for client
        Debug.Log($"NetworkManager.Singleton.IsClient: {NetworkManager.Singleton.IsClient}");   // Will be True for host, True for client
        Debug.Log($"NetworkManager.Singleton.LocalClientId: {NetworkManager.Singleton.LocalClientId}");

        // For GameStateManager logic:
        if (NetworkManager.Singleton.IsHost)
        {
            // This code runs only on the host (editor instance)
            // It will handle server-authoritative logic for the game state.
            Debug.Log("GameStateManager: Running as Host (Server + Client). This is the authoritative instance.");
            // Example: Initialize game rounds, manage global timers, etc.
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            // This code runs on dedicated clients, and also on the client-side of the host
            // (but the IsHost check handles the authoritative part for the host)
            Debug.Log("GameStateManager: Running as Client. Observing game state.");
            // Example: Update UI based on NetworkVariables, react to RPCs.
        }
        
        Invoke(nameof(CheckValues), 1f);
    }

    #endregion
}
