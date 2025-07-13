using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies;
using TMPro;
using UnityEditor;

public class HostLobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyCodeText;
    // [SerializeField] private TMP_Text sessionNameText;

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void OnHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
        
        // Start loading
        MainCanvasManagement.Instance.StartLoading("Hosting Lobby...");

        // TODO: fix compile error
        /*string guidSessionName = GUID.Generate().ToString();
        
        var lobby = await LobbyService.Instance.CreateLobbyAsync(
            guidSessionName,
            2
        );

        lobbyCodeText.text = lobby.LobbyCode;
        
        FindObjectOfType<CanvasPage_Lobby>()?.OnJoinedLobby(lobby);*/
    }
}