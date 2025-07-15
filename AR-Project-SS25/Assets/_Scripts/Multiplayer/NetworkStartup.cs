using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class NetworkStartup : MonoBehaviour
{
    [InfoBox("Put here all Networked prefabs, that should be spawned by the host/server on startup.")]
    public List<GameObject> networkedPrefabs;

    [InfoBox("These objects will be only enabled for player 1 at network startup.")]
    public List<GameObject> player1TrackedObjects;

    [InfoBox("These objects will be only enabled for player 2 at network startup.")]
    public List<GameObject> player2TrackedObjects;

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("❌ No NetworkManager found.");
            return;
        }

        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnServerStarted()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            foreach (var prefab in networkedPrefabs)
            {
                GameObject go = Instantiate(prefab);
                go.GetComponent<NetworkObject>().Spawn();
                Debug.Log($"[Server] Spawned: {go.name}");
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // This ensures this logic only runs for the local player
            HandleLocalPlayerSetup();
        }
    }

    private void HandleLocalPlayerSetup()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;

        Debug.Log($"🔄 Handling local player setup for ClientId: {localId}");

        if (localId == 0)
        {
            SetActiveForList(player1TrackedObjects, true);
            SetActiveForList(player2TrackedObjects, false);
        }
        else if (localId == 1)
        {
            SetActiveForList(player1TrackedObjects, false);
            SetActiveForList(player2TrackedObjects, true);
        }
        else
        {
            SetActiveForList(player1TrackedObjects, false);
            SetActiveForList(player2TrackedObjects, false);
        }

        // Optional: destroy this object after setup
        Destroy(gameObject);
    }

    private void SetActiveForList(List<GameObject> objects, bool isActive)
    {
        foreach (var obj in objects)
        {
            if (obj != null)
                obj.SetActive(isActive);
        }
    }
}
