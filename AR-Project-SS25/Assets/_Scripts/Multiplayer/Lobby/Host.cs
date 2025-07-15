using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies;
using TMPro;
using UnityEditor;
using Unity.Netcode.Transports.UTP;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Unity.Services.Lobbies.Models;

public class HostLobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyCodeText;

    private void Start()
    {
        string localIP = GetLocalIPAddress();
        lobbyCodeText.text = $"{localIP}";
    }

    public void OnHostButtonClicked()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("0.0.0.0", 7777);
        NetworkManager.Singleton.StartHost();

        MainCanvasManagement.Instance.StartLoading("Hosting Game...");

        // Manually show lobby page
        FindObjectOfType<CanvasPage_Lobby>()?.OnJoinedLobby();
    }

    private string GetLocalIPAddress()
    {
        foreach (var ip in System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return "IP not found";
    }
}