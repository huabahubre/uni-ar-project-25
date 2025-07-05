using Sirenix.OdinInspector;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;

public class CanvasPage_Lobby : CanvasPage
{
    
    [BoxGroup("References"), Header("Buttons")]
    public Button Button_Back;
    
    [BoxGroup("References")]
    public Button Button_StartGame;
    
    
    
    [BoxGroup("References"), Header("Panels")]
    public GameObject Panel_Actions;

    [BoxGroup("References")]
    public GameObject Panel_ActiveLobby;
    

    
    public override void Initialize()
    {
        Button_Back.onClick.AddListener(OnBackButtonClick);
        Button_StartGame.onClick.AddListener(OnStartGame);
        
        base.Initialize();
    }

    public override void OnShow()
    {
        Panel_Actions.SetActive(true);
        Panel_ActiveLobby.SetActive(false);
        
        base.OnShow();
    }
    
    

    void OnBackButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Menu");
    }

    
    
    public void OnJoinedLobby(Lobby lobby)
    {
        Panel_Actions.SetActive(false);
        Panel_ActiveLobby.SetActive(true);
    
        // Set StartButton based on Host status
        Button_StartGame.gameObject.SetActive(lobby.HostId == AuthenticationService.Instance.PlayerId);
    }


    public void OnStartGame()
    {
        Debug.Log("Starting game...");
        MainCanvasManagement.Instance.ShowPage("Gameplay");
    }
}
