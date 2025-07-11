using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class NetworkStartup : MonoBehaviour
{
    [InfoBox("Put here all Networked prefabs, that should be spawned by the host/server on startup.")]
    public List<GameObject> networkedPrefabs;

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
    }

    private void OnServerStarted()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            foreach (var prefab in networkedPrefabs)
            {
                GameObject go = Instantiate(prefab);
                go.GetComponent<NetworkObject>().Spawn();
                Debug.Log($"Spawned: {go.name}");
            }
        }
        
        // ✅ Destroy this object after spawning is complete
        Destroy(gameObject);
        Debug.Log("NetworkStartup destroyed after spawning objects.");
    }
}