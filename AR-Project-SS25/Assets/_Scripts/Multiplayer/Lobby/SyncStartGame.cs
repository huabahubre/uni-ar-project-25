using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class SyncStartGame : NetworkBehaviour
{
    public static SyncStartGame Instance;
    [SerializeField] private NetworkVariable<bool> _gameStarted = new NetworkVariable<bool>(false);
    
    [BoxGroup("GameObjects")]
    [SerializeField] private List<GameObject> managerObjects = new List<GameObject>();
    
    public bool gameStarted
    {
        set
        {
            if (IsHost)
            {
                _gameStarted.Value = value;
            }
            else
            {
                Debug.LogWarning("Only the host can set GameStarted.");
            }
        }
    }
    
    private void Awake()
    {
        Debug.Log("Initializing SyncStartGame");
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
        _gameStarted.OnValueChanged += OnGameStartedChanged;
    }

    public override void OnDestroy()
    {
        _gameStarted.OnValueChanged -= OnGameStartedChanged;
        base.OnDestroy();
    }
    
    private void OnGameStartedChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            // This will be called on the server to start the game
            Debug.Log("Game was started on the server. Changing UI and activating managers.");
            MainCanvasManagement.Instance.ShowPage("Gameplay");
            foreach (var manager in managerObjects)
            {
                manager.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Game state changed to has not started yet. This shouldn't happen in normal flow.");
        }
    }
}
