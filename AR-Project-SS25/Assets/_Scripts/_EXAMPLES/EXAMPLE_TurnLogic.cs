using System;
using UnityEngine;

public class EXAMPLE_TurnLogic : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            PlayfieldManagement.Instance.OnPlayfieldTracked();
        }
        
    }
}
