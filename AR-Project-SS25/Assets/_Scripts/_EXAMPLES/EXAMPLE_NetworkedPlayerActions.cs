using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

#if NEW_INPUT_SYSTEM_INSTALLED
using UnityEngine.InputSystem;
#endif

[InfoBox("SPACE: Emulates playfield marker is now tracked.\nESCAPE: Emulates playfield marker tracking lost.\n\nThis script is only for testing purposes and should not be used in production builds!")]
public class EXAMPLE_NetworkedPlayerActions : NetworkBehaviour
{
    
    void Update()
    {
        if (!IsOwner || !IsSpawned) return;

        
#if ENABLE_INPUT_SYSTEM && NEW_INPUT_SYSTEM_INSTALLED

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Debug.Log("[DEBUG] Emulating playfield marker is now tracked.");
                PlayfieldManagement.Instance.OnPlayfieldTracked();
            }
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("[DEBUG] Emulating playfield marker tracking lost.");
                PlayfieldManagement.Instance.OnLostPlayfieldTracking();
            }
#else
        // Old input backends are enabled.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[DEBUG] Emulating playfield marker is now tracked.");
            PlayfieldManagement.Instance.OnPlayfieldTracked();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[DEBUG] Emulating playfield marker tracking lost.");
            PlayfieldManagement.Instance.OnLostPlayfieldTracking();
        }
        
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("[DEBUG] Emulating local player lost HP.");
            
            PlayerState.LocalPlayer.UpdatePlayerHealthServerRpc(-1);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("[DEBUG] Emulating remote player lost HP.");
            
            PlayerState.EnemyPlayer.UpdatePlayerHealthServerRpc(-1);
        }
#endif
    }
}

