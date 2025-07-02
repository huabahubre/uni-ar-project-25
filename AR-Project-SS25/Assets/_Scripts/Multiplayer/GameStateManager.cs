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
    public NetworkVariable<bool> isServerTurn = new NetworkVariable<bool>(
        true, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    public ulong player1ClientId;

    void Awake()
    {
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
        player1HP.OnValueChanged += OnPlayer1HealthChanged;
        player2HP.OnValueChanged += OnPlayer2HealthChanged;
        isServerTurn.OnValueChanged += OnIsServerTurnChanged;
        if (IsServer)
        {
            isServerTurn.Value = true;
            player1ClientId = NetworkManager.Singleton.LocalClientId;
        }
    }

    [ServerRpc]
    public void EndTurnRequestServerRpc(int damage, ServerRpcParams rpcParams = default)
    {
        ulong requestingClientId = rpcParams.Receive.SenderClientId;

        bool isCurrentPlayer = isServerTurn.Value
            ? requestingClientId == player1ClientId
            : requestingClientId != player1ClientId;

        if (!isCurrentPlayer)
        {
            Debug.LogWarning($"Client {requestingClientId} tried to act out of turn.");
            return;
        }

        UpdateHealth(damage, isServerTurn.Value);
        UpdateTurn();
        CheckWinCondition();
    }

    private void UpdateHealth(int damage, bool isServer)
    {
        if (isServer)
        {
            // Player 1 is attacking → damage Player 2
            player2HP.Value = Mathf.Max(0, player2HP.Value - damage);
        }
        else
        {
            // Player 2 is attacking → damage Player 1
            player1HP.Value = Mathf.Max(0, player1HP.Value - damage);
        }
    }
    
    private void UpdateTurn()
    {
        isServerTurn.Value = !isServerTurn.Value;
    }
    
    private void CheckWinCondition()
    {
        if (player1HP.Value <= 0)
            Debug.Log("Player 2 wins!");
            // Update UI, etc.
        else if (player2HP.Value <= 0)
            // Update UI, etc.
            Debug.Log("Player 1 wins!");
    }

    
    private void OnPlayer1HealthChanged(int oldValue, int newValue)
    {
        // Update UI, etc.
    }
    
    private void OnPlayer2HealthChanged(int oldValue, int newValue)
    {
        // Update UI, etc.
    }

    private void OnIsServerTurnChanged(bool oldValue, bool newValue)
    {
        // Update UI, etc.
    }


}
