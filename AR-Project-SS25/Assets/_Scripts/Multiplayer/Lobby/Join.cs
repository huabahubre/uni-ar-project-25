using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class JoinLobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text displayCode;

    public async void OnJoinButtonClicked()
    {
        string code = joinCodeInput.text;
        if (string.IsNullOrEmpty(code))
        {
            Debug.Log("Please enter a valid code");
            return;
        }
        
        displayCode.text = "Joined lobby: " + code;
        await LobbyService.Instance.JoinLobbyByCodeAsync(code);
        NetworkManager.Singleton.StartClient();
    }
}