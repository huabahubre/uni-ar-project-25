using Unity.Netcode;
using UnityEngine;

public class Sync : NetworkBehaviour
{
    public NetworkVariable<float> health = new NetworkVariable<float>(1);
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        // Done when player prefab is spawned
        // Init stuff
        base.OnNetworkSpawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncHealthServerRpc(float newHealth)
    {
        health.Value = newHealth;
    }
}


// RPC (Broadcast)
// Events that send from client to server (i want to update that variable with my value)