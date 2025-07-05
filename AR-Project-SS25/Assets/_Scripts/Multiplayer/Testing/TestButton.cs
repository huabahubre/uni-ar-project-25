using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class TestButton : MonoBehaviour
{
    [SerializeField] int damage = 10;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TMP_Text turnStatusText;
    [SerializeField] private TMP_Text healthStatusPlayer1;
    [SerializeField] private TMP_Text healthStatusPlayer2;

    void Start()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsSpawned)
        {
            SetupListeners();
            UpdateButtonUI(GameStateManager.Instance.activePlayerClientId.Value);
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
            UpdateButtonUI(newActiveClientId);
        };
        
        GameStateManager.Instance.player1HP.OnValueChanged += (_, newHealth) =>
        {
            UpdateHealthUI(newHealth, healthStatusPlayer1);
        };
        GameStateManager.Instance.player2HP.OnValueChanged += (_, newHealth) =>
        {
            UpdateHealthUI(newHealth, healthStatusPlayer2);
        };
    }

    void UpdateHealthUI(int newHealth, TMP_Text healthStatusText)
    {
        if (healthStatusText != null)
        {
            string player = healthStatusText == healthStatusPlayer1 ? "Player 1" : "Player 2";
            healthStatusText.text = player + ": " + newHealth;
        }
    }
    
    void UpdateButtonUI(ulong newActiveClientId)
    {
        bool isMyTurn = NetworkManager.Singleton.LocalClientId == newActiveClientId;

        endTurnButton.interactable = isMyTurn;

        if (turnStatusText != null)
        {
            turnStatusText.text = isMyTurn ? "Your Turn!" : "Waiting...";
            turnStatusText.color = isMyTurn ? Color.green : Color.gray;
        }
    }

    public void OnEndTurnClicked()
    {
        if (GameStateManager.Instance != null)
        {
            Debug.Log("Trying EndTurnRequestServerRpc with damage: " + damage);
            GameStateManager.Instance.EndTurnRequestServerRpc(damage);
        }
    
        endTurnButton.interactable = false; // Prevent spam click until turn updates
    }
    
}
