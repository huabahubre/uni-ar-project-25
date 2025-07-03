using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class JoinLobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeInput;

    public async void OnJoinButtonClicked()
    {
        string code = joinCodeInput.text;
        if (string.IsNullOrEmpty(code))
            return;

        await LobbyService.Instance.JoinLobbyByCodeAsync(code);
        NetworkManager.Singleton.StartClient();
    }
}