using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies.Models;

public class JoinLobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text displayCode;

    public void OnJoinButtonClicked()
    {
        string ip = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(ip))
        {
            Debug.Log("Please enter a valid IP address.");
            return;
        }

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, 7777);
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.StartClient();

        MainCanvasManagement.Instance.StartLoading("Connecting to host...");
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Successfully connected to host!");
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

            // ✅ Trigger your Canvas logic
            FindObjectOfType<CanvasPage_Lobby>()?.OnJoinedLobby();
        }
    }
}