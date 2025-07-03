using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class TestButton : MonoBehaviour
{
    [SerializeField] int damage = 10;
    public Button endTurnButton;
    public TMP_Text turnStatusText; // or use `public Text turnStatusText;`
    public TMP_Text healthStatus; // or use `public Text turnStatusText;`

    void Start()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsSpawned)
        {
            SetupListeners();
            UpdateUI(GameStateManager.Instance.activePlayerClientId.Value);
        }
        else
        {
            // Retry setup if GameStateManager isn't ready yet
            Invoke(nameof(Start), 0.2f);
        }
    }

    void SetupListeners()
    {
        GameStateManager.Instance.activePlayerClientId.OnValueChanged += (_, newActiveClientId) =>
        {
            UpdateUI(newActiveClientId);
        };
        
        GameStateManager.Instance.enemyHealth.OnValueChanged += (_, newHealth) =>
        {
            if (healthStatus != null)
            {
                healthStatus.text = "Enemy Health: " + newHealth;
            }
        };
    }

    void UpdateUI(ulong newActiveClientId)
    {
        bool isMyTurn = NetworkManager.Singleton.LocalClientId == newActiveClientId;

        endTurnButton.interactable = isMyTurn;

        if (turnStatusText != null)
        {
            turnStatusText.text = isMyTurn ? "Your Turn!" : "Waiting...";
            turnStatusText.color = isMyTurn ? Color.green : Color.gray;
        }
    }

    // public void OnEndTurnClicked()
    // {
    //     if (GameStateManager.Instance != null)
    //     {
    //         Debug.Log("Could execute EndTurnRequestServerRpc with damage: " + damage);
    //         GameStateManager.Instance.EndTurnRequestServerRpc(damage);
    //     }
    //
    //     endTurnButton.interactable = false; // Prevent spam click until turn updates
    // }
    
    public void OnEndTurnClicked()
    {
        GameStateManager.Instance.Health = 50;
    }
    
}
