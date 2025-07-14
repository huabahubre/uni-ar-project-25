using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CanvasPage_GameOver : CanvasPage
{
    [BoxGroup("References"), Header("Buttons")]
    public Button Button_Exit;
    
    [BoxGroup("References")]
    public Button Button_Rematch;
    
    [BoxGroup("References")]
    public Button Button_Menu;
    
    
    
    [BoxGroup("References"), Header("Panels")]
    public GameObject Panel_Win;

    [BoxGroup("References")]
    public GameObject Panel_Lose;
    
    [BoxGroup("References")]
    public GameObject Panel_AcceptRematch;
    
    [BoxGroup("References")]
    public GameObject Panel_WaitingForRematch;
    
    
    
    public override void Initialize()
    {
        Button_Exit.onClick.AddListener(OnExitButtonClick);
        Button_Menu.onClick.AddListener(OnMenuButtonClick);
        Button_Rematch.onClick.AddListener(OfferRematch);
        
        base.Initialize();
    }

    public override void OnShow()
    {
        // Hide all panels initially
        Panel_Win.SetActive(false);
        Panel_Lose.SetActive(false);
        Panel_AcceptRematch.SetActive(false);
        Panel_WaitingForRematch.SetActive(false);
        
        // Set Win or Loose panel based on game result
        Panel_Win.SetActive(GameStateManager.Instance.IsLocalPlayerWinner());
        Panel_Lose.SetActive(!GameStateManager.Instance.IsLocalPlayerWinner());
        
        // Subscribe to rematch events
        GameStateManager.OnRematchOffered += OnOfferedRematch;
        
        base.OnShow();
    }


    void OnMenuButtonClick()
    {
        MainCanvasManagement.Instance.ShowPage("Menu");
    }

    
    void OnExitButtonClick()
    {
        Application.Quit();
    }

    
    #region Rematch
    
    
    public void OfferRematch()
    {
        Panel_WaitingForRematch.SetActive(true);
        DataManagement.Instance.isRematchLobby = true;
        GameStateManager.Instance.RequestRematchServerRpc();
    }

    
    public void OnOfferedRematch()
    {
        Panel_WaitingForRematch.SetActive(false);
        Panel_AcceptRematch.SetActive(true);
    }
    
    
    public void AcceptRematchOffer()
    {
        DataManagement.Instance.isRematchLobby = true;
        GameStateManager.Instance.RequestRematchServerRpc();
    }

    public void CancelRematchOffer()
    {
        GameStateManager.Instance.CancelRematchServerRpc();
        
        DataManagement.Instance.isRematchLobby = false;
        Panel_WaitingForRematch.SetActive(false);
        Panel_AcceptRematch.SetActive(false);
    }
    
    
    #endregion
    

}
