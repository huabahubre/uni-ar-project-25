using System;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class PlayerState : NetworkBehaviour
{
    public static PlayerState LocalPlayer;
    public static PlayerState EnemyPlayer;

    // Inspector exposed variables
    [SerializeField, BoxGroup("Runtime Player Variables")] private bool isLocalPlayer;
    
    
    
    
    // Networked Variables --> Only from LocalPlayer
    [BoxGroup("Runtime Player Variables")]
    public NetworkVariable<int> PlayerHealth = new NetworkVariable<int>(100, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    [BoxGroup("Runtime Player Variables")]
    public NetworkVariable<int> ElementIndex = new();
    
    [BoxGroup("Runtime Player Variables")]
    public NetworkVariable<FixedString32Bytes> PlayerName = new();

    [BoxGroup("Runtime Player Variables")]
    public NetworkVariable<bool> IsShieldActive = new();
    
    [BoxGroup("Runtime Player Variables")]
    public NetworkVariable<int> ActiveShieldElement = new();
    
    public static event Action<PlayerState> OnPlayerStateUpdated;
    public static event Action<PlayerState> OnEnemyJoined;

    
    private void Start()
    {
        // Read
        // int myElement = PlayerState.LocalPlayer.ElementIndex.Value;
        // int enemyElement = PlayerState.EnemyPlayer.ElementIndex.Value;
    }


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalPlayer = this;
            isLocalPlayer = true;
            this.gameObject.name = "[Local] Player";
            Debug.Log("LocalPlayer spawned");
        }
        else
        {
            EnemyPlayer = this;
            isLocalPlayer = false;
            this.gameObject.name = "[Remote] Player";
            Debug.Log("EnemyPlayer spawned");
        }
        
        // Subscribe callback to changes
        ElementIndex.OnValueChanged += (_, _) => OnPlayerStateUpdate();
        PlayerName.OnValueChanged += (_, _) => OnPlayerStateUpdate();
        PlayerHealth.OnValueChanged += (_, _) => OnPlayerStateUpdate();
        IsShieldActive.OnValueChanged += (_, _) => OnPlayerStateUpdate();

        // Init local player
        InitLocalPlayerData();
        
        if(!isLocalPlayer)
            OnEnemyJoined?.Invoke(this);
    }


    void OnPlayerStateUpdate()
    {
        // Debug.Log($"[PlayerState] {(isLocalPlayer ? "Local::" : "Remote::")} Player state updated!");
        
        OnPlayerStateUpdated?.Invoke(this);
    }
    
    
    
    void InitLocalPlayerData()
    {
        SetElementIndexServerRpc(0);
        SetPlayerNameServerRpc("?");
    }
    
    
    
    [Button]
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerHealthServerRpc(int diff)
    {
        int currentHealth = PlayerHealth.Value;
        currentHealth += diff;
        
        // Ensure health does not go below 0
        currentHealth = Mathf.Max(0, currentHealth);
        PlayerHealth.Value = currentHealth;
    }
    
    
    [Button]
    [ServerRpc(RequireOwnership = false)]
    public void SetElementIndexServerRpc(int idx)
    {
        ElementIndex.Value = idx;
    }
    
    
    [Button]
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerNameServerRpc(string name)
    {
        PlayerName.Value = name;
    }
}

