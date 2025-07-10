using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

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

            if (Keyboard.current.spaceKey.isPressed)
            {
                Debug.Log("[DEBUG] Emulating playfield marker is now tracked.");
                PlayfieldManagement.Instance.OnPlayfieldTracked();
            }
            if (Keyboard.current.escapeKey.isPressed)
            {
                Debug.Log("[DEBUG] Emulating playfield marker tracking lost.");
                PlayfieldManagement.Instance.OnLostPlayfieldTracking();
            }
#else
        // Old input backends are enabled.
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("[DEBUG] Emulating playfield marker is now tracked.");
            PlayfieldManagement.Instance.OnPlayfieldTracked();
        }
        
        if (Input.GetKey(KeyCode.Escape))
        {
            Debug.Log("[DEBUG] Emulating playfield marker tracking lost.");
            PlayfieldManagement.Instance.OnLostPlayfieldTracking();
        }
#endif
    }
}

