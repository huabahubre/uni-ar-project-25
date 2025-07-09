using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance;

    public NetworkVariable<int> player1HP = new NetworkVariable<int>(100, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    public NetworkVariable<int> player2HP = new NetworkVariable<int>(100, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    public NetworkVariable<ulong> activePlayerClientId = new NetworkVariable<ulong>(
        0, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    
    [SerializeField] private GameObject playerStateManagerPrefab;


    public Action onFinishedTurn;
    
    
    
    // TODO: create shield network variable with type and health

    void Awake()
    {
        Debug.Log("Initializing GameStateManager");
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("GameStateManager OnNetworkSpawn called");
        
        // CheckValues();
        
        // player1HP.OnValueChanged += OnPlayer1HealthChanged;
        // player2HP.OnValueChanged += OnPlayer2HealthChanged;
        
        PlayerState.OnPlayerStateUpdated += HandlePlayerUpdate;
        
        //NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected; TODO: test later by using a build
        AssignFirstPlayerServer();
    }
    
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
        
        var instance = Instantiate(playerStateManagerPrefab);
        var netObj = instance.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId); // ✅ important: ties it to the client
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
