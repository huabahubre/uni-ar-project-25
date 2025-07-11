using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Android.Gradle;
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
    }
    
    
    #endregion
    
    
    
    // Synced Game State
    public NetworkVariable<GameState> CurrentGameState = new NetworkVariable<GameState>(
        GameState.Lobby,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    // Synced Turn Phase
    private TurnPhase currentTurnPhase = TurnPhase.None;

    // The client ID of the active player (already declared as: activePlayerClientId)
    public NetworkVariable<ulong> activePlayerClientId = new NetworkVariable<ulong>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    // Track player readiness --> both have to scan the playfield before the game starts
    private NetworkVariable<bool> player1Ready = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> player2Ready = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    
    
    public static event Action<GameState> GameStateChanged;
    public static event Action<bool> OnLocalTurnChanged;
    public static event Action OnActivePlayerConfirmedSpellCast;

    
    
    
    
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

    #region Ready Up (Scan the playfield)
    
    
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
    
    #endregion
    
    
    #region 1. Confirm Spell cast
    
    // Called from client once they try to cast their spell
    [ServerRpc(RequireOwnership = false)]
    public void ConfirmSpellCastServerRpc(int elementId, int spellTypeId, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        ulong sender = rpcParams.Receive.SenderClientId;
        if (sender != activePlayerClientId.Value)
        {
            Debug.LogWarning($"[Server] Client {sender} tried to act out of turn.");
            return;
        }

        Debug.Log($"[Server] Spell cast confirmed by ClientId: {sender}, Element: {(ElementType)elementId}, SpellType: {(SpellType)spellTypeId}");
        
        HandleSpellCastStart(sender, elementId, spellTypeId);
    }

    
    #endregion

    #region 2. Wait for Spell Animation

    private SpellType currentSpellType;
    private ElementType currentElementType;
    private ulong currentCasterId;

    private void HandleSpellCastStart(ulong casterClientId, int elementId, int spellTypeId)
    {
        currentTurnPhase = TurnPhase.Casting;
    
        currentCasterId = casterClientId;
        currentSpellType = (SpellType)spellTypeId;
        currentElementType = (ElementType)elementId;

        // Debug.Log($"[Server] Broadcasting spell animation: {currentSpellType} of {currentElementType}");

        PlaySpellAnimationClientRpc(casterClientId, elementId, spellTypeId);
    }

    [ClientRpc]
    private void PlaySpellAnimationClientRpc(ulong casterClientId, int elementId, int spellTypeId)
    {
        bool isLocalPlayerCaster = (casterClientId == NetworkManager.Singleton.LocalClientId);
        SpellType spellType = (SpellType)spellTypeId;
        ElementType elementType = (ElementType)elementId;
        
        Debug.Log($"[Client] Playing spell animation: {spellType} of {elementType}");

        // Set Callbacks for UI updates
        OnActivePlayerConfirmedSpellCast?.Invoke();
        
        // Spawn the spell
        SpellManager.Instance.SpawnSpell(isLocalPlayerCaster, spellType, elementType);
        // StartCoroutine(SpellAnimationRoutine());
    }

    // TODO: THIS IS ONLY FOR DEBUGGING --> SKIPPING ANIMATION HERE!
    private System.Collections.IEnumerator SpellAnimationRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        NotifySpellAnimationCompleteServerRpc();
    }
    
    
    private HashSet<ulong> animationFinishedClients = new();

    // Call this from both clients, when spell animation is complete
    [ServerRpc(RequireOwnership = false)]
    public void NotifySpellAnimationCompleteServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;

        animationFinishedClients.Add(sender);

        if (animationFinishedClients.Count >= NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            animationFinishedClients.Clear();
            HandleSpellCastResolve();
        }
    }
    
    
    #endregion
    
    #region 3. Resolve Spell Damage
    
    
    // TODO: @Dawin Calculate the damage of the spell here
    private void HandleSpellCastResolve()
    {
        Debug.Log($"[Server] Resolving spell cast: {currentSpellType} of {currentElementType} by ClientId: {currentCasterId}");
        
        // THIS IS HOW YOU GET ALL THE REFERENCES TO ALL PLAYERS
        bool isHostCastingPlayer = (currentCasterId == NetworkManager.Singleton.ConnectedClientsIds[0]);   // We check here which player is the caster --> Server == Host == PlayerState.LocalPlayer
        PlayerState castingPlayer = isHostCastingPlayer ? PlayerState.LocalPlayer : PlayerState.EnemyPlayer;    // This is the spell caster
        PlayerState targetPlayer = isHostCastingPlayer ? PlayerState.EnemyPlayer : PlayerState.LocalPlayer;     // This is the target player
        ElementType casterElement = (ElementType)castingPlayer.ElementIndex.Value; // The element of the casting player
        ElementType targetElement = (ElementType)targetPlayer.ElementIndex.Value; // The element of the target player
        int damage = UnityEngine.Random.Range(10, 25); // EXAMPLE RANDOM DAMAGE

        // CALCULATING DAMAGE BASED ON SPELL TYPE TODO: @Dawin Implement actual damage calculation based on spell type and casterElement / targetElement
        switch (currentSpellType)
        {
            case SpellType.SingleShot:
                damage = 10;
                break;
            case SpellType.Spear:
                damage = 30;
                break;
            case SpellType.WideShot:
                damage = 50;
                break;
            case SpellType.Shield:
                damage = 0; // Set damage to 0, as shield does not deal damage
                castingPlayer.IsShieldActive.Value = true; // Activate shield for the caster
                castingPlayer.ActiveShieldElement.Value = (int)currentElementType; // Set the shield element for the caster
                break;
            case SpellType.GroundPound:
                damage = 80;
                break;
        }
        
        
        // CHECK HERE IF TARGET HAS A SHIELD ACTIVE TODO: @Dawin Calculate damage reduction based on shield element
        if (targetPlayer.IsShieldActive.Value)
        {
            ElementType targetShieldType = (ElementType)targetPlayer.ActiveShieldElement.Value; // The element of the target player's shield
            damage = 0; // --> Calculate damage reduction
            
            // Disable shield after use
            targetPlayer.IsShieldActive.Value = false;
        }
        
        
        // UPDATE HEALTH OF THE TARGET PLAYER
        targetPlayer.UpdatePlayerHealthServerRpc(-damage);

        Debug.Log($"[Server] Spell resolved: Player {(isHostCastingPlayer ? "2" : "1")} took {damage} damage");

        CheckWinCondition();
    }
    
    #endregion

    #region 4. Check Win condition
    
    
    private void CheckWinCondition()
    {
        bool localDead = PlayerState.LocalPlayer.PlayerHealth.Value <= 0;
        bool enemyDead = PlayerState.EnemyPlayer.PlayerHealth.Value <= 0;

        if (localDead || enemyDead)
        {
            ulong winner = localDead ? PlayerState.EnemyPlayer.OwnerClientId : PlayerState.LocalPlayer.OwnerClientId;

            Debug.Log($"[Server] Game over. Winner is ClientId: {winner}");

            AnnounceGameOverClientRpc(winner);

            SetGameState(GameState.GameOver);
            return;
        }

        
        // If no one is dead, just end the turn
        EndTurn();
    }
    
    private bool isLocalPlayerWinner = false;
    
    [ClientRpc]
    private void AnnounceGameOverClientRpc(ulong winnerId)
    {
        Debug.Log($"[Client] GameOver RPC. Winner: {winnerId}, You: {NetworkManager.Singleton.LocalClientId}");

        isLocalPlayerWinner = winnerId == NetworkManager.Singleton.LocalClientId;
        
        if (isLocalPlayerWinner)
            Debug.Log("[Client] YOU WIN (via RPC)!");
        else
            Debug.Log("[Client] YOU LOSE (via RPC)!");

        MainCanvasManagement.Instance.ShowPage("GameOver");

        GameStateChanged?.Invoke(GameState.GameOver);
    }


    
    
    // Called from client to forfeit the game
    [ServerRpc(RequireOwnership = false)]
    public void SurrenderServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        ulong surrenderingClientId = rpcParams.Receive.SenderClientId;

        var ids = NetworkManager.Singleton.ConnectedClientsIds;
        if (ids.Count < 2) return;

        // Determine the winner (the other player)
        ulong winner = ids[0] == surrenderingClientId ? ids[1] : ids[0];
        AnnounceGameOverClientRpc(winner);
        
        Debug.Log($"[Server] Client {surrenderingClientId} surrendered. Winner is ClientId {winner}");

        SetGameState(GameState.GameOver);
    }

    public bool IsLocalPlayerWinner()
    {
        return isLocalPlayerWinner;
    }

    
    #endregion
    
    #region 5. End of Turn
    
    private void EndTurn()
    {
        currentTurnPhase = TurnPhase.End;

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
        
        Debug.Log($"[Client] It is {(isMyTurn ? "YOUR" : "ENEMY")} turn.");
    }
    
    #endregion
    
    

    private void BeginTurn()
    {
        if (!IsServer) return;

        currentTurnPhase = TurnPhase.Start;
        // Debug.Log($"[Server] Turn started for ClientId: {activePlayerClientId.Value}");

        // Notify both clients whose turn it is
        NotifyTurnClientRpc(activePlayerClientId.Value);
    }
    

#endregion

    
#region Rematch Logic --> This is not working for a second match ONLY FOR VIDEO

    private NetworkVariable<bool> player1Rematch = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> player2Rematch = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public static event Action<bool> OnLocalRematchUpdated;
    public static event Action OnRematchOffered;

    
    [ServerRpc(RequireOwnership = false)]
    public void RequestRematchServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;
        var ids = NetworkManager.Singleton.ConnectedClientsIds;

        if (ids.Count < 2) return;

        if (sender == ids[0])
        {
            player1Rematch.Value = true;
            Debug.Log("Player 1 requested a rematch.");
        }
        else if (sender == ids[1])
        {
            player2Rematch.Value = true;
            Debug.Log("Player 2 requested a rematch.");
        }

        // Notify the *other* player
        NotifyRematchOfferedClientRpc(sender);

        CheckRematchState();
    }


    [ServerRpc(RequireOwnership = false)]
    public void CancelRematchServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;
        var ids = NetworkManager.Singleton.ConnectedClientsIds;

        if (ids.Count < 2) return;

        if (sender == ids[0])
            player1Rematch.Value = false;
        else if (sender == ids[1])
            player2Rematch.Value = false;

        Debug.Log($"Player {sender} canceled rematch.");

        CancelRematchClientRpc();
    }

    private void CheckRematchState()
    {
        if (player1Rematch.Value && player2Rematch.Value)
        {
            Debug.Log("[Server] Both players accepted rematch. Restarting...");

            ResetGameState();
        }
    }

    private void ResetGameState()
    {
        // Reset all necessary values before returning to Lobby
        player1Rematch.Value = false;
        player2Rematch.Value = false;
        player1Ready.Value = false;
        player2Ready.Value = false;
        currentTurnPhase = TurnPhase.None;

        // Reset health values
        // PlayerState.LocalPlayer.ResetPlayerServerRpc();
        // PlayerState.EnemyPlayer.ResetPlayerServerRpc();

        SetGameState(GameState.Lobby);
    }

    [ClientRpc]
    private void CancelRematchClientRpc()
    {
        Debug.Log("[Client] Rematch canceled by opponent.");
        OnLocalRematchUpdated?.Invoke(false);
    }
    
    [ClientRpc]
    private void NotifyRematchOfferedClientRpc(ulong offeringClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == offeringClientId)
            return; // Don't show it to the one who offered

        Debug.Log($"[Client] Received rematch offer from Client {offeringClientId}");
        
        OnRematchOffered?.Invoke(); // Show rematch UI
    }


#endregion

    
    
    // [OBSOLETE] 
    // public NetworkVariable<int> player1HP = new NetworkVariable<int>(100, 
    //     NetworkVariableReadPermission.Everyone,
    //     NetworkVariableWritePermission.Server);
    //
    // public NetworkVariable<int> player2HP = new NetworkVariable<int>(100, 
    //     NetworkVariableReadPermission.Everyone,
    //     NetworkVariableWritePermission.Server);
    
    // public NetworkVariable<ulong> activePlayerClientId = new NetworkVariable<ulong>(
    //     0, 
    //     NetworkVariableReadPermission.Everyone,
    //     NetworkVariableWritePermission.Server);
    
    // public Action onFinishedTurn;
    
    #region [OBSOLETE] OnNetworkSpawn
    
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

    #region [OBSOLETE] NetworkVariable Updates
    
    
    // [ServerRpc(RequireOwnership = false)] // From client to the server
    // public void EndTurnRequestServerRpc(int damage, ServerRpcParams rpcParams = default)
    // {
    //     ulong requestingClientId = rpcParams.Receive.SenderClientId;
    //     Debug.Log($"RPC EndTurnRequestServerRpc: {requestingClientId}");
    //
    //     if (requestingClientId != activePlayerClientId.Value)
    //     {
    //         Debug.LogWarning($"Client {requestingClientId} tried to act out of turn. It is {activePlayerClientId.Value}s turn.");
    //         return;
    //     }
    //
    //     if (IsServer)
    //     {
    //         UpdateHealth(damage, requestingClientId);
    //         UpdateTurn();
    //         CheckWinCondition();
    //     }
    // }

    // [Button]
    // private void UpdateHealth(int damage, ulong requestingClientId)
    // {
    //     bool playerOneAttacks = NetworkManager.Singleton.ConnectedClientsIds[0] == requestingClientId;
    //     
    //     if (playerOneAttacks)
    //     {
    //         // Player 1 is attacking → damage Player 2
    //         string x = $"Player {player2HP.Value} health reduced by {damage}";
    //         player2HP.Value = Mathf.Max(0, player2HP.Value - damage);
    //         
    //         // Update PlayerState of LocalPlayer
    //         PlayerState.LocalPlayer.UpdatePlayerHealthServerRpc(-damage);
    //         
    //         
    //         Debug.Log($"{x}. New health {player2HP.Value}");
    //     }
    //     else
    //     {
    //         // Player 2 is attacking → damage Player 1
    //         string x = $"Player {player1HP.Value} health reduced by {damage}";
    //         player1HP.Value = Mathf.Max(0, player1HP.Value - damage);
    //         
    //         // Update PlayerState of Enemy
    //         PlayerState.EnemyPlayer.UpdatePlayerHealthServerRpc(-damage);
    //         
    //         Debug.Log($"{x}. New health {player1HP.Value}");
    //     }
    // }
    
    // private void UpdateTurn()
    // {
    //     // Switch active player
    //     if (activePlayerClientId.Value == NetworkManager.Singleton.ConnectedClientsIds[0])
    //     {
    //         activePlayerClientId.Value = NetworkManager.Singleton.ConnectedClientsIds[1];
    //         Debug.Log($"Turn changed to Player 2 (ClientId: {activePlayerClientId.Value})");
    //     }
    //     else
    //     {
    //         activePlayerClientId.Value = NetworkManager.Singleton.ConnectedClientsIds[0];
    //         Debug.Log($"Turn changed to Player 1 (ClientId: {activePlayerClientId.Value})");
    //     }
    //     
    //     onFinishedTurn?.Invoke();
    // }
    //
    // private void CheckWinCondition()
    // {
    //     if (player1HP.Value <= 0)
    //         Debug.Log("Player 2 wins!");
    //         // Update UI, etc.
    //     else if (player2HP.Value <= 0)
    //         Debug.Log("Player 1 wins!");
    //         // Update UI, etc.
    // }
    
    
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
