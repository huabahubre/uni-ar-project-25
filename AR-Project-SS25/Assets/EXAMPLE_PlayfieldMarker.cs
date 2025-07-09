using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class EXAMPLE_PlayfieldMarker : MonoBehaviour
{
    public bool updateMyPosition = true;
    
    
    [Button]
    public void IamTrackedNow()
    {
        Debug.Log("Playfield marker is now tracked.");
        PlayfieldManagement.Instance.OnPlayfieldTracked();
    }
    
    [Button]
    public void IamLost()
    {
        Debug.Log("Playfield marker tracking lost.");
        PlayfieldManagement.Instance.OnLostPlayfieldTracking();
    }

    private void Update()
    {
        if(updateMyPosition)
            PlayfieldManagement.Instance.UpdatePlayfieldPosition( transform.position,
            transform.rotation);
    }
}
